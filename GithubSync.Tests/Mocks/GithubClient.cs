using GithubSync.Application.Github;
using System;
using System.Collections.Generic;
using System.Text;

namespace GithubSync.Tests.Mocks
{
    internal class MockGithubClient : IGithubClient
    {
        private readonly IReadOnlyList<GithubIssueDTO> _issues;

        public MockGithubClient(IReadOnlyList<GithubIssueDTO> issues) => _issues = issues;

        public Task<IReadOnlyList<GithubIssueDTO>> ListIssuesAsync(string repository, DateTimeOffset? since, CancellationToken ct)
        {
            // Simulate "since" filtering
            var filtered = since is null
                ? _issues
                : _issues.Where(i => i.UpdatedAt >= since.Value).ToList();

            return Task.FromResult((IReadOnlyList<GithubIssueDTO>)filtered);
        }
    }
}
