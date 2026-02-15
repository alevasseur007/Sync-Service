using GithubSync.Api.Contracts.Issues;
using GithubSync.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GithubSync.Api.Controllers
{
    [ApiController]
    [Route("issues")]
    public sealed class IssuesController : ControllerBase
    {
        private readonly AppDbContext db;

        public IssuesController(AppDbContext pDb) => this.db = pDb;

        // GET /issues?state=open&label=bug&assignee=octocat&updatedSince=...&page=1&pageSize=50
        [HttpGet]
        public async Task<ActionResult<PagedResult<IssueListItemDTO>>> List(
            [FromQuery] string? state,
            [FromQuery] string? label,
            [FromQuery] string? assignee,
            [FromQuery] DateTimeOffset? updatedSince,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken ct = default)
        {
            // Basic validation
            if (page < 1) return BadRequest(new { error = "page must be >= 1" });
            if (pageSize is < 1 or > 200) return BadRequest(new { error = "pageSize must be between 1 and 200" });

            state = string.IsNullOrWhiteSpace(state) ? null : state.Trim().ToLowerInvariant();
            if (state is not null && state is not ("open" or "closed" or "all"))
                return BadRequest(new { error = "state must be open, closed, or all" });

            var q = db.Issues
                .AsNoTracking()
                .Include(i => i.IssueLabels)
                    .ThenInclude(il => il.Label)
                .AsQueryable();

            if (state is "open" or "closed")
                q = q.Where(i => i.State == state);

            if (!string.IsNullOrWhiteSpace(assignee))
                q = q.Where(i => i.AssigneeLogin == assignee);

            if (updatedSince is not null)
            {
                var sinceUtc = updatedSince.Value.UtcDateTime;
                q = q.Where(i => i.UpdatedAt >= sinceUtc);
            }

            if (!string.IsNullOrWhiteSpace(label))
                q = q.Where(i => i.IssueLabels.Any(il => il.Label.Name == label));

            q = q.OrderByDescending(i => i.UpdatedAt).ThenByDescending(i => i.Number);

            var total = await q.CountAsync(ct);

            var items = await q
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(i => new IssueListItemDTO(
                    i.Number,
                    i.Title,
                    i.State,
                    new DateTimeOffset(i.UpdatedAt, TimeSpan.Zero),
                    i.HtmlUrl,
                    i.AuthorLogin,
                    i.AssigneeLogin,
                    i.CommentsCount,
                    i.IssueLabels
                        .Select(il => il.Label.Name)
                        .OrderBy(n => n)
                        .ToList()
                ))
                .ToListAsync(ct);

            return Ok(new PagedResult<IssueListItemDTO>(page, pageSize, total, items));
        }

        // GET /issues/{number}
        [HttpGet("{number:int}")]
        public async Task<ActionResult<IssueDetailsDTO>> GetByNumber(int number, CancellationToken ct)
        {
            var issue = await db.Issues
                .AsNoTracking()
                .Include(i => i.IssueLabels)
                    .ThenInclude(il => il.Label)
                .SingleOrDefaultAsync(i => i.Number == number, ct);

            if (issue is null) return NotFound();

            var dto = new IssueDetailsDTO(
                Number: issue.Number,
                GitHubIssueId: issue.GitHubIssueId,
                Repository: issue.Repository,
                Title: issue.Title,
                Body: issue.Body,
                State: issue.State,
                CreatedAt: issue.CreatedAt,
                UpdatedAt: issue.UpdatedAt,
                ClosedAt: issue.ClosedAt,
                HtmlUrl: issue.HtmlUrl,
                AuthorLogin: issue.AuthorLogin,
                AssigneeLogin: issue.AssigneeLogin,
                CommentsCount: issue.CommentsCount,
                Labels: issue.IssueLabels.Select(il => il.Label.Name).OrderBy(n => n).ToList()
            );

            return Ok(dto);
        }
    }
}
