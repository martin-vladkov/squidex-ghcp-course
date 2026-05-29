## Summary
- What changed and why: two test-infrastructure packages were on outdated versions across all 5 backend test projects; bumped both to their latest safe releases to keep the toolchain current and reduce vulnerability surface in the dev pipeline
- Plan:
  1. Run `dotnet list package --outdated` on both GHCP test projects to enumerate candidates
  2. Select two low-risk upgrades: `Microsoft.NET.Test.Sdk` 18.4.0 → 18.6.0 (minor — test runner SDK, zero production blast radius) and `coverlet.collector` 10.0.0 → 10.0.1 (patch — coverage collector, zero production blast radius)
  3. Skip `FluentAssertions` 7.0.0 → 8.10.0 (major — likely breaking assertion API changes) and `Meziantou.Analyzer` 3.0.50 → 3.0.98 (could introduce new enforced lint rules mid-exercise)
  4. Apply via `sed` across all 5 test `.csproj` files to keep the version consistent
  5. Restore, build, run both GHCP suites, document output
- Files/paths touched (all version-only changes, no logic):
  - `backend/tests/Squidex.Domain.Apps.Core.Tests/Squidex.Domain.Apps.Core.Tests.csproj`
  - `backend/tests/Squidex.Domain.Apps.Entities.Tests/Squidex.Domain.Apps.Entities.Tests.csproj`
  - `backend/tests/Squidex.Infrastructure.Tests/Squidex.Infrastructure.Tests.csproj`
  - `backend/tests/Squidex.Web.Tests/Squidex.Web.Tests.csproj`
  - `backend/tests/Squidex.Domain.Users.Tests/Squidex.Domain.Users.Tests.csproj`

## Evidence
- Tests/logs/metrics:
  ```
  # Squidex.Domain.Apps.Core.Tests (LanguagesConfigTests)
  dotnet test … --filter "FullyQualifiedName~LanguagesConfigTests"
  → Passed! - Failed: 0, Passed: 23, Skipped: 0, Total: 23

  # Squidex.Domain.Apps.Entities.Tests (AppDomainObjectTests)
  dotnet test … --filter "FullyQualifiedName~AppDomainObjectTests"
  → Passed! - Failed: 0, Passed: 40, Skipped: 0, Total: 40
  ```
- Coverage: unchanged — no production code touched; package upgrades are test runner and coverage collector only

## Risk & Rollback
- Risk: low — both upgrades are test-toolchain only (`Microsoft.NET.Test.Sdk`, `coverlet.collector`); no production assemblies, no public API surface changed
- Rollback (restore previous versions in all 5 csproj files):
  ```bash
  cd backend/tests
  find . -name "*.csproj" ! -path "*/obj/*" -exec sed -i '' \
    -e 's|Version="18.6.0"|Version="18.4.0"|g' \
    -e 's|coverlet.collector" Version="10.0.1"|coverlet.collector" Version="10.0.0"|g' \
    {} \;
  dotnet restore
  ```

## Review Focus
- **All 5 `.csproj` diffs** — confirm only the two package version strings changed; no other edits crept in
- **`FluentAssertions` 7→8 skipped** — major version jump carries high breakage risk (assertion method signatures changed in v8); deferred to a dedicated breaking-upgrade PR
- **`Meziantou.Analyzer` 3.0.50→3.0.98 skipped** — new minor version introduces additional enforced rules that could cause CI failures under `-warnaserror`; deferred for evaluation in a lint-focused PR
- Reviewer can reproduce: `dotnet restore && dotnet test … --filter "FullyQualifiedName~LanguagesConfigTests"` and `… --filter "FullyQualifiedName~AppDomainObjectTests"`

## Track
- Level: Walk
- Exercise: Walk Ex7
