# CI Baseline

> **Updated:** Crawl Exercise 10 (2026-05-29)

---

## Two paths to run checks

| Path | When to use |
|------|-------------|
| **GitHub Actions** (`.github/workflows/ghcp-crawl.yml`) | Automatic on every push/PR to any `exercise-*` branch |
| **Local script** (`test-crawl.sh`) | Before pushing; reproduces CI exactly with no setup |

---

## GitHub Actions — `ghcp-crawl.yml`

**Trigger:** `push` or `pull_request` targeting any branch matching `exercise-*`.

**What it runs:**

| Step | Command | Tests |
|------|---------|-------|
| Core.Model suite | `dotnet test … --filter "FullyQualifiedName~LanguagesConfigTests"` | 22 |
| Entities suite | `dotnet test … --filter "FullyQualifiedName~AppDomainObjectTests"` | 33 |

**Note:** The existing `dev.yml` workflow only covers `master`/`release/*` and requires Docker + full test containers. `ghcp-crawl.yml` is intentionally lightweight — pure unit tests, no Docker, completes in ~2 minutes.

**How to read results:**  
→ GitHub repo → Actions tab → "GHCP Crawl — Unit Tests" → click the run → expand each step.

---

## Local script — `test-crawl.sh`

```bash
./test-crawl.sh
```

Runs both suites sequentially, prints colored PASSED/FAILED per suite, exits non-zero if any suite fails. Requires .NET 10 SDK on PATH (same as CI).

```
=== Core.Model — LanguagesConfigTests ===
…
PASSED: Core.Model — LanguagesConfigTests

=== Entities — AppDomainObjectTests ===
…
PASSED: Entities — AppDomainObjectTests

Results: 2 passed, 0 failed
```

---

## Adding a new test suite

1. Add a `run_suite` call to `test-crawl.sh`:
   ```bash
   run_suite \
     "MyProject — MyTests" \
     "$BACKEND/tests/MyProject.Tests/MyProject.Tests.csproj" \
     "FullyQualifiedName~MyTests"
   ```

2. Add a matching step to `.github/workflows/ghcp-crawl.yml`:
   ```yaml
   - name: Test — MyProject (MyTests)
     run: |
       dotnet test \
         backend/tests/MyProject.Tests/MyProject.Tests.csproj \
         --filter "FullyQualifiedName~MyTests" \
         --logger "console;verbosity=normal"
   ```

---

## Relationship to existing CI

```
dev.yml          → master / release/*   → Docker build + TestContainers + Mongo + SQL + Playwright
ghcp-crawl.yml   → exercise-*           → Unit tests only (fast, no Docker)
```

The two workflows are independent. Merging exercise branches into master (if done) would then also fall under `dev.yml`.
