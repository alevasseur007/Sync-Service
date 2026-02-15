using FluentAssertions;
using GithubSync.Api.Contracts.Issues;
using GithubSync.Application.Github;
using System.Net;
using System.Net.Http.Json;

namespace GithubSync.Tests;

public class IssueTests
{
    private static List<GithubIssueDTO> SeedIssues()
    {
        var now = DateTimeOffset.UtcNow;

        return new()
        {
            new(
                Id: 1,
                Number: 101,
                Title: "First issue",
                Body: "Hello",
                State: "open",
                CreatedAt: now.AddDays(-2),
                UpdatedAt: now.AddMinutes(-10),
                ClosedAt: null,
                HtmlUrl: "https://github.com/owner/repo/issues/101",
                AuthorLogin: "alice",
                AssigneeLogin: null,
                CommentsCount: 0,
                Labels: new[] { "bug", "triage" }
            ),
            new(
                Id: 2,
                Number: 102,
                Title: "Second issue",
                Body: null,
                State: "closed",
                CreatedAt: now.AddDays(-3),
                UpdatedAt: now.AddMinutes(-5),
                ClosedAt: now.AddMinutes(-1),
                HtmlUrl: "https://github.com/owner/repo/issues/102",
                AuthorLogin: "bob",
                AssigneeLogin: "alice",
                CommentsCount: 2,
                Labels: new[] { "enhancement" }
            )
        };
    }

    [Fact]
    public async Task Given_seeded_github_issues_When_sync_Then_issues_are_inserted()
    {
        // Arrange
        var seed = SeedIssues();
        await using var factory = new TestAppFactory(seed);
        var client = factory.CreateClient();

        // Act
        var resp = await client.PostAsync("/sync", content: null);

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await client.GetFromJsonAsync<PagedResult<IssueListItemDTO>>("/issues?page=1&pageSize=50");
        list!.Total.Should().Be(2);
    }

    [Fact]
    public async Task Given_two_issues_When_paging_pageSize_1_Then_returns_one_item_and_total_two()
    {
        // Arrange
        await using var factory = new TestAppFactory(SeedIssues());
        var client = factory.CreateClient();
        (await client.PostAsync("/sync", null)).EnsureSuccessStatusCode();

        // Act
        var page1 = await client.GetFromJsonAsync<PagedResult<IssueListItemDTO>>("/issues?page=1&pageSize=1");
        var page2 = await client.GetFromJsonAsync<PagedResult<IssueListItemDTO>>("/issues?page=2&pageSize=1");

        // Assert
        page1!.Total.Should().Be(2);
        page1.Items.Should().HaveCount(1);

        page2!.Total.Should().Be(2);
        page2.Items.Should().HaveCount(1);

        page1.Items[0].Number.Should().NotBe(page2.Items[0].Number);
    }

    [Fact]
    public async Task Given_mixed_states_When_filter_state_open_Then_only_open_issues_returned()
    {
        // Arrange
        await using var factory = new TestAppFactory(SeedIssues());
        var client = factory.CreateClient();
        (await client.PostAsync("/sync", null)).EnsureSuccessStatusCode();

        // Act
        var open = await client.GetFromJsonAsync<PagedResult<IssueListItemDTO>>("/issues?state=open&page=1&pageSize=50");

        // Assert
        open!.Total.Should().Be(1);
        open.Items[0].State.Should().Be("open");
    }

    [Fact]
    public async Task Given_labels_When_filter_label_bug_Then_only_bug_issues_returned()
    {
        // Arrange
        await using var factory = new TestAppFactory(SeedIssues());
        var client = factory.CreateClient();
        (await client.PostAsync("/sync", null)).EnsureSuccessStatusCode();

        // Act
        var bugs = await client.GetFromJsonAsync<PagedResult<IssueListItemDTO>>("/issues?label=bug&page=1&pageSize=50");

        // Assert
        bugs!.Total.Should().Be(1);
        bugs.Items[0].Labels.Should().Contain("bug");
    }

    [Fact]
    public async Task Given_an_issue_When_get_by_number_Then_returns_details_including_labels()
    {
        // Arrange
        await using var factory = new TestAppFactory(SeedIssues());
        var client = factory.CreateClient();
        (await client.PostAsync("/sync", null)).EnsureSuccessStatusCode();

        // Act
        var details = await client.GetFromJsonAsync<IssueDetailsDTO>("/issues/101");

        // Assert
        details.Should().NotBeNull();
        details!.Number.Should().Be(101);
        details.Labels.Should().Contain(new[] { "bug", "triage" });
    }
}
