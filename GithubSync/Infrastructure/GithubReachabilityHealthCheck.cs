using GithubSync.Application.Github;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace GithubSync.Infrastructure
{
    public sealed class GitHubReachabilityHealthCheck : IHealthCheck
    {
        private readonly HttpClient _http;
        private readonly GithubOptions _options;

        public GitHubReachabilityHealthCheck(HttpClient http, IOptions<GithubOptions> options)
        {
            _http = http;
            _options = options.Value;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            //lightweight call to check status
            using var req = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/rate_limit");
            req.Headers.UserAgent.ParseAdd("GithubSync/1.0");
            req.Headers.Accept.ParseAdd("application/vnd.github+json");
            req.Headers.Add("X-GitHub-Api-Version", "2022-11-28");

            if (!string.IsNullOrWhiteSpace(_options.Token))
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.Token);

            try
            {
                using var resp = await _http.SendAsync(req, cancellationToken);
                if (resp.IsSuccessStatusCode)
                    return HealthCheckResult.Healthy("GitHub reachable");

                return HealthCheckResult.Degraded($"GitHub returned {(int)resp.StatusCode}");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Degraded("GitHub check failed", ex);
            }
        }
    }
}
