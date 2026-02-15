GitHub Issues Sync Service

A small API that mirrors GitHub Issues from one repository into a local database so you can query them quickly, without repeatedly hitting the GitHub API. The service supports manual sync and scheduled sync, and exposes filterable/paged read endpoints.

What this service does

Mirrors issues from a configured GitHub repository (owner/repo) into a local SQLite database

Exposes a read API to list issues and fetch issue details

Keeps data updated via:

Manual sync: POST /sync

Scheduled sync: runs every GitHub__SyncIntervalMinutes

Tracks sync state:

last attempt time

last successful sync time

watermark (LastSeenUpdatedAt) for incremental sync

last run status + error

How it works (high level)
Sync model

The service fetches issues from GitHub using the REST API.

It uses an incremental watermark (last seen updated_at) to avoid re-fetching the full issue set every time.

Sync is unique:

If an issue doesn’t exist locally → insert

If it exists → update only when GitHub’s updated_at is newer

No overlapping sync runs:

A single-flight lock prevents the scheduled sync and manual sync from running at the same time.

Storage

SQLite database persisted in a Docker volume at /data/issues.db

Tables include:

Issues

Labels + join table (many-to-many)

SyncStates (repository sync metadata)

API Endpoints
Sync / control

POST /sync
Triggers a sync run. Returns counts + watermark change.

GET /sync/status
Returns the last attempt, last success, watermark, and any last error.

Read

GET /issues
Query params:

state = open|closed|all

label (optional)

assignee (optional)

updatedSince (optional, ISO 8601)

page (default 1)

pageSize (default 50)

GET /issues/{number}
Returns details for one issue by issue number.

Health

GET /health/live
Process is running

GET /health/ready
Dependencies check (DB required; GitHub reachability is “degraded” if down)

Run with Docker
1) Prerequisites

Docker Desktop (Windows/Mac) or Docker Engine (Linux)

2) Create .env (do not commit)

Create a .env file next to docker-compose.yml:

GITHUB_REPO=dotnet/runtime
GITHUB_TOKEN=github_pat_...

3) Start
docker compose up --build


Service URLs:

API: http://localhost:8080

Swagger UI: http://localhost:8080/swagger

Demo script (copy/paste)
# Health
curl http://localhost:8080/health/live
curl http://localhost:8080/health/ready

# Manual sync
curl -X POST http://localhost:8080/sync

# Read API
curl "http://localhost:8080/issues?page=1&pageSize=10"
curl "http://localhost:8080/issues?state=open&page=1&pageSize=10"
curl "http://localhost:8080/issues?label=bug&page=1&pageSize=10"

# Sync status
curl http://localhost:8080/sync/status

Configuration

Environment variables (Docker uses .env):

GITHUB_REPO → GitHub__Repository

GITHUB_TOKEN → GitHub__Token

GitHub__SyncIntervalMinutes (default: 10)

GitHub__InitialLookbackDays (default: 7)

ConnectionStrings__AppDb (default in Docker: Data Source=/data/issues.db)

Notes / design decisions

SQLite for a lightweight demo and easy local persistence

EF Core for migrations + simple mapping

resilient (retry/backoff on GitHub calls)

observable (structured logs and clear status endpoints)
