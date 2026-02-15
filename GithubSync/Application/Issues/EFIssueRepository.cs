using GithubSync.Application.Github;
using GithubSync.Infrastructure.Persistence;
using GithubSync.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace GithubSync.Application.Issues
{
    public class EFIssueRepository
    : IIssueRepository
    {
        private readonly AppDbContext db;

        public EFIssueRepository(AppDbContext pDb) => this.db = pDb;

        public async Task<UpsertOutcome> UpsertBatchAsync(string repository, IReadOnlyList<GithubIssueDTO> issues, CancellationToken ct)
        {
            if (issues.Count == 0)
                return new UpsertOutcome(0, 0, 0);

            // Load existing issues for this batch
            var ids = issues.Select(i => i.Id).ToList();

            var existing = await db.Issues
                .Include(x => x.IssueLabels)
                .ThenInclude(il => il.Label)
                .Where(x => x.Repository == repository && ids.Contains(x.GitHubIssueId))
                .ToListAsync(ct);

            var existingByGitHubId = existing.ToDictionary(x => x.GitHubIssueId);

            int inserted = 0, updated = 0, unchanged = 0;

            // Preload labels for repo into dictionary (avoid per-issue queries)
            var repoLabels = await db.Labels
                .Where(l => l.Repository == repository)
                .ToListAsync(ct);

            var labelByName = repoLabels.ToDictionary(l => l.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var dto in issues)
            {
                if (!existingByGitHubId.TryGetValue(dto.Id, out var entity))
                {
                    entity = new Issue
                    {
                        Repository = repository,
                        GitHubIssueId = dto.Id,
                        Number = dto.Number,
                        Title = dto.Title,
                        Body = dto.Body,
                        State = dto.State,
                        CreatedAt = dto.CreatedAt.UtcDateTime,
                        UpdatedAt = dto.UpdatedAt.UtcDateTime,
                        ClosedAt = dto.ClosedAt?.UtcDateTime,
                        HtmlUrl = dto.HtmlUrl,
                        AuthorLogin = dto.AuthorLogin,
                        AssigneeLogin = dto.AssigneeLogin,
                        CommentsCount = dto.CommentsCount
                    };

                    db.Issues.Add(entity);
                    inserted++;

                    await ApplyLabelsAsync(repository, entity, dto.Labels, labelByName, ct);
                    continue;
                }

                if (dto.UpdatedAt <= entity.UpdatedAt)
                {
                    unchanged++;
                    continue;
                }

                entity.Number = dto.Number;
                entity.Title = dto.Title;
                entity.Body = dto.Body;
                entity.State = dto.State;
                entity.CreatedAt = dto.CreatedAt.UtcDateTime;
                entity.UpdatedAt = dto.UpdatedAt.UtcDateTime;
                entity.ClosedAt = dto.ClosedAt?.UtcDateTime;
                entity.HtmlUrl = dto.HtmlUrl;
                entity.AuthorLogin = dto.AuthorLogin;
                entity.AssigneeLogin = dto.AssigneeLogin;
                entity.CommentsCount = dto.CommentsCount;

                updated++;

                await ApplyLabelsAsync(repository, entity, dto.Labels, labelByName, ct);
            }

            await db.SaveChangesAsync(ct);
            return new UpsertOutcome(inserted, updated, unchanged);
        }

        private async Task ApplyLabelsAsync(
            string repository,
            Issue issue,
            IReadOnlyList<string> labels,
            Dictionary<string, Label> labelByName,
            CancellationToken ct)
        {
            var desired = new HashSet<string>(labels.Where(l => !string.IsNullOrWhiteSpace(l)), StringComparer.OrdinalIgnoreCase);

            issue.IssueLabels.RemoveAll(il => !desired.Contains(il.Label.Name));

            var existingNames = issue.IssueLabels.Select(il => il.Label.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var toAdd = desired.Except(existingNames).ToList();

            foreach (var name in toAdd)
            {
                if (!labelByName.TryGetValue(name, out var labelEntity))
                {
                    labelEntity = new Label { Repository = repository, Name = name };
                    db.Labels.Add(labelEntity);
                    labelByName[name] = labelEntity;
                    await db.SaveChangesAsync(ct);
                }

                issue.IssueLabels.Add(new IssueLabel { Issue = issue, Label = labelEntity });
            }
        }
    }
}
