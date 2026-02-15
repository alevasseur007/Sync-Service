using GithubSync.Application.Github;
using GithubSync.Application.Sync;
using Microsoft.Extensions.Options;

namespace GithubSync.Infrastructure.Sync
{
    public class ScheduledSyncService : BackgroundService
    {
        private readonly ILogger<ScheduledSyncService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly GithubOptions _options;
        private readonly SingleSyncGate _gate;

        public ScheduledSyncService(
            ILogger<ScheduledSyncService> logger,
            IServiceScopeFactory scopeFactory,
            IOptions<GithubOptions> options,
            SingleSyncGate gate)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _options = options.Value;
            _gate = gate;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var interval = TimeSpan.FromMinutes(Math.Max(1, _options.SyncIntervalMinutes));

            _logger.LogInformation("Scheduled sync enabled. Interval={IntervalMinutes} repo={Repo}",
                interval.TotalMinutes, _options.Repository);

            // Small initial delay to let app start
            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (!await _gate.TryEnterAsync(stoppingToken))
                    {
                        _logger.LogInformation("Scheduled sync skipped (another sync is running).");
                    }
                    else
                    {
                        try
                        {
                            using var scope = _scopeFactory.CreateScope();
                            var sync = scope.ServiceProvider.GetRequiredService<ISyncIssues>();

                            _logger.LogInformation("Scheduled sync starting…");
                            var result = await sync.RunAsync(stoppingToken);

                            _logger.LogInformation("Scheduled sync done. status={Status} inserted={Inserted} updated={Updated} unchanged={Unchanged} durationMs={DurationMs}",
                                result.Status, result.Inserted, result.Updated, result.Unchanged,
                                (result.FinishedAt - result.StartedAt).TotalMilliseconds);
                        }
                        finally
                        {
                            _gate.Exit();
                        }
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // normal shutdown
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Scheduled sync failed.");
                    // no rethrow so host continues running
                }

                await Task.Delay(interval, stoppingToken);
            }
        }
    }
}
