using System.Collections.Concurrent;

namespace GithubSync.Application.Sync.Deprecated
{
    //DEPRECATED: Was used to test in memory before going into EF and SQLite persistence
    public class InMemorySyncStateRepo
    {
        public sealed class InMemorySyncStateRepository //: ISyncStateRepository
        {
            private readonly ConcurrentDictionary<string, DateTimeOffset> _watermarks = new();

            public Task<DateTimeOffset?> GetWatermarkAsync(string repository, CancellationToken ct)
            {
                return Task.FromResult(_watermarks.TryGetValue(repository, out var wm) ? (DateTimeOffset?)wm : null);
            }

            public Task SetWatermarkAsync(string repository, DateTimeOffset watermark, CancellationToken ct)
            {
                _watermarks[repository] = watermark;
                return Task.CompletedTask;
            }
        }
    }
}
