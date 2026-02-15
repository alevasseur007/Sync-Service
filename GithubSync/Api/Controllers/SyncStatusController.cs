using GithubSync.Application.Github;
using GithubSync.Application.Sync;
using GithubSync.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GithubSync.Api.Controllers
{
    [ApiController]
    [Route("sync/status")]
    public sealed class SyncStatusController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly GithubOptions _options;

        public SyncStatusController(AppDbContext db, IOptions<GithubOptions> options)
        {
            _db = db;
            _options = options.Value;
        }

        [HttpGet]
        public async Task<ActionResult<SyncStatusDTO>> Get(CancellationToken ct)
        {
            var repo = _options.Repository;

            var state = await _db.SyncStates
                .AsNoTracking()
                .SingleOrDefaultAsync(s => s.Repository == repo, ct);

            if (state is null)
            {
                return Ok(new SyncStatusDTO(
                    Repository: repo,
                    LastSuccessfulSyncAt: null,
                    LastSeenUpdatedAt: null,
                    LastRunStatus: null,
                    LastError: null
                ));
            }

            return Ok(new SyncStatusDTO(
                Repository: state.Repository,
                LastSuccessfulSyncAt: state.LastSuccessfulSyncAt,
                LastSeenUpdatedAt: state.LastSeenUpdatedAt,
                LastRunStatus: state.LastRunStatus,
                LastError: state.LastError
            ));
        }
    }
}
