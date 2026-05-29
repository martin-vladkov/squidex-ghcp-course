## Summary
- What changed and why: no `CONTRIBUTING.md` existed; created it with the Walk plan-first workflow, branching strategy, PR template requirements, evidence rules, and Copilot usage guidelines; created `docs/onboarding-walk.md` as a single-page copy-paste prompt for Copilot Chat; linked both from README
- Plan:
  1. Audit existing docs — `CONTRIBUTING.md` absent; `docs/` had only `architecture.md`
  2. **File 1 (`CONTRIBUTING.md`):** create — branching table, plan-first workflow, PR template section requirements, evidence checklist, Copilot do/don't list, code style rules
  3. **File 2 (`docs/onboarding-walk.md`):** create — single fenced code block covering repo paths, conventions, workflow, PR template, test commands; plus "what to expect" guidance and baseline coverage table
  4. **File 3 (`README.md`):** add two sentences linking both new files from the existing Contributing section
- Files/paths touched:
  - `CONTRIBUTING.md` (new)
  - `docs/onboarding-walk.md` (new)
  - `README.md` (Contributing section updated)

## Evidence
- Tests/logs/metrics: no production or test code changed; documentation-only exercise
  ```
  git diff --stat walk-exercise-3..walk-exercise-4
  CONTRIBUTING.md        | <new>
  docs/onboarding-walk.md| <new>
  README.md              | 4 lines changed
  ```
- Coverage: unchanged from Walk Ex2 baseline (Core 57.2 % line, Entities 56.9 % line) — no code touched

## Risk & Rollback
- Risk: low — documentation-only; no production or test code modified
- Rollback: `git revert <this commit>` or delete `CONTRIBUTING.md` + `docs/onboarding-walk.md` and revert `README.md`

## Review Focus
- **`CONTRIBUTING.md`** — verify branching table matches actual branch convention used in PRs #1–19; check evidence checklist covers all items reviewers currently look for; confirm Copilot do/don't list reflects what worked and failed during Crawl exercises
- **`docs/onboarding-walk.md`** — reviewer should paste the block into a fresh Copilot Chat and confirm the model produces a numbered plan before any diffs; check that key paths are accurate against current repo structure
- **`README.md`** — confirm the two new links resolve correctly on GitHub

## Track
- Level: Walk
- Exercise: Walk Ex4
