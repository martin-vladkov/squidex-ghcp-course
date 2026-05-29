## Summary
- Fixed 3 × SA1519 warnings in `AppDomainObject.cs` — added braces to `if (logLanguageOps)` multi-line bodies (our own code introduced in Ex13; StyleCop requires braces on all multi-line child statements)
- Added `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` to `Squidex.Domain.Apps.Core.Model.csproj` — the module was already at 0 warnings; this locks it clean for all future changes
- Added `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>` to the same csproj — activates IDE/style analyzer rules during `dotnet build`, not just inside the IDE
- Updated `.github/workflows/ghcp-crawl.yml` — added `-warnaserror` to the Core.Model test step so CI fails if a warning is introduced
- Files touched: `AppDomainObject.cs`, `Squidex.Domain.Apps.Core.Model.csproj`, `.github/workflows/ghcp-crawl.yml`

## Review focus
- **Critical files:** `Squidex.Domain.Apps.Core.Model.csproj` — verify `TreatWarningsAsErrors` and `EnforceCodeStyleInBuild` are inside the unconditional `<PropertyGroup>` (not the Debug-only group); `AppDomainObject.cs` — verify all 3 `if (logLanguageOps)` blocks now have `{ }` braces
- **Check that:** `dotnet build src/Squidex.Domain.Apps.Core.Model/… --no-incremental` produces 0 warnings and 0 errors; `dotnet build src/Squidex.Domain.Apps.Entities/… --no-incremental` produces 0 SA1519 warnings
- **Not in scope:** adding `TreatWarningsAsErrors` to other projects (they may have pre-existing warnings not yet inventoried)

## Evidence
- Tests/logs/metrics:
  ```
  ./test-crawl.sh
  PASSED: Core.Model — LanguagesConfigTests   (22 passed)
  PASSED: Entities — AppDomainObjectTests     (34 passed)
  Results: 2 passed, 0 failed

  dotnet build src/Squidex.Domain.Apps.Core.Model/… --no-incremental
  → Build succeeded. (0 warnings, 0 errors)

  dotnet build src/Squidex.Domain.Apps.Entities/… --no-incremental
  → Build succeeded. (0 SA1519 warnings)
  ```
- Prompt used with Copilot: "If linting exists, tighten one path and fix a few high-signal findings."

## Risk & Rollback
- Risk: low — fixes are mechanical (braces); `TreatWarningsAsErrors` only on Core.Model which was already clean
- Rollback: `git revert <this commit>`

## Track
- Level: Crawl
- Exercise: Ex14
