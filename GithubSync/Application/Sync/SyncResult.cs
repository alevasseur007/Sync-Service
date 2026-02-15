namespace GithubSync.Application.Sync
{
    public sealed record SyncResult(
        string Repository,
        DateTimeOffset StartedAt,
        DateTimeOffset FinishedAt,
        int Inserted,
        int Updated,
        int Unchanged,
        DateTimeOffset? WatermarkBefore,
        DateTimeOffset? WatermarkAfter,
        string Status,
        string? Error
    );
}
