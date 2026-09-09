# URL Health Monitor

A small service that polls a list of URLs on a schedule, persists the results, and shows their status/history in a web UI.

## Stack

- **Backend**: ASP.NET Core (.NET 9), layered architecture — `Domain` → `Application` → `Infrastructure` → `Api`
- **Persistence**: EF Core + SQLite, schema/seed data via migrations
- **Scheduling**: Quartz.NET (a single repeating job that checks all active URLs concurrently)
- **Frontend**: React + TypeScript (Vite), plain CSS
- **Docs**: Swagger UI (Swashbuckle) at `/swagger` in Development
- **Containerized**: Dockerfiles for both API and frontend, wired together with `docker-compose.yml`

## Running it

**Locally:**
```bash
cd backend/src/RosnetHealth.Api
dotnet run                      # API on http://localhost:5044, applies migrations + seeds on startup

cd frontend
npm install
npm run dev                     # UI on http://localhost:5173
```

**Docker:**
```bash
docker compose up -d --build
# API:      http://localhost:8080
# Frontend: http://localhost:3000
```
SQLite data persists across container restarts via a named volume (`rosnethealth-data`).

## Architecture

Four backend layers, each with a single job:

- **Domain** — `MonitoredUrlEntity`, `HealthCheckEntity`, and the one real piece of business logic in the whole layer: `HealthCheckEntity.Create(...)`, which decides what "healthy" means (2xx/3xx = Up) from a status code. No dependencies on anything else.
- **Application** — the ports (`IMonitoredUrlRepository`, `IHealthCheckRepository`, `IUrlHealthChecker`) that Infrastructure implements, `UrlMonitorService` (use-case orchestration: add/list/pause/delete/history), DTOs, and duplicate-URL validation (the one business rule that needs repository access, so it can't live in Domain).
- **Infrastructure** — EF Core `DbContext` + configurations (cascade delete on URL removal, unique index on `Url`), repository implementations, the HTTP health checker, and the Quartz job that drives polling.
- **Api** — controllers, global exception-handling middleware (`DuplicateUrlException` → 409, everything else → 500 `ProblemDetails`), Swagger, CORS.

Repository interfaces live in Application, not Infrastructure — Application is the consumer, Infrastructure is the implementer (Dependency Inversion), which is also why `Application.Tests` can fully test `UrlMonitorService` with mocks and never touch a database.

## What's built (core scope)

- Add/list/pause-resume/delete monitored URLs
- A Quartz job checks all active URLs every 30s (configurable), concurrently, and persists every result — paused URLs are skipped but stay visible in the UI, greyed out
- Full history per URL, viewable in the UI
- Seed data + migrations, so a fresh clone has data on first run

## What I cut, and why

- **Per-URL polling intervals** — explicitly called out as bonus scope in the brief. One global interval (Quartz job checks everyone every cycle) covers the actual requirement; per-URL intervals would mean tracking a "next due" time per URL and querying for what's due each tick instead of "check everyone" — a bigger change I didn't think was worth the time given it's optional.
- **CQRS / MediatR / FluentValidation** — my usual stack for this kind of service layer, but for 4 endpoints it's disproportionate ceremony (a Command + Handler + Validator per operation vs. one method on a plain service class). DataAnnotations on the request DTO cover shape validation; the one rule that needed data access (duplicate detection) lives in `UrlMonitorService`.
- **Soft delete** — considered it, went with hard delete + cascade instead. There's no requirement to recover a deleted URL's history, and "pause" already covers "stop monitoring without losing data." A second `IsDeleted` flag on top of `IsActive` would just be two overlapping ways to express similar things.
- **Auth / multi-tenancy** — out of scope per the brief; this is a single-operator tool as built.
- **Response-body assertions, retry/backoff on checks** — bonus territory per the brief; the core "is it up" signal (status code + timing) is what's implemented.

## Testing approach

34 backend tests, deliberately uneven across layers — I put effort where there was actual logic to test, not to hit a coverage number:

| Layer | Tests | Why |
|---|---|---|
| Domain | 7 | Only `HealthCheckEntity`'s status-classification boundaries (null/199/200/399/400/500). `MonitoredUrlEntity` and the base `Entity` are plain data holders — nothing there to test. |
| Application | 13 | The heaviest coverage on purpose: `UrlMonitorService` behind mocked repositories/checker (no DB, no HTTP), covering the null-status "never checked" path, not-found guards, and duplicate-URL detection. Plus the hand-written half of `UrlMapper` (the Mapperly-generated half is closer to boilerplate). |
| Infrastructure | 14 | Repository tests run against **real SQLite** (in-memory, via an open `SqliteConnection`), not mocks — the point was verifying EF Core actually translates the `GroupBy`/`OrderByDescending().First()` "latest check per URL" query correctly, and that the cascade-delete FK config really removes history rows. `HttpUrlHealthChecker` tested against a fake `HttpMessageHandler` for the success/500/connection-failure/timeout paths. |

**What's explicitly not tested, and why:**
- **Api layer** — no controller or `WebApplicationFactory` integration tests. Manually verified every endpoint (validation → 400, duplicate → 409, missing → 404, pause/resume, delete) via live HTTP calls during development, which is how the DbContext concurrency bug below was actually found — but that verification isn't repeatable the way a test suite is.
- **`HealthCheckJob` itself** — the orchestration logic it calls is tested indirectly (via the repository and HTTP checker tests it composes), but the Quartz-specific wiring (the job/trigger registration, `IJobExecutionContext` plumbing) isn't tested directly. Mocking Quartz's execution context for low marginal value didn't seem worth it at this scope.
- **Frontend** — no test runner set up at all (no Vitest/RTL). Verified manually in-browser (add/pause/resume/delete/history, dark mode, error states) rather than with automated tests.
- **One known branch gap**: `HttpUrlHealthChecker`'s real-cancellation guard (`catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)`) is correct but isn't directly exercised by a test — the timeout test only covers the "not a real cancellation" branch.

## AI usage

Built with Claude Code throughout, as a genuine back-and-forth rather than one-shot generation — the conversation history is effectively a design log. A few specific, concrete examples of the collaboration (not a generic "AI helped me code" statement):

- **I overrode a direct recommendation.** Claude's default suggestion for polling was a plain `BackgroundService` + `PeriodicTimer` — the right-sized tool for one global-interval job, no more. I asked to switch to Quartz.NET anyway, mainly to keep the door open for per-URL scheduling later and because it's closer to what a real .NET shop would already have running. That's a real tradeoff (more dependency weight for a scope that didn't strictly need it), not a free upgrade, and I'd defend that choice in the panel rather than call it "what the AI said to do."
- **It found a real concurrency bug by actually running the code**, not by reading it: the first version of the polling job shared one `DbContext` across concurrent per-URL checks via `Task.WhenAll`, which threw `"A second operation was started on this context instance..."` the first time it actually ran. The fix (a fresh DI scope, and therefore a fresh `DbContext`, per concurrent check) came from that live failure, not a code-review guess.
- **I also caught a framing issue, not just a bug.** When discussing the N+1 query for "latest check per URL," it initially argued the fix was "extra complexity" not worth it at this scale — I pushed back that the fix (a `GroupBy`/`OrderByDescending().First()` query) wasn't actually much harder to write than the naive version, so there was no good reason to deliberately ship the worse one. It agreed and we went with the single-query version, which is now tested against real SQLite in `HealthCheckRepositoryTests`.

## What I'd do with more time

Roughly in priority order:

1. **Per-URL polling intervals.** The single biggest deliberate scope cut. Would add a nullable `IntervalSeconds` on `MonitoredUrlEntity` (falling back to a global default), change the Quartz job's query from "all active URLs" to "URLs due for a check right now," and add the interval field to the add-URL UI.

2. **Api and integration test coverage.** Right now the Api layer's correctness is only verified by hand. I'd add `WebApplicationFactory`-based integration tests hitting real endpoints against an in-memory/temp SQLite database — covering the validation → 400, duplicate → 409, and not-found → 404 paths — plus a couple of tests around the exception-handling middleware itself, which currently has zero direct coverage despite being the thing that turns exceptions into HTTP responses.

3. **Close the race-condition gap on duplicate URLs.** The unique index on `Url` is real defense-in-depth against the check-then-insert race in `AddUrlAsync`, but if that race actually happened, the resulting `DbUpdateException` would currently fall through to a generic 500 instead of a clean 409. I'd add a specific catch for that in the exception middleware (or catch it in the repository and translate it to `DuplicateUrlException` there).

4. **Immediate check on add.** Right now a newly added URL shows "never checked" until the next scheduled cycle (up to the full polling interval away). I sketched this out but didn't build it: add an `IHealthCheckTrigger` port in Application, implement it in Infrastructure via Quartz's `scheduler.TriggerJob(...)` (which doesn't block the response — it just tells Quartz to run the job again immediately), and call it from `AddUrlAsync`. Small change, meaningfully better first-use experience.

5. **Frontend automated tests.** No Vitest/RTL setup at all currently. Would prioritize testing `api.ts` (the fetch layer, including error parsing) and `UrlRow`'s pause/resume/delete interactions, since those are the parts most likely to silently break.

6. **Runtime-configurable frontend API URL.** `VITE_API_BASE_URL` is baked in at Docker build time. A more production-ready setup would inject it at container *start* time (e.g. an entrypoint script that writes a small `config.js` from an environment variable, or nginx `envsubst` templating), so the same built image could be deployed against different API URLs without a rebuild.

7. **Observability.** Structured logging (Serilog) instead of the default console logger, and a real `/health` endpoint for the API container itself (distinct from what this app *monitors* — this would be Docker/orchestrator-facing liveness, via `HEALTHCHECK` in the Dockerfile and ASP.NET Core's `AddHealthChecks()`).

8. **CI.** A GitHub Actions workflow running `dotnet test` and the frontend type-check/build on every push — there isn't one yet, so regressions are currently only caught by remembering to run tests locally.

9. **Real-time updates.** The UI polls every 5 seconds; a `HealthCheckEntity` insert could instead push over SignalR so the dashboard updates the instant a check completes, rather than up to 5 seconds later.

10. **Response-body assertions and configurable retry/backoff** — both explicitly bonus scope per the brief, but the natural next step for making the "is it up" signal richer than status-code-only.

## References 
https://docs.google.com/document/d/1Ra21O8kFNV3Mn5ouCuOxSlTqqQYEAN-ivRIlJVpBJQs/edit?usp=sharing
