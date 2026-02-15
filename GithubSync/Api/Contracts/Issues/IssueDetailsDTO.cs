namespace GithubSync.Api.Contracts.Issues
{
    public sealed record IssueDetailsDTO(
        int Number,
        long GitHubIssueId,
        string Repository,
        string Title,
        string? Body,
        string State,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        DateTimeOffset? ClosedAt,
        string HtmlUrl,
        string AuthorLogin,
        string? AssigneeLogin,
        int CommentsCount,
        IReadOnlyList<string> Labels
    );
}
