## Summary
- Added `.github/workflows/ghcp-crawl.yml` — GitHub Actions workflow that triggers on `push`/`pull_request` for all `exercise-*` branches; runs both unit test suites (no Docker, no containers) using `dotnet test` with named filters
- Added `test-crawl.sh` — local fallback script at repo root; runs the same two suites with colored output and a non-zero exit code on any failure; executable (`chmod +x`)
- Added `ai-track-docs/ci.md` — documents both paths (CI and local), how to read results, how to add future suites
- Files touched: `.github/workflows/ghcp-crawl.yml`, `test-crawl.sh`, `ai-track-docs/ci.md`

## Evidence
- Tests/logs/metrics:
  ```
  ./test-crawl.sh
  PASSED: Core.Model — LanguagesConfigTests   (22 passed)
  PASSED: Entities — AppDomainObjectTests     (33 passed)
  Results: 2 passed, 0 failed
  exit=0
  ```

## Risk & Rollback
- Risk: low — new files only; no existing workflow modified
- Rollback: git revert <this commit>

## Track
- Level: Crawl
- Exercise: Ex10
