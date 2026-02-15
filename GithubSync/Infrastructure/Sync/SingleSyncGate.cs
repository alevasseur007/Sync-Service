namespace GithubSync.Infrastructure.Sync
{
    public sealed class SingleSyncGate
    {
        private readonly SemaphoreSlim _sem = new(1, 1);

        public async Task<bool> TryEnterAsync(CancellationToken ct)
            => await _sem.WaitAsync(0, ct);

        public void Exit() => _sem.Release();
    }
}
