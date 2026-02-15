using GithubSync.Application.Github;

namespace GithubSync.Application.Issues
{
    public interface IIssueRepository
    {
        Task<UpsertOutcome> UpsertBatchAsync(string repository, IReadOnlyList<GithubIssueDTO> issues, CancellationToken ct);
    }

    public sealed record UpsertOutcome(int Inserted, int Updated, int Unchanged);
}
