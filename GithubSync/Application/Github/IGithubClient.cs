namespace GithubSync.Application.Github
{
    public interface IGithubClient
    {
        Task<IReadOnlyList<GithubIssueDTO>> ListIssuesAsync(
            string repository,
            DateTimeOffset? since,
            CancellationToken cancellationToken = default);
    }
}
