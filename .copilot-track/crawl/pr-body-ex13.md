## Summary
- Added `Features:LogLanguageOps` feature toggle to `AppDomainObject` — `IConfiguration` key read once at construction; defaults to `true`; when `false` the three `LogInformation` calls in `AddLanguage`, `RemoveLanguage`, `UpdateLanguage` are silenced without code change or redeploy
- Added `AddLanguage_should_not_log_when_toggle_is_off` test — builds `IConfiguration` in-memory with `false`, constructs an `offSut`, verifies `LogLevel.Information` containing `"AddLanguage"` is never called
- Updated `ai-track-docs/logging.md` — added Toggle section with key table, example config snippet, and how-it-works explanation
- Files touched: `AppDomainObject.cs`, `AppDomainObjectTests.cs`, `ai-track-docs/logging.md`

## Review focus
- **Critical file:** `AppDomainObject.cs` — verify `logLanguageOps` field is resolved with `?.GetValue<bool>(…, defaultValue: true) ?? true` (two levels of null-safety) so missing `IConfiguration` never breaks the constructor
- **Check that:** toggle-off test uses a fully initialised `offSut` (owns its own `CreateApp` event) so it isn't affected by `sut`'s fake-persistence snapshot; `MustNotHaveHappened` assertion is strict enough
- **Not in scope:** persisting the toggle in `appsettings.json` (gitignored environment overrides are the deployment mechanism)

## Evidence
- Tests/logs/metrics:
  ```
  dotnet test … --filter "FullyQualifiedName~AppDomainObjectTests"
  → Passed! - Failed: 0, Passed: 34, Skipped: 0, Total: 34, Duration: 591 ms
  ```
- Prompt used with Copilot: "Add a small safe toggle around a non-critical behavior with ON/OFF tests and brief documentation."

## Risk & Rollback
- Risk: low — defaults to `true`; existing behavior unchanged unless config key explicitly set to `false`
- Rollback: `git revert <this commit>`

## Track
- Level: Crawl
- Exercise: Ex13
