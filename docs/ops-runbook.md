# Ops Runbook — Rules Pipeline Resilience

_Last updated: 2026-05-31 (Run Exercise 15)_

---

## Overview

The Rules pipeline processes cron-job triggers, enqueues flow instances, and tracks
rule usage statistics.  Three external call paths now carry exponential-backoff retry
via `RulesResilienceHelper`:

| Call path | Class | Retry target |
|-----------|-------|--------------|
| Usage-stat write (completed/failed) | `RuleFlowTrackingCallback` | `IRuleUsageTracker.TrackAsync` |
| Cron-job enqueue | `CronJobUpdater.HandleCronJobAsync` | `IRuleEnqueuer.EnqueueAsync` |
| Per-event usage-stat write | `RuleQueueWriter.WriteAsync` | `IRuleUsageTracker.TrackAsync` |

---

## Tuning parameters

All parameters live in the `rules` config section (`appsettings.json` or environment
variables using the `__` separator).

| Config key | Env variable | Default | Description |
|------------|-------------|---------|-------------|
| `rules:resilienceMaxAttempts` | `RULES__RESILIENCEMAXATTEMPTS` | `3` | Total attempts per operation (1 = retry disabled). Increase when backend latency spikes are common; keep ≤ 5 for interactive flows. |
| `rules:resilienceInitialDelayMs` | `RULES__RESILIENCEINITIALDELAYMS` | `200` | Base back-off in ms; doubles with every retry (200 → 400 → 800 …). Increase for batch/background jobs; decrease below 100 ms only for in-memory stores. |
| `rules:enableCronJobTrigger` | `RULES__ENABLECRONJOBTRIGGER` | `true` | Feature flag — set `false` to disable all cron-job triggering without deployment (Ex13). |

### Worked example

```jsonc
// appsettings.Production.json
"rules": {
  "resilienceMaxAttempts": 5,     // extra tolerance for unreliable network
  "resilienceInitialDelayMs": 500 // longer back-off for slow storage tier
}
```

---

## Transient vs. non-transient conditions

`RulesResilienceHelper` **retries** on:
- `IOException` — storage / network I/O errors
- `TimeoutException` — upstream service timeout
- `OperationCanceledException` where the **caller's token is NOT cancelled** — inner task timed out

`RulesResilienceHelper` **does NOT retry** (propagates immediately) on:
- Any other exception type (e.g., `ArgumentException`, `InvalidOperationException`)
- `OperationCanceledException` where the caller's token **IS cancelled** (honour it)

---

## Current behavior under failure

### Before Ex15 (no resilience)

| Failure | Outcome |
|---------|---------|
| `TrackAsync` throws `IOException` | Exception propagates to the ASP.NET Core host; the event-consumer task faults; the event is NOT re-processed |
| `EnqueueAsync` throws `IOException` | Cron-job event is silently dropped; rule execution is missed for that fire time |
| Storage latency spike | First call hangs until timeout; no automatic recovery |

### After Ex15 (resilience applied)

| Failure | Outcome |
|---------|---------|
| `TrackAsync` throws `IOException` | Retried up to `resilienceMaxAttempts − 1` times with exponential back-off; propagates only if all attempts fail |
| `EnqueueAsync` throws `IOException` | Same retry behaviour; cron-job event is only lost after all attempts are exhausted |
| Storage latency spike | Request times out; treated as transient `OperationCanceledException` and retried |

---

## Escalation steps

1. **Observe**: check `squidex.rules.*` OTel metrics for `cronjob_trigger_skipped`
   counter spikes and slow-latency percentiles (via your Grafana/Prometheus dashboard).

2. **Short-term relief — disable cron triggers**:
   ```bash
   # Set via environment variable and restart
   RULES__ENABLECRONJOBTRIGGER=false
   ```
   This stops all cron-job processing without a deployment (see Ex13 feature flag).

3. **Short-term relief — reduce retry noise**:
   ```bash
   RULES__RESILIENCEMAXATTEMPTS=1   # disable retries while diagnosing
   ```

4. **Investigate**: inspect application logs for `LogCronJobSkipped` / `LogFlowExecutionFailed`
   messages.  Each carries `ruleId` and `appId` for targeted investigation.

5. **Re-enable**: once the underlying storage issue is resolved, restore defaults:
   ```bash
   RULES__ENABLECRONJOBTRIGGER=true
   RULES__RESILIENCEMAXATTEMPTS=3
   RULES__RESILIENCEINITIALDELAYMS=200
   ```

---

## Rollback guidance

Resilience wrapping is purely additive — removing it reverts to direct `await` calls.

| File | Rollback action |
|------|----------------|
| `RuleFlowTrackingCallback.cs` | Replace `RulesResilienceHelper.ExecuteWithRetryAsync(...)` with a direct `await ruleUsageTracker.TrackAsync(...)`. Remove `IOptions<RulesOptions>` constructor param. |
| `CronJobUpdater.cs` | Replace wrapper with `await ruleEnqueuer.EnqueueAsync(...).ConfigureAwait(false)`. Remove `resilienceMaxAttempts`/`resilienceInitialDelayMs` fields. |
| `RuleQueueWriter.cs` | Replace wrapper with `await ruleUsageTracker.TrackAsync(...)`. Remove optional `IOptions<RulesOptions>?` constructor param. |
| `RulesResilienceHelper.cs` | Delete the file. |
| `RulesOptions.cs` | Remove `ResilienceMaxAttempts` and `ResilienceInitialDelayMs` properties. |

No database migrations or state changes are required.

---

## Related backlog items

See [backlog.md](backlog.md) — items BACK-6 through BACK-10 for Rules/ clean-up tasks.
