## Summary
- Added XML `///` doc comments to `LanguagesConfig` class, `English` static field, and the 5 members changed in Ex3 (`Master`, `AllKeys`, `Values`, `IsMaster`, `Contains`)
- Created `ai-track-docs/extending-languages-config.md` covering invariants, mutation pattern, query extension pattern, and test pattern
- No logic changed — comments and docs only
- Files touched: backend/src/Squidex.Domain.Apps.Core.Model/Apps/LanguagesConfig.cs, ai-track-docs/extending-languages-config.md

## Evidence
- Tests/logs/metrics:
  ```
  dotnet test tests/Squidex.Domain.Apps.Core.Tests/Squidex.Domain.Apps.Core.Tests.csproj \
    --filter "FullyQualifiedName~LanguagesConfigTests"
  → Passed! - Failed: 0, Passed: 20, Skipped: 0, Total: 20, Duration: 76 ms
  ```

## Risk & Rollback
- Risk: low
- Rollback: git revert 7a255e13a5bda7e6e60e5791d253756e5caf60b7

## Track
- Level: Crawl
- Exercise: Ex4
