namespace GithubSync.Application.Sync
{
    public sealed record SyncStatusDTO(
        string Repository,
        DateTimeOffset? LastSuccessfulSyncAt,
        DateTimeOffset? LastSeenUpdatedAt,
        string? LastRunStatus,
        string? LastError
    );
}
