namespace GithubSync.Infrastructure.Persistence.Models
{
    public sealed class IssueLabel
    {
        public long IssueId { get; set; }
        public Issue Issue { get; set; } = null!;

        public long LabelId { get; set; }
        public Label Label { get; set; } = null!;
    }
}
