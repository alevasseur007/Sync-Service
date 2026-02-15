using GithubSync.Application.Github;
using Microsoft.Extensions.Options;

namespace GithubSync.Application.Sync
{
    public class OptionsBoundSyncIssues : ISyncIssues
    {
        private readonly SyncIssuesUseCase _inner;
        private readonly GithubOptions _options;

        public OptionsBoundSyncIssues(SyncIssuesUseCase inner, IOptions<GithubOptions> options)
        {
            _inner = inner;
            _options = options.Value;
        }

        public Task<SyncResult> RunAsync(CancellationToken ct)
            => _inner.RunForRepoAsync(_options.Repository, ct);
    }
}
