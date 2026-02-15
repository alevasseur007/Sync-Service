namespace GithubSync.Application.Sync
{
    public interface ISyncIssues
    {
        Task<SyncResult> RunAsync(CancellationToken cancellationToken = default);
    }
}
