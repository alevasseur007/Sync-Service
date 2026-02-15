using GithubSync.Application.Github;
using GithubSync.Application.Issues;

namespace GithubSync.Application.Sync
{
    public class SyncIssuesUseCase : ISyncIssues
    {
        private readonly ILogger<SyncIssuesUseCase> _logger;
        private readonly IGithubClient gitClient;
        private readonly IIssueRepository issueRepository;
        private readonly ISyncStateRepository syncStateRepository;

        public SyncIssuesUseCase(
        IGithubClient pGitClient,
        IIssueRepository pIssues,
        ISyncStateRepository pSyncState,
        ILogger<SyncIssuesUseCase> logger)
        {
            gitClient = pGitClient;
            issueRepository = pIssues;
            syncStateRepository = pSyncState;
            _logger = logger;
        }

        public async Task<SyncResult> RunAsync(CancellationToken ct)
        {
            DateTimeOffset started = DateTimeOffset.UtcNow;

            // In a later step, read repository from options passed into this use-case.
            // For now, we'll keep repository injected via options in Infrastructure or through DI factory.
            // But simplest: the GitHub client will be configured with the repo in options and passed here separately.
            throw new InvalidOperationException("Repository must be provided via a decorator or options-bound wrapper.");
        }

        public async Task<SyncResult> RunForRepoAsync(string repository, CancellationToken ct)
        {
            var syncRunId = Guid.NewGuid().ToString("n");
            var started = DateTimeOffset.UtcNow;
            DateTimeOffset? watermarkBefore = null;
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["syncRunId"] = syncRunId,
                ["repository"] = repository
            });

            try
            {
                _logger.LogInformation("Sync started. watermarkBefore={WatermarkBefore}", watermarkBefore);

                watermarkBefore = await syncStateRepository.GetWatermarkAsync(repository, ct);
                var effectiveSince = watermarkBefore is null
                    ? DateTimeOffset.UtcNow.AddDays(-7)
                    : watermarkBefore.Value.AddSeconds(1);

                var ghIssues = await gitClient.ListIssuesAsync(repository, effectiveSince, ct);

                _logger.LogInformation("Fetched {Count} issues from GitHub.", ghIssues.Count);

                var outcome = await issueRepository.UpsertBatchAsync(repository, ghIssues, ct);

                _logger.LogInformation("Upsert done. inserted={Inserted} updated={Updated} unchanged={Unchanged}",
                outcome.Inserted, outcome.Updated, outcome.Unchanged);

                var watermarkAfter = ghIssues.Count == 0
                    ? watermarkBefore
                    : ghIssues.Max(i => i.UpdatedAt);

                var finished = DateTimeOffset.UtcNow;
                await syncStateRepository.MarkSuccessAsync(repository, finished, watermarkAfter, ct);
                _logger.LogInformation("Sync completed. watermarkAfter={WatermarkAfter}", watermarkAfter);

                return new SyncResult(
                    Repository: repository,
                    StartedAt: started,
                    FinishedAt: finished,
                    Inserted: outcome.Inserted,
                    Updated: outcome.Updated,
                    Unchanged: outcome.Unchanged,
                    WatermarkBefore: watermarkBefore,
                    WatermarkAfter: watermarkAfter,
                    Status: "Success",
                    Error: null
                );
            }
            catch (Exception ex)
            {
                var finished = DateTimeOffset.UtcNow;

                try
                {
                    await syncStateRepository.MarkFailureAsync(repository, finished, ex.Message, ct);
                }
                catch { }

                _logger.LogError(ex, "Sync failed.");

                return new SyncResult(
                    Repository: repository,
                    StartedAt: started,
                    FinishedAt: finished,
                    Inserted: 0,
                    Updated: 0,
                    Unchanged: 0,
                    WatermarkBefore: watermarkBefore,
                    WatermarkAfter: watermarkBefore,
                    Status: "Failed",
                    Error: ex.Message
                );
            }
        }
    }
}
