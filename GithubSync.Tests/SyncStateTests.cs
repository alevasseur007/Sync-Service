using FluentAssertions;
using GithubSync.Application.Github;
using GithubSync.Application.Sync;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace GithubSync.Tests
{
    public class SyncStateTests
    {
        [Fact]
        public async Task Given_no_sync_When_get_status_Then_fields_are_nullish()
        {
            // Arrange
            await using var factory = new TestAppFactory(new List<GithubIssueDTO>());
            var client = factory.CreateClient();

            // Act
            var status = await client.GetFromJsonAsync<SyncStatusDTO>("/sync/status");

            // Assert
            status.Should().NotBeNull();
            status!.LastRunStatus.Should().BeNull();
            status.LastSeenUpdatedAt.Should().BeNull();
        }

        [Fact]
        public async Task Given_successful_sync_When_get_status_Then_run_status_is_success()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var seed = new List<GithubIssueDTO>
        {
            new(
                Id: 1, Number: 1, Title: "A", Body: null, State: "open",
                CreatedAt: now.AddDays(-1), UpdatedAt: now,
                ClosedAt: null, HtmlUrl: "x", AuthorLogin: "u",
                AssigneeLogin: null, CommentsCount: 0, Labels: Array.Empty<string>()
            )
        };

            await using var factory = new TestAppFactory(seed);
            var client = factory.CreateClient();

            // Act
            (await client.PostAsync("/sync", null)).EnsureSuccessStatusCode();
            var status = await client.GetFromJsonAsync<SyncStatusDTO>("/sync/status");

            // Assert
            status!.LastRunStatus.Should().Be("Success");
            status.LastSeenUpdatedAt.Should().NotBeNull();
            status.LastSuccessfulSyncAt.Should().NotBeNull();
        }
    }
}
