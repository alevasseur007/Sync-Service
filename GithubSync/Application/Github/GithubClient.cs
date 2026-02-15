using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace GithubSync.Application.Github
{
    public class GithubClient : IGithubClient
    {
        private readonly HttpClient _http;
        private readonly GithubOptions _options;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public GithubClient(HttpClient http, IOptions<GithubOptions> options)
        {
            _http = http;
            _options = options.Value;
        }

        public async Task<IReadOnlyList<GithubIssueDTO>> ListIssuesAsync(
            string repository,
            DateTimeOffset? since,
            CancellationToken ct)
        {
            // repository format: owner/repo
            var parts = repository.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length != 2)
                throw new ArgumentException("Repository must be in 'owner/repo' format.", nameof(repository));

            var owner = parts[0];
            var repo = parts[1];

            var results = new List<GithubIssueDTO>(capacity: 256);
            const int perPage = 100;
            var page = 1;

            while (true)
            {
                var url = $"/repos/{owner}/{repo}/issues?state=all&per_page={perPage}&page={page}";
                if (since is not null)
                {
                    url += $"&since={Uri.EscapeDataString(since.Value.UtcDateTime.ToString("O"))}";
                }

                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                // auth
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.Token);
                req.Headers.UserAgent.ParseAdd("GithubIssuesMirror/1.0"); // required by GitHub
                req.Headers.Accept.ParseAdd("application/vnd.github+json");
                req.Headers.Add("X-GitHub-Api-Version", "2022-11-28");

                using var resp = await _http.SendAsync(req, ct);
                if (!resp.IsSuccessStatusCode)
                {
                    var body = await resp.Content.ReadAsStringAsync(ct);
                    throw new HttpRequestException($"GitHub API error {(int)resp.StatusCode}: {body}");
                }

                await using var stream = await resp.Content.ReadAsStreamAsync(ct);
                var pageItems = await JsonSerializer.DeserializeAsync<List<GithubIssueModel>>(stream, JsonOptions, ct)
                                ?? new List<GithubIssueModel>();

                // Filter out PRs: PRs have pull_request object
                var issuesOnly = pageItems.Where(x => x.PullRequest is null).ToList();

                results.AddRange(issuesOnly.Select(Map));

                if (pageItems.Count < perPage)
                    break;

                page++;
            }

            return results;
        }

        private static GithubIssueDTO Map(GithubIssueModel m)
        {
            var labels = m.Labels?.Select(l => l.Name).Where(n => !string.IsNullOrWhiteSpace(n)).ToList()
                         ?? new List<string>();

            return new GithubIssueDTO(
                Id: m.Id,
                Number: m.Number,
                Title: m.Title ?? "",
                Body: m.Body,
                State: m.State ?? "open",
                CreatedAt: m.CreatedAt,
                UpdatedAt: m.UpdatedAt,
                ClosedAt: m.ClosedAt,
                HtmlUrl: m.HtmlUrl ?? "",
                AuthorLogin: m.User?.Login ?? "",
                AssigneeLogin: m.Assignee?.Login,
                CommentsCount: m.Comments,
                Labels: labels
            );
        }
    }
}
