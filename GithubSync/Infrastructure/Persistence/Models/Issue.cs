namespace GithubSync.Infrastructure.Persistence.Models
{
    public sealed class Issue
    {
        public long Id { get; set; } // DB PK
        public required string Repository { get; set; } // "owner/repo"

        public long GitHubIssueId { get; set; } // stable unique id from GitHub
        public int Number { get; set; }

        public required string Title { get; set; }
        public string? Body { get; set; }
        public required string State { get; set; } // "open"/"closed"

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }

        public required string HtmlUrl { get; set; }

        public required string AuthorLogin { get; set; }
        public string? AssigneeLogin { get; set; }
        public int CommentsCount { get; set; }

        public List<IssueLabel> IssueLabels { get; set; } = new();
    }
}
