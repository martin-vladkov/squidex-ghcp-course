# Walk Backlog — Observability & Quality Epic

Items identified during Walk exercises Ex1–Ex11. Each item is scoped to one PR.
Acceptance criteria are written as testable bullet points.

---

## BACK-1 — Wire Prometheus exporter for `squidex.app.language_ops`

**Background**  
Walk Ex9 added the OTel counter `squidex.app.language_ops` (meter `Squidex.Apps`)
and registered it in the MeterProvider. The counter is collected in-process but
there is no Prometheus exporter, so operators cannot scrape
`squidex_app_language_ops_total` from a `/metrics` endpoint.

**Code paths**
- `backend/src/Squidex/Config/Domain/TelemetryServices.cs` — add
  `AddPrometheusExporter()` to the MeterProviderBuilder
- `backend/src/Squidex/appsettings.json` — add `logging:prometheus:enabled`
  flag (default `false`)
- `backend/src/Squidex/Program.cs` or `Startup.cs` — map `/metrics` endpoint
  when the flag is on

**Acceptance criteria**
- [ ] `appsettings.json` has `logging:prometheus:enabled` (default `false`)
- [ ] `GET /metrics` returns Prometheus text format including
  `squidex_app_language_ops_total{op="AddLanguage",language="en"}` when the
  flag is `true`
- [ ] The `/metrics` endpoint is not registered when the flag is `false`
- [ ] curl evidence or integration test captured in PR Evidence section
- [ ] Rollback: flip flag to `false` — endpoint disappears without redeploy

**Dependencies**: Walk Ex9 (AppMetrics.cs) must be merged first.  
**Package required**: `OpenTelemetry.Exporter.Prometheus.AspNetCore`

---

## BACK-2 — Extend language-op metric to all other AppDomainObject operations

**Background**  
`AppMetrics.LanguageOps` only counts the three language commands. The same
`AppDomainObject` handles ~20 other commands (contributor, client, role,
workflow, plan changes) with no metric coverage.

**Code paths**
- `backend/src/Squidex.Domain.Apps.Entities/Apps/DomainObject/AppDomainObject.cs`
  — existing `LogLanguageOp` pattern; generalise to `LogDomainOp` and call from
  all private command handlers
- `backend/src/Squidex.Domain.Apps.Entities/Apps/DomainObject/AppMetrics.cs`
  — add a second counter `squidex.app.domain_ops` tagged by `op` and `aggregate`

**Acceptance criteria**
- [ ] New counter `squidex.app.domain_ops` is defined in `AppMetrics.cs`
- [ ] All private handlers in `AppDomainObject.cs` call the new counter
- [ ] `MeterListener` test verifies at least one non-language op (e.g.
  `AttachClient`) is counted correctly
- [ ] 0 new StyleCop warnings
- [ ] Existing `AddLanguage_increments_language_ops_counter` test still passes

**Dependencies**: BACK-1 (optional; can be implemented without Prometheus).

---

## BACK-3 — Add hard coverage threshold for `Squidex.Domain.Apps.Entities` assembly

**Background**  
Walk Ex10 added a soft coverage gate (advisory only, `continue-on-error: true`).
The `Squidex.Domain.Apps.Entities` assembly is the most actively worked-on
assembly in the Walk exercises and has good test coverage for the language-op
code paths. A hard threshold (e.g. 60% line coverage on that assembly alone)
would catch regressions without gating the full build.

**Code paths**
- `.github/workflows/ghcp-walk-coverage.yml` — add a second job or step that
  runs `reportgenerator` with `-reporttypes:Cobertura` and then checks the
  `line-rate` attribute in the output XML for the one assembly
- Keep the existing advisory summary job unchanged

**Acceptance criteria**
- [ ] New CI step reads the Cobertura XML and extracts the line-rate for
  `Squidex.Domain.Apps.Entities`
- [ ] Step exits non-zero if line-rate < 0.60 (60%), causing the job to fail
- [ ] Threshold is set in a top-level `env:` variable so it is easy to adjust
- [ ] PR evidence includes actual line-rate figure from CI output
- [ ] The existing advisory summary step remains `continue-on-error: true`

**Dependencies**: Walk Ex10 (coverage workflow) must be merged first.

---

## BACK-4 — Run gitleaks in full git-history mode on CI

**Background**  
Walk Ex8 added `gitleaks detect --no-git` (file scan only). This misses secrets
that were committed and then deleted — they remain in the git history and are
still recoverable. Running `gitleaks detect` (without `--no-git`) scans the
full commit history and catches those cases.

**Code paths**
- `.github/workflows/ghcp-walk-security.yml` — the `gitleaks/gitleaks-action@v2`
  step already fetches full history (`fetch-depth: 0`); remove `--no-git` from
  the implicit default or confirm the action uses history mode by default

**Acceptance criteria**
- [ ] CI step runs gitleaks in history-scan mode (not `--no-git`)
- [ ] All 28 known-safe values from `.gitleaks.toml` are suppressed in history
  mode as well (verify with `gitleaks detect --source . --config .gitleaks.toml`)
- [ ] CI run passes with 0 new leaks detected
- [ ] PR Evidence section includes `gitleaks detect` output showing 0 leaks

**Dependencies**: Walk Ex8 (`.gitleaks.toml`) must be merged first.  
**Note**: `gitleaks/gitleaks-action@v2` already defaults to history mode when
`fetch-depth: 0` is set — confirm this is actually the case before implementing.

---

## BACK-5 — Raise `LanguagesConfig.Cleanup` coverage with property-based tests

**Background**  
Walk Ex6 optimised `LanguagesConfig.Cleanup` (`Fallbacks.Any()` → `Count > 0`,
`ToList()` → `Keys.ToArray()`). The existing `Perf_Set_Cleanup_baseline` test
measures timing but does not assert correctness across arbitrary inputs (e.g.
a language that is its own fallback, or a missing master language). Property-
based tests would catch edge cases that unit tests miss.

**Code paths**
- `backend/tests/Squidex.Domain.Apps.Core.Tests/Model/Apps/LanguagesConfigTests.cs`
  — add 2-3 `[Theory]` tests using `FsCheck.Xunit` or `CsCheck` covering:
  1. Cleanup removes all fallbacks for languages not in the active set
  2. Master language is never removed even if it appears in a fallback list
  3. Round-trip: applying Cleanup twice produces the same result as once
- `backend/tests/Squidex.Domain.Apps.Core.Tests/Squidex.Domain.Apps.Core.Tests.csproj`
  — add `FsCheck.Xunit` or `CsCheck` package reference

**Acceptance criteria**
- [ ] At least 3 property-based test cases added with clear property descriptions
- [ ] All 3 properties hold for 1 000 random inputs (default `[Property]` runs)
- [ ] Tests run in CI via the existing `ghcp-walk-coverage.yml` filter
- [ ] 0 new StyleCop warnings
- [ ] PR Evidence section shows the test pass count including the new tests

**Dependencies**: Walk Ex6 (Cleanup optimisation) must be merged.

---

## Priority order

| # | Item | Effort | Risk | Value |
|---|------|--------|------|-------|
| 1 | BACK-4 (gitleaks history mode) | XS | Low | High — closes a real security gap |
| 2 | BACK-3 (hard coverage threshold) | S | Low | High — enforces quality floor |
| 3 | BACK-1 (Prometheus exporter) | S | Low | Medium — enables production visibility |
| 4 | BACK-2 (extend domain-op metric) | M | Low | Medium — broader observability |
| 5 | BACK-5 (property-based tests) | M | Low | Medium — robustness for edge cases |
