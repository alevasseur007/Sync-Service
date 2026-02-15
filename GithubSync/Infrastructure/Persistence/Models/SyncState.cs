namespace GithubSync.Infrastructure.Persistence.Models
{
    public sealed class SyncState
    {
        public long Id { get; set; }
        public required string Repository { get; set; }

        public DateTime? LastSuccessfulSyncAt { get; set; }
        public DateTime? LastSeenUpdatedAt { get; set; }
        public DateTime? LastAttemptAt { get; set; }

        public string? LastRunStatus { get; set; } // "Success"/"Failed"
        public string? LastError { get; set; }
    }
}
