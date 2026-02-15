namespace GithubSync.Application.Sync
{
    public interface ISyncStateRepository
    {
        Task<DateTimeOffset?> GetWatermarkAsync(string repository, CancellationToken ct);

        Task MarkSuccessAsync(
            string repository,
            DateTimeOffset finishedAtUtc,
            DateTimeOffset? watermarkAfter,
            CancellationToken ct);

        Task MarkFailureAsync(
            string repository,
            DateTimeOffset finishedAtUtc,
            string error,
            CancellationToken ct);
    }
}
