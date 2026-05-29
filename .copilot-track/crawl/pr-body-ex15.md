## Summary
- Extracted `logLanguageOps` initialization to `ResolveLogLanguageOps` static helper in `AppDomainObject` — wraps `IConfiguration.GetValue<bool>` in try/catch; on any exception defaults to `true` (fail-open) so a malformed or unavailable config value never crashes `AppDomainObject` construction or blocks language operations
- Added `AddLanguage_should_fail_open_when_config_throws` — fake `IConfiguration` whose `GetSection` throws `InvalidOperationException`; verifies construction succeeds and logging still fires (exercises the catch path directly)
- Added `AddLanguage_should_fail_open_when_toggle_value_is_malformed` — in-memory config with `"Features:LogLanguageOps" = "not-a-bool"`; verifies construction succeeds and logging still fires (exercises the type-conversion failure path)
- Updated `ai-track-docs/logging.md` — Toggle section now shows the full `ResolveLogLanguageOps` helper with fail-open rationale
- Files touched: `AppDomainObject.cs`, `AppDomainObjectTests.cs`, `ai-track-docs/logging.md`

## Review focus
- **Critical file:** `AppDomainObject.cs` — verify the `catch (Exception)` is inside `ResolveLogLanguageOps` (static, not instance) and that the comment clearly explains fail-open intent; check that all three `if (logLanguageOps)` blocks still have braces (SA1519 fix from Ex14)
- **Check that:** `AddLanguage_should_fail_open_when_config_throws` asserts `MustHaveHappenedOnceOrMore` (not `Exactly(1)`) to tolerate any initialization-time log calls from the base class; the two new tests each create their own `sut` instance via `PublishAsync(…, new CreateApp {…})` so they don't conflict with the shared `sut`
- **Not in scope:** applying the same fail-open pattern to other toggle reads elsewhere in the codebase; retrying failed logging calls (logging failures are always silently dropped by design)

## Evidence
- Tests/logs/metrics:
  ```
  dotnet test … --filter "FullyQualifiedName~AppDomainObjectTests"
  → Passed! - Failed: 0, Passed: 36, Skipped: 0, Total: 36, Duration: 1 s
  ```
- Prompt used with Copilot: "Add one minimal resilience improvement (timeout/retry/backoff or error mapping) with 1–2 failure tests and brief documentation."

## Risk & Rollback
- Risk: low — behavior is identical when config is healthy; only exception paths change
- Rollback: `git revert <this commit>`

## Track
- Level: Crawl
- Exercise: Ex15
