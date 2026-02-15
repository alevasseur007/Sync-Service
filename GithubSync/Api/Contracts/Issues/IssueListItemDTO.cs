namespace GithubSync.Api.Contracts.Issues
{
    public sealed record IssueListItemDTO(
        int Number,
        string Title,
        string State,
        DateTimeOffset UpdatedAt,
        string HtmlUrl,
        string AuthorLogin,
        string? AssigneeLogin,
        int CommentsCount,
        IReadOnlyList<string> Labels
    );
}
