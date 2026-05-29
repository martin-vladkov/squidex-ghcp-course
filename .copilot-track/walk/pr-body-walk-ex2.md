## Summary
- What changed and why: surfaced coverage numbers that were already being collected (Coverlet + XPlat Code Coverage) but never documented; added step-by-step coverage instructions to README so any contributor can reproduce results locally; added a PR snippet template for keeping coverage visible in future PRs
- Plan: locate existing config (`coverlet.runsettings.xml`, `RunCoverage.ps1`) → run two GHCP-relevant suites → parse Cobertura XML → document commands + baseline numbers + HTML report steps → add PR snippet template
- Files/paths touched:
  - `README.md` — new "Running Tests and Coverage" section (commands, baseline table, PR snippet template)

## Evidence
- Tests/logs/metrics:
  ```
  # Suite 1 — Squidex.Domain.Apps.Core.Tests
  dotnet test … --collect "XPlat Code Coverage" --settings coverlet.runsettings.xml
  → Failed: 1 (pre-existing: StringFormatterTests geolocation), Passed: 1245, Total: 1246

  # Suite 2 — Squidex.Domain.Apps.Entities.Tests
  dotnet test … --collect "XPlat Code Coverage" --settings coverlet.runsettings.xml
  → Failed: 2 (pre-existing: FFMpeg tests), Passed: 1528, Total: 1530
  ```
  > The pre-existing failures are unrelated to this exercise (FFMpeg binary absent in CI, geolocation format test environment-specific).

- Coverage (from Cobertura XML, parsed via python3):
  ```
  Suite: Squidex.Domain.Apps.Core.Tests
    Line:   57.2 %   Branch: 57.2 %
    Packages: Core.Model · Core.Operations · Events · Infrastructure · Shared

  Suite: Squidex.Domain.Apps.Entities.Tests
    Line:   56.9 %   Branch: 48.4 %
    Packages: + Entities · Users · Web
  ```

## Risk & Rollback
- Risk: low — README-only change; no production or test code modified
- Rollback: `git revert <this commit>`

## Review Focus
- **README "Running Tests and Coverage" section** — verify the `dotnet test` commands reproduce the numbers above on your machine; confirm the PowerShell note points to `RunCoverage.ps1 -testAll`
- **Baseline table** — numbers reflect the two GHCP exercise suites only (not the full project); check the table note is clear enough to avoid confusion with a "total project coverage" figure
- **PR snippet template** — reviewer can copy it into a future PR to confirm the format works end-to-end

## Track
- Level: Walk
- Exercise: Walk Ex2
