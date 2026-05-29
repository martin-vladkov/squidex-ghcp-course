## Summary
- Added `ai-track-docs/dependencies.md` — full NuGet/npm dependency audit, pinning policy, update cadence, and CLI commands for checking outdated packages
- Added `"engines": {"node": ">=22.0.0"}` to `frontend/package.json` — only undocumented pinning gap found (all NuGet packages already use exact version pins)
- No version upgrades performed
- Files touched: `ai-track-docs/dependencies.md`, `frontend/package.json`

## Evidence
- Tests/logs/metrics:
  ```
  dotnet test tests/Squidex.Domain.Apps.Core.Tests/Squidex.Domain.Apps.Core.Tests.csproj \
    --filter "FullyQualifiedName~LanguagesConfigTests"
  → Passed! - Failed: 0, Passed: 22, Skipped: 0, Total: 22
  ```

## Risk & Rollback
- Risk: low
- Rollback: git revert d8f28573c

## Track
- Level: Crawl
- Exercise: Ex7
