using GithubSync.Application.Github;
using Microsoft.Extensions.Options;

namespace GithubSync.Application.Sync.Deprecated
{
    public class FakeSyncIssues : ISyncIssues
    {
        private readonly GithubOptions options;

        public FakeSyncIssues(IOptions<GithubOptions> pOptions) => options = pOptions.Value;
        public async Task<SyncResult> RunAsync(CancellationToken cancellationToken = default)
        {
            DateTimeOffset started = DateTimeOffset.UtcNow;
            await Task.Delay(250, cancellationToken);
            DateTimeOffset finished = DateTimeOffset.UtcNow;

            return new SyncResult(
                Repository: options.Repository,
                StartedAt: started,
                FinishedAt: finished,
                Inserted: 0,
                Updated: 0,
                Unchanged: 0,
                WatermarkBefore: null,
                WatermarkAfter: null,
                Status: "Success",
                Error:null
            );
        }
    }
}
