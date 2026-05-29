# Dependency Hygiene

> **Updated:** Crawl Exercise 7 (2026-05-29)  
> **Policy owner:** Track participants — review at each Walk/Run phase start

---

## Policy summary

| Rule | Detail |
|------|--------|
| No unreviewed major upgrades | All major-version bumps require a dedicated PR with regression evidence |
| Backend: exact pins only | `Version="x.y.z"` — no `*`, `~`, or `^` ranges in `.csproj` files |
| Frontend: exact pins for Angular, ranges acceptable for tooling | See table below |
| Runtime versions documented here | Node, .NET SDK documented; deviations need a PR update |
| Security patches are exempt from freeze | CVE patches may skip the major-upgrade review but still need a test run |

---

## Runtime versions (locally verified 2026-05-29)

| Runtime | Version | Pinned? | Pin location |
|---------|---------|---------|--------------|
| .NET SDK | 10.0.103 | ✅ via `<TargetFramework>net10.0</TargetFramework>` in every `.csproj` | Per-project csproj |
| Node.js | 22.22.0 | ⚠️ `engines` field added (Ex7) | `frontend/package.json` |
| npm | bundled with Node | — | — |

---

## Backend NuGet — chosen module (`Squidex.Domain.Apps.Core.Model`)

| Package | Version | Role | Pin style |
|---------|---------|------|-----------|
| `NetTopologySuite` | 2.6.0 | Geospatial types | Exact ✅ |
| `NodaTime.Serialization.SystemTextJson` | 1.3.1 | Date/time JSON | Exact ✅ |
| `Microsoft.Extensions.Configuration.UserSecrets` | 10.0.6 | Dev secrets | Exact ✅ |
| `Squidex.Flows` | 8.0.1 | Internal rule engine | Exact ✅ |
| `System.ComponentModel.Annotations` | 5.0.0 | Validation attributes | Exact ✅ |
| `StyleCop.Analyzers` *(build-only)* | 1.1.118 | Style enforcement | Exact ✅ |
| `Meziantou.Analyzer` *(build-only)* | 3.0.50 | Extra roslyn rules | Exact ✅ |
| `RefactoringEssentials` *(build-only)* | 5.6.0 | Roslyn refactoring | Exact ✅ |

**Assessment:** all NuGet packages use exact version pins — no action required.

---

## Backend NuGet — test project (`Squidex.Domain.Apps.Core.Tests`)

| Package | Version | Note |
|---------|---------|------|
| `xunit.v3` | 3.2.2 | Test framework — exact ✅ |
| `xunit.runner.visualstudio` | 3.1.5 | VS runner — exact ✅ |
| `Microsoft.NET.Test.Sdk` | 18.4.0 | Test SDK — exact ✅ |
| `FluentAssertions` | `[7.0.0]` | Bracket pin (NuGet exact range) ✅ |
| `FakeItEasy` | 9.0.1 | Mocking — exact ✅ |
| `coverlet.collector` | 10.0.0 | Coverage — exact ✅ |

---

## Frontend npm (`frontend/package.json`)

### Production dependencies (critical)

| Package | Version | Pin style | Risk if unpinned |
|---------|---------|-----------|-----------------|
| `@angular/*` | 21.1.3 | Exact ✅ | Breaking API changes |
| `angular-gridster2` | 21.0.1 | Exact ✅ | Layout breakage |

### Dev / tooling dependencies

| Package | Version | Pin style | Acceptable? |
|---------|---------|-----------|-------------|
| `typescript` | 5.9.3 | Exact ✅ | — |
| `vitest` | `^4.0.8` | Range ⚠️ | Acceptable for tooling; pin if flaky |
| `eslint` | `^9.39.2` | Range ⚠️ | Acceptable; may silently add new rules |
| `@typescript-eslint/*` | `^8.54.0` | Range ⚠️ | Acceptable; pin on ESLint rule churn |
| `@angular/cli` | `^21.1.3` | Range ⚠️ | Pin to exact when Angular major changes |
| `@lithiumjs/angular` | `^9.0.0` | Range ⚠️ | Minor risk; watch for breaking changes |

**Recommendation:** ranges on tooling (vitest, eslint) are acceptable. If CI becomes flaky on a tooling bump, pin exact at that point.

---

## Pinning gaps identified (Ex7)

| Gap | Severity | Action taken |
|-----|----------|-------------|
| Node.js version not documented | Low | Added `"engines": {"node": ">=22.0.0"}` to `frontend/package.json` |
| No `.nvmrc` / `volta` config | Low | Documented here; add `.nvmrc` if team uses nvm |
| `vitest`, `eslint` on `^` ranges | Low | Acceptable for now; pin if CI flakes |

---

## Update cadence recommendations

| Layer | Cadence | Trigger |
|-------|---------|---------|
| .NET patch (x.y.**z**) | Monthly | Dependabot / manual |
| Angular patch | With each Angular release | Check angular.io/changelog |
| Security CVE (any layer) | Within 48 h | GitHub security advisory |
| Major version (.NET, Angular, xUnit) | Per track Walk/Run phase | Dedicated PR with full test run |

---

## How to check for outdated packages

```bash
# Backend
cd backend
dotnet list package --outdated

# Frontend
cd frontend
npm outdated
```
