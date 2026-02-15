namespace GithubSync.Application.Github
{
    public sealed record GithubIssueDTO
    (
        long Id,
        int Number,
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
