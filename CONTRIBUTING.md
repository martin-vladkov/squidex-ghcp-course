# Contributing to Squidex (GHCP Track)

This document describes how to contribute using the **GitHub Copilot (GHCP) Walk** methodology — a plan-first, evidence-backed workflow designed to build trust between AI-assisted authorship and human reviewers.

---

## Quick links

| Resource | Path |
|---|---|
| Architecture map | [docs/architecture.md](docs/architecture.md) |
| Walk onboarding prompt | [docs/onboarding-walk.md](docs/onboarding-walk.md) |
| Coverage instructions | [README.md § Running Tests and Coverage](README.md#running-tests-and-coverage) |
| PR template | [.github/PULL_REQUEST_TEMPLATE.md](.github/PULL_REQUEST_TEMPLATE.md) |

---

## Branching strategy

| Scenario | Branch name | Base branch |
|---|---|---|
| Crawl exercise N | `exercise-N` | `exercise-(N-1)` |
| Walk exercise N | `walk-exercise-N` | `walk-exercise-(N-1)` |
| Hotfix / one-off | `fix/<short-description>` | `main` or the relevant exercise branch |

```
# Walk exercise example
git checkout -b walk-exercise-5 walk-exercise-4
```

Rules:
- One logical change per branch. Keep scope small and reviewable.
- Never commit directly to `main` or `exercise-*` branches.
- Always branch from the **previous exercise** so that PRs show only the incremental delta.

---

## Plan-first workflow

Before writing any code, produce a written plan and include it in the PR description.

**Minimum plan content:**
1. What problem is being solved and why
2. Files to change (2–4 maximum for Walk exercises)
3. Expected impact on tests and coverage
4. Rollback strategy

**Copilot mini-prompt pattern:**
```
Create a plan first: list steps and files to change.
Then produce diffs file-by-file.
Include: test strategy, evidence to capture, and rollback.
Keep scope small and reviewable.
```

---

## PR expectations

Every PR **must** include all five sections of the PR template:

```markdown
## Summary         — what + why + plan link + files touched
## Evidence        — test output (copy-paste), coverage percentage
## Risk & Rollback — risk level (low/medium/high) + revert command
## Review Focus    — where to look + verification steps the reviewer can run
## Track           — Level (Crawl/Walk/Run) + Exercise number
```

### Evidence requirements

| Item | Required |
|---|---|
| `dotnet test` output snippet (pass/fail counts) | Always |
| Coverage % (line + branch) for changed suites | Walk and above |
| Build with `-warnaserror` clean | Always for `Core.Model` changes |
| Diagram render / lint output | When architecture or diagram files are changed |

### How to run tests and capture evidence

```bash
# Targeted (fastest — use for PR evidence)
cd backend
dotnet test tests/Squidex.Domain.Apps.Entities.Tests/… \
  --filter "FullyQualifiedName~AppDomainObjectTests" \
  --logger "console;verbosity=minimal"

# Full GHCP suites
./test-crawl.sh   # from repo root

# Coverage
cd backend/tests
dotnet test Squidex.Domain.Apps.Entities.Tests/… \
  --collect "XPlat Code Coverage" \
  --results-directory ./_coverage-out \
  --settings coverlet.runsettings.xml
```

See [README.md § Running Tests and Coverage](README.md#running-tests-and-coverage) for the full coverage workflow.

---

## Using Copilot on this track

### Do

- **Plan first.** Ask Copilot to produce a numbered plan before any diffs.
- **Review every diff** before accepting. Understand what changed and why.
- **Include evidence.** Paste real terminal output — never fabricate numbers.
- **Keep scope small.** Walk exercises touch 2–4 files maximum.

### Don't

- Accept a diff you haven't read.
- Let Copilot write tests that only assert `true` or `!= null`.
- Skip the PR template sections — they exist so reviewers can trust AI-assisted output.
- Commit generated files (migration outputs, coverage XML, `_coverage-out/`) — they are in `.gitignore`.

### Recommended Copilot Chat pattern for a new exercise

```
I am working on [repo name]. The current branch is [branch].
My task: [exercise description].

Steps:
1. Propose a plan (files to change, reason, impact).
2. Show me diffs file-by-file. Wait for my approval between files.
3. After all diffs, show me the exact test command to run as evidence.
4. Draft the PR description using the project PR template.
```

---

## Code style

- **C# / .NET:** StyleCop is enforced (`TreatWarningsAsErrors=true` on `Core.Model`). Run `dotnet build -warnaserror` before pushing.
- **No `System.Diagnostics.Stopwatch`** in `Squidex.Domain.Apps.Entities` — use `ValueStopwatch` from `Squidex.Infrastructure`.
- **Structured logging only** — `log.LogInformation("key={Value}", value)`, never string interpolation in log calls.
- **No PII in logs** — see `ai-track-docs/security-hygiene.md`.
