using GithubSync.Application.Github;

namespace GithubSync.Application.Issues.Deprecated
{
    //DEPRECATED: Was used to test in memory before going into EF and SQLite persistence
    public class NullIssueRepository : IIssueRepository
    {
        public Task<UpsertOutcome> UpsertBatchAsync(string repository, IReadOnlyList<GithubIssueDTO> issues, CancellationToken ct)
        {
            // For this step, just pretend everything is "unchanged"
            return Task.FromResult(new UpsertOutcome(Inserted: 0, Updated: 0, Unchanged: issues.Count));
        }
    }
}
