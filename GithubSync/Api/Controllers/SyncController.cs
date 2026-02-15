using GithubSync.Application.Sync;
using GithubSync.Infrastructure.Sync;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GithubSync.Api.Controllers
{
    [ApiController]
    [Route("sync")]
    public class SyncController : ControllerBase
    {
        private readonly ISyncIssues _sync;
        private readonly SingleSyncGate _gate;

        public SyncController(ISyncIssues sync, SingleSyncGate gate)
        {
            _sync = sync;
            _gate = gate;
        }

        [HttpPost]
        public async Task<ActionResult<SyncResult>> Run(CancellationToken ct)
        {
            if (!await _gate.TryEnterAsync(ct))
                return Conflict(new { error = "A sync is already running." });

            try
            {
                var result = await _sync.RunAsync(ct);
                return Ok(result);
            }
            finally
            {
                _gate.Exit();
            }
        }
    }
}
