## Summary
- Added structured `LogInformation` calls (fields: `op`, `status`, `elapsed_ms`, `language`) to `AppDomainObject`'s three language-lifecycle private methods: `AddLanguage`, `RemoveLanguage`, `UpdateLanguage`
- Used `Stopwatch.StartNew()` around `Raise()` to capture elapsed_ms; `status="ok"` always at this point (guard already passed; errors surface via framework exception handler)
- Promoted `log` from constructor local to class field in `AppDomainObjectTests` to enable assertion
- Added `AddLanguage_should_log_structured_operation` test — asserts `LogLevel.Information` was emitted containing `"AddLanguage"`
- Added `ai-track-docs/logging.md` — pattern reference, example JSON output, how-to-view (console, Serilog JSON, aggregator queries), and extension template
- Files touched: `AppDomainObject.cs`, `AppDomainObjectTests.cs`, `ai-track-docs/logging.md`

## Evidence
- Tests/logs/metrics:
  ```
  dotnet test tests/Squidex.Domain.Apps.Entities.Tests/Squidex.Domain.Apps.Entities.Tests.csproj \
    --filter "FullyQualifiedName~AppDomainObjectTests"
  → Passed! - Failed: 0, Passed: 33, Skipped: 0, Total: 33, Duration: 7 s
  ```

## Risk & Rollback
- Risk: low — logging only; no domain logic or event shape changed
- Rollback: git revert <this commit>

## Track
- Level: Crawl
- Exercise: Ex9
