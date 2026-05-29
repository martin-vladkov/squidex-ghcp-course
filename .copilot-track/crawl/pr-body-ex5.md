## Summary
- Added `Guard.NotEmpty` validation to `LanguagesConfig` constructor — passing an empty language map now throws a clear `ArgumentException` instead of an opaque `InvalidOperationException` from inside `Cleanup`
- Added negative test `Should_throw_for_empty_language_map` proving the guard fires
- Files touched: backend/src/Squidex.Domain.Apps.Core.Model/Apps/LanguagesConfig.cs, backend/tests/Squidex.Domain.Apps.Core.Tests/Model/Apps/LanguagesConfigTests.cs

## Evidence
- Tests/logs/metrics:
  ```
  dotnet test tests/Squidex.Domain.Apps.Core.Tests/Squidex.Domain.Apps.Core.Tests.csproj \
    --filter "FullyQualifiedName~LanguagesConfigTests"
  → Passed! - Failed: 0, Passed: 21, Skipped: 0, Total: 21, Duration: 71 ms
  ```
  (was 20 before — +1 negative test)

## Risk & Rollback
- Risk: low
- Rollback: git revert 53b8f6bbcce7163433002dcb501c6570ff164952

## Track
- Level: Crawl
- Exercise: Ex5
