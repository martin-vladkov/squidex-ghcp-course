# Walk Backlog — Observability & Quality Epic

Items identified during Walk exercises Ex1–Ex11. Each item is scoped to one PR.
Acceptance criteria are written as testable bullet points.

---

## BACK-1 — Wire Prometheus exporter for `squidex.app.language_ops`

**Background**  
Walk Ex9 added the OTel counter `squidex.app.language_ops` (meter `Squidex.Apps`)
and registered it in the MeterProvider. The counter is collected in-process but
there is no Prometheus exporter, so operators cannot scrape
`squidex_app_language_ops_total` from a `/metrics` endpoint.

**Code paths**
- `backend/src/Squidex/Config/Domain/TelemetryServices.cs` — add
  `AddPrometheusExporter()` to the MeterProviderBuilder
- `backend/src/Squidex/appsettings.json` — add `logging:prometheus:enabled`
  flag (default `false`)
- `backend/src/Squidex/Program.cs` or `Startup.cs` — map `/metrics` endpoint
  when the flag is on

**Acceptance criteria**
- [ ] `appsettings.json` has `logging:prometheus:enabled` (default `false`)
- [ ] `GET /metrics` returns Prometheus text format including
  `squidex_app_language_ops_total{op="AddLanguage",language="en"}` when the
  flag is `true`
- [ ] The `/metrics` endpoint is not registered when the flag is `false`
- [ ] curl evidence or integration test captured in PR Evidence section
- [ ] Rollback: flip flag to `false` — endpoint disappears without redeploy

**Dependencies**: Walk Ex9 (AppMetrics.cs) must be merged first.  
**Package required**: `OpenTelemetry.Exporter.Prometheus.AspNetCore`

---

## BACK-2 — Extend language-op metric to all other AppDomainObject operations

**Background**  
`AppMetrics.LanguageOps` only counts the three language commands. The same
`AppDomainObject` handles ~20 other commands (contributor, client, role,
workflow, plan changes) with no metric coverage.

**Code paths**
- `backend/src/Squidex.Domain.Apps.Entities/Apps/DomainObject/AppDomainObject.cs`
  — existing `LogLanguageOp` pattern; generalise to `LogDomainOp` and call from
  all private command handlers
- `backend/src/Squidex.Domain.Apps.Entities/Apps/DomainObject/AppMetrics.cs`
  — add a second counter `squidex.app.domain_ops` tagged by `op` and `aggregate`

**Acceptance criteria**
- [ ] New counter `squidex.app.domain_ops` is defined in `AppMetrics.cs`
- [ ] All private handlers in `AppDomainObject.cs` call the new counter
- [ ] `MeterListener` test verifies at least one non-language op (e.g.
  `AttachClient`) is counted correctly
- [ ] 0 new StyleCop warnings
- [ ] Existing `AddLanguage_increments_language_ops_counter` test still passes

**Dependencies**: BACK-1 (optional; can be implemented without Prometheus).

---

## BACK-3 — Add hard coverage threshold for `Squidex.Domain.Apps.Entities` assembly

**Background**  
Walk Ex10 added a soft coverage gate (advisory only, `continue-on-error: true`).
The `Squidex.Domain.Apps.Entities` assembly is the most actively worked-on
assembly in the Walk exercises and has good test coverage for the language-op
code paths. A hard threshold (e.g. 60% line coverage on that assembly alone)
would catch regressions without gating the full build.

**Code paths**
- `.github/workflows/ghcp-walk-coverage.yml` — add a second job or step that
  runs `reportgenerator` with `-reporttypes:Cobertura` and then checks the
  `line-rate` attribute in the output XML for the one assembly
- Keep the existing advisory summary job unchanged

**Acceptance criteria**
- [ ] New CI step reads the Cobertura XML and extracts the line-rate for
  `Squidex.Domain.Apps.Entities`
- [ ] Step exits non-zero if line-rate < 0.60 (60%), causing the job to fail
- [ ] Threshold is set in a top-level `env:` variable so it is easy to adjust
- [ ] PR evidence includes actual line-rate figure from CI output
- [ ] The existing advisory summary step remains `continue-on-error: true`

**Dependencies**: Walk Ex10 (coverage workflow) must be merged first.

---

## BACK-4 — Run gitleaks in full git-history mode on CI

**Background**  
Walk Ex8 added `gitleaks detect --no-git` (file scan only). This misses secrets
that were committed and then deleted — they remain in the git history and are
still recoverable. Running `gitleaks detect` (without `--no-git`) scans the
full commit history and catches those cases.

**Code paths**
- `.github/workflows/ghcp-walk-security.yml` — the `gitleaks/gitleaks-action@v2`
  step already fetches full history (`fetch-depth: 0`); remove `--no-git` from
  the implicit default or confirm the action uses history mode by default

**Acceptance criteria**
- [ ] CI step runs gitleaks in history-scan mode (not `--no-git`)
- [ ] All 28 known-safe values from `.gitleaks.toml` are suppressed in history
  mode as well (verify with `gitleaks detect --source . --config .gitleaks.toml`)
- [ ] CI run passes with 0 new leaks detected
- [ ] PR Evidence section includes `gitleaks detect` output showing 0 leaks

**Dependencies**: Walk Ex8 (`.gitleaks.toml`) must be merged first.  
**Note**: `gitleaks/gitleaks-action@v2` already defaults to history mode when
`fetch-depth: 0` is set — confirm this is actually the case before implementing.

---

## BACK-5 — Raise `LanguagesConfig.Cleanup` coverage with property-based tests

**Background**  
Walk Ex6 optimised `LanguagesConfig.Cleanup` (`Fallbacks.Any()` → `Count > 0`,
`ToList()` → `Keys.ToArray()`). The existing `Perf_Set_Cleanup_baseline` test
measures timing but does not assert correctness across arbitrary inputs (e.g.
a language that is its own fallback, or a missing master language). Property-
based tests would catch edge cases that unit tests miss.

**Code paths**
- `backend/tests/Squidex.Domain.Apps.Core.Tests/Model/Apps/LanguagesConfigTests.cs`
  — add 2-3 `[Theory]` tests using `FsCheck.Xunit` or `CsCheck` covering:
  1. Cleanup removes all fallbacks for languages not in the active set
  2. Master language is never removed even if it appears in a fallback list
  3. Round-trip: applying Cleanup twice produces the same result as once
- `backend/tests/Squidex.Domain.Apps.Core.Tests/Squidex.Domain.Apps.Core.Tests.csproj`
  — add `FsCheck.Xunit` or `CsCheck` package reference

**Acceptance criteria**
- [ ] At least 3 property-based test cases added with clear property descriptions
- [ ] All 3 properties hold for 1 000 random inputs (default `[Property]` runs)
- [ ] Tests run in CI via the existing `ghcp-walk-coverage.yml` filter
- [ ] 0 new StyleCop warnings
- [ ] PR Evidence section shows the test pass count including the new tests

**Dependencies**: Walk Ex6 (Cleanup optimisation) must be merged.

---

## Priority order

| # | Item | Effort | Risk | Value |
|---|------|--------|------|-------|
| 1 | BACK-4 (gitleaks history mode) | XS | Low | High — closes a real security gap |
| 2 | BACK-3 (hard coverage threshold) | S | Low | High — enforces quality floor |
| 3 | BACK-1 (Prometheus exporter) | S | Low | Medium — enables production visibility |
| 4 | BACK-2 (extend domain-op metric) | M | Low | Medium — broader observability |
| 5 | BACK-5 (property-based tests) | M | Low | Medium — robustness for edge cases |

---

# Run Backlog — Rules Subsystem Reliability Epic

Items identified during Run Ex12 analysis of `backend/src/Squidex.Domain.Apps.Entities/Rules/`.
Each item is scoped to one PR. Acceptance criteria are testable bullet points.
Tracker disabled — items committed here per Ex12 fallback guidance.

---

## BACK-6 — CronJobUpdater: add error handling in `HandleCronJobAsync`

**Background**
`HandleCronJobAsync` has no `try/catch`. If `appProvider.GetRuleAsync` or
`ruleEnqueuer.EnqueueAsync` throws a transient exception (DB timeout, bus
unavailable), the exception propagates unhandled to the cron scheduler. The
cron trigger is silently lost with no log entry and no retry opportunity.

**Code path**
`backend/src/Squidex.Domain.Apps.Entities/Rules/CronJobUpdater.cs` — method
`HandleCronJobAsync` (lines 38–55)

**Acceptance criteria**
- [ ] `HandleCronJobAsync` wraps its body in `try/catch(Exception ex)`
- [ ] On exception: call a new `LogMessages.LogFailedToHandleCronJob` (`Error`,
  with `ruleId`, `appId.Id`, and `exception` parameters)
- [ ] Exception is caught and not re-thrown (cron scheduler keeps running)
- [ ] New unit test: mock `GetRuleAsync` to throw; assert no exception escapes
  and the logger received exactly one Error-level call
- [ ] Existing tests still pass (`CronJobUpdaterTests` 11/11)

**Pattern to follow**
`backend/src/Squidex.Domain.Apps.Entities/Rules/Runner/RuleRunnerJob.cs` —
`LogMessages.LogFailedToRunRule` call in the catch block

**Priority / effort / good first task**
Medium priority · Small (~30 min) · ✅ Good first task

---

## BACK-7 — RuleEnqueuer: evict rule cache on `RuleDeleted` / `RuleUpdated`

**Background**
`GetRulesAsync` caches app rules in `IMemoryCache` for `RulesCacheDuration`
(default ~10 s). `RuleEnqueuer.On()` processes `IEventConsumer` events but
handles no `RuleDeleted` or `RuleUpdated` events. After a rule is deleted or
changed, up to 10 s of incoming events can still match the stale cached copy,
creating ghost rule-job entries.

**Code path**
`backend/src/Squidex.Domain.Apps.Entities/Rules/RuleEnqueuer.cs` — `GetRulesAsync`
(lines 118–134), `On(IEnumerable<Envelope<IEvent>>)` (line 76)

**Acceptance criteria**
- [ ] `On()` handles `RuleDeleted` by calling `cache.Remove(cacheKey)` for the
  affected app
- [ ] `On()` handles `RuleUpdated` by calling `cache.Remove(cacheKey)` for the
  affected app
- [ ] Unit tests: send `RuleDeleted`/`RuleUpdated` envelopes; assert the cache
  key is evicted (use a mock or real `IMemoryCache`)
- [ ] Existing `RuleEnqueuerTests` still pass

**Priority / effort / good first task**
Medium priority · Small (~45 min) · ✅ Good first task

---

## BACK-8 — RuleFlowTrackingCallback: distinguish Cancelled/Timeout from failure

**Background**
`OnUpdateAsync` tracks any non-`Completed` status as a failure
(`totalFailed += 1`). If `FlowExecutionStatus` has additional values (e.g.,
`Cancelled`, `Running`, `Pending`), they incorrectly inflate the failure
counter in `IRuleUsageTracker`. The `RuleCounters` struct only has
`TotalSucceeded` and `TotalFailed` — a `TotalCancelled` field may be needed.

**Code paths**
- `backend/src/Squidex.Domain.Apps.Entities/Rules/RuleFlowTrackingCallback.cs`
  line 23 — binary `Completed` / else branch
- `backend/src/Squidex.Domain.Apps.Entities/Rules/IRuleUsageTracker.cs`
  line 35 — `RuleCounters` struct

**Acceptance criteria**
- [ ] Enumerate all `FlowExecutionStatus` values from the Squidex.Flows package
- [ ] `OnUpdateAsync` handles each known status explicitly (no implicit fallthrough)
- [ ] If a `Cancelled` state exists, neither `totalSucceeded` nor `totalFailed`
  is incremented for it (or a new `TotalCancelled` counter is added)
- [ ] Unit tests cover all enum variants
- [ ] Existing tests still pass

**Priority / effort**
Low priority · Medium (~1 h) · Requires Squidex.Flows package inspection first

---

## BACK-9 — RuleCommandMiddleware: log rule mutations for audit trail

**Background**
Rule create/update/delete/enable/disable commands pass through
`RuleCommandMiddleware.EnrichResultAsync` with no log output. There is no
way to audit who changed a rule, when, or what the result was from application
logs. This makes post-incident investigation difficult.

**Code path**
`backend/src/Squidex.Domain.Apps.Entities/Rules/RuleCommandMiddleware.cs` —
`EnrichResultAsync` (lines 20–31)

**Acceptance criteria**
- [ ] Add `ILogger<RuleCommandMiddleware>` constructor parameter
- [ ] Add `LogMessages.LogRuleMutated` (`Information`, parameters: `ruleId`,
  `commandType` string) called after enrichment succeeds
- [ ] Unit test: execute a `CreateRule` command through the middleware and assert
  the Information log is emitted
- [ ] Follows `[LoggerMessage]` source-generator pattern (no string interpolation)
- [ ] Existing tests still pass

**Priority / effort / good first task**
Low priority · Small (~20 min) · ✅ Good first task — follows established Ex9 pattern exactly

---

## BACK-10 — RuleQueueWriter: log batch job count on flush

**Background**
`FlushCoreAsync` in `RuleQueueWriter` writes batches of flow jobs to
`IFlowManager` but logs no count information. Operators cannot tell from logs
how many jobs were queued per batch or per event-consumer cycle. The existing
`LogMessages.LogAddingRuleJob` fires per individual job but carries no batch
aggregate.

**Code path**
`backend/src/Squidex.Domain.Apps.Entities/Rules/RuleQueueWriter.cs` —
`FlushCoreAsync` (lines 76+)

**Acceptance criteria**
- [ ] After `FlushCoreAsync` completes, log one `Information` message with the
  count of jobs written in that flush using `[LoggerMessage]`
- [ ] Log is suppressed (no-op) when count is 0 to avoid log noise on quiet
  event streams
- [ ] Unit test: write 3 jobs and flush; assert logger received exactly one
  batch-count log with value 3
- [ ] Existing `RuleQueueWriterTests` still pass

**Priority / effort / good first task**
Low priority · Small (~20 min) · ✅ Good first task

---

## Run backlog priority order

| # | Item | Effort | Risk | Value |
|---|------|--------|------|-------|
| 1 | BACK-6 (CronJob error handling) | S | Low | High — prevents silent trigger loss |
| 2 | BACK-7 (cache eviction on delete) | S | Low | Medium — prevents ghost rule-jobs |
| 3 | BACK-9 (mutation audit log) | XS | Low | Medium — enables post-incident audit |
| 4 | BACK-10 (batch count log) | XS | Low | Low — operational visibility |
| 5 | BACK-8 (status enum coverage) | M | Low | Low — correctness improvement |

## Delegation — BACK-6 simulated patch plan

Item BACK-6 is the highest-priority good-first-task. Below is the agent-ready
patch plan a developer (or AI agent) would follow to complete it.

### Patch plan

**Branch**: `fix/cron-job-error-handling`  
**Base**: `main` (or current sprint branch)

**Step 1** — Add log message to `LogMessages.cs`

```csharp
[LoggerMessage(Level = LogLevel.Error,
    Message = "Failed to handle cron job for rule '{ruleId}' in app '{appId}'.")]
public static partial void LogFailedToHandleCronJob(
    ILogger logger, DomainId ruleId, DomainId appId, Exception exception);
```

**Step 2** — Wrap `HandleCronJobAsync` body in `CronJobUpdater.cs`

```csharp
public async Task HandleCronJobAsync(CronJob<CronJobContext> job, CancellationToken ct)
{
    var (appId, ruleId) = job.Context;
    try
    {
        var rule = await appProvider.GetRuleAsync(appId.Id, ruleId, ct);
        if (rule == null || rule.Trigger is not CronJobTrigger cronJob)
        {
            LogMessages.LogCronJobSkipped(log, ruleId, appId.Id);
            return;
        }
        LogMessages.LogCronJobTriggered(log, ruleId, appId.Id);
        var @event = new RuleCronJobTriggered { AppId = appId, RuleId = ruleId, Value = cronJob.Value };
        await ruleEnqueuer.EnqueueAsync(rule, Envelope.Create(@event), ct);
    }
    catch (Exception ex)
    {
        LogMessages.LogFailedToHandleCronJob(log, ruleId, appId.Id, ex);
    }
}
```

**Step 3** — Add test to `CronJobUpdaterTests.cs`

```csharp
[Fact]
public async Task Should_log_error_and_not_rethrow_when_rule_provider_throws()
{
    A.CallTo(() => AppProvider.GetRuleAsync(AppId.Id, A<DomainId>._, A<CancellationToken>._))
        .Throws(new InvalidOperationException("transient"));
    A.CallTo(() => log.IsEnabled(LogLevel.Error)).Returns(true);

    var job = new CronJob<CronJobContext>
    {
        Id = DomainId.NewGuid().ToString(),
        Context = new CronJobContext(AppId, DomainId.NewGuid()),
    };

    // Must not throw
    await sut.HandleCronJobAsync(job, CancellationToken);

    A.CallTo(log)
        .Where(x => x.Method.Name == "Log" && x.GetArgument<LogLevel>(0) == LogLevel.Error)
        .MustHaveHappenedOnceExactly();
}
```

**Estimated diff**: +15 lines production code, +25 lines test code.  
**Verification**: `dotnet test --filter CronJobUpdater` → all tests pass.

