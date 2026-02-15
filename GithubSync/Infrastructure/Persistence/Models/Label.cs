namespace GithubSync.Infrastructure.Persistence.Models
{
    public sealed class Label
    {
        public long Id { get; set; }
        public required string Repository { get; set; }
        public required string Name { get; set; }

        public List<IssueLabel> IssueLabels { get; set; } = new();
    }
}
