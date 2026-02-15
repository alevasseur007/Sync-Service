using GithubSync.Infrastructure.Persistence;
using GithubSync.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace GithubSync.Application.Sync
{
    public class EFSyncStateRepository : ISyncStateRepository
    {
        private readonly AppDbContext db;

        public EFSyncStateRepository(AppDbContext pDb) => db = pDb;

        public async Task<DateTimeOffset?> GetWatermarkAsync(string repository, CancellationToken ct)
        {
            var state = await db.SyncStates.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Repository == repository, ct);

            return state?.LastSeenUpdatedAt;
        }

        public async Task MarkSuccessAsync(
        string repository,
        DateTimeOffset finishedAtUtc,
        DateTimeOffset? watermarkAfter,
        CancellationToken ct)
        {
            var state = await db.SyncStates
                .SingleOrDefaultAsync(x => x.Repository == repository, ct);

            if (state is null)
            {
                state = new SyncState { Repository = repository };
                db.SyncStates.Add(state);
            }

            state.LastSuccessfulSyncAt = finishedAtUtc.UtcDateTime;
            state.LastAttemptAt = DateTime.UtcNow;
            state.LastRunStatus = "Success";
            state.LastError = null;

            if (watermarkAfter is not null)
            {
                if (state.LastSeenUpdatedAt is null || watermarkAfter > state.LastSeenUpdatedAt)
                    state.LastSeenUpdatedAt = watermarkAfter?.UtcDateTime;
            }

            await db.SaveChangesAsync(ct);
        }

        public async Task MarkFailureAsync(
            string repository,
            DateTimeOffset finishedAtUtc,
            string error,
            CancellationToken ct)
        {
            var state = await db.SyncStates
                .SingleOrDefaultAsync(x => x.Repository == repository, ct);

            if (state is null)
            {
                state = new SyncState { Repository = repository };
                db.SyncStates.Add(state);
            }

            state.LastRunStatus = "Failed";
            state.LastError = Truncate(error, 2000);

            state.LastAttemptAt = DateTime.UtcNow;

            await db.SaveChangesAsync(ct);
        }

        private static string Truncate(string s, int max)
            => s.Length <= max ? s : s[..max];
    }
}
