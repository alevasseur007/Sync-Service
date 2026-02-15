using GithubSync.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace GithubSync.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Issue> Issues => Set<Issue>();
        public DbSet<Label> Labels => Set<Label>();
        public DbSet<IssueLabel> IssueLabels => Set<IssueLabel>();
        public DbSet<SyncState> SyncStates => Set<SyncState>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Issue
            modelBuilder.Entity<Issue>()
                .HasIndex(x => new { x.Repository, x.GitHubIssueId })
                .IsUnique();

            modelBuilder.Entity<Issue>()
                .HasIndex(x => new { x.Repository, x.Number });

            // Label
            modelBuilder.Entity<Label>()
                .HasIndex(x => new { x.Repository, x.Name })
                .IsUnique();

            // IssueLabel (many-to-many)
            modelBuilder.Entity<IssueLabel>()
                .HasKey(x => new { x.IssueId, x.LabelId });

            modelBuilder.Entity<IssueLabel>()
                .HasOne(x => x.Issue)
                .WithMany(x => x.IssueLabels)
                .HasForeignKey(x => x.IssueId);

            modelBuilder.Entity<IssueLabel>()
                .HasOne(x => x.Label)
                .WithMany(x => x.IssueLabels)
                .HasForeignKey(x => x.LabelId);

            // SyncState
            modelBuilder.Entity<SyncState>()
                .HasIndex(x => x.Repository)
                .IsUnique();
        }
    }
}
