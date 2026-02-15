using System.Text.Json.Serialization;

namespace GithubSync.Application.Github
{
    public class GithubIssueModel
    {
        [JsonPropertyName("id")] public long Id { get; set; }
        [JsonPropertyName("number")] public int Number { get; set; }
        [JsonPropertyName("title")] public string? Title { get; set; }
        [JsonPropertyName("body")] public string? Body { get; set; }
        [JsonPropertyName("state")] public string? State { get; set; }
        [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; set; }
        [JsonPropertyName("updated_at")] public DateTimeOffset UpdatedAt { get; set; }
        [JsonPropertyName("closed_at")] public DateTimeOffset? ClosedAt { get; set; }
        [JsonPropertyName("html_url")] public string? HtmlUrl { get; set; }
        [JsonPropertyName("comments")] public int Comments { get; set; }

        [JsonPropertyName("user")] public UserModel? User { get; set; }
        [JsonPropertyName("assignee")] public UserModel? Assignee { get; set; }

        [JsonPropertyName("labels")] public List<LabelModel>? Labels { get; set; }

        [JsonPropertyName("pull_request")] public object? PullRequest { get; set; }
    }

    public sealed class UserModel
    {
        [JsonPropertyName("login")] public string? Login { get; set; }
    }

    public sealed class LabelModel
    {
        [JsonPropertyName("name")] public string Name { get; set; } = "";
    }
}
