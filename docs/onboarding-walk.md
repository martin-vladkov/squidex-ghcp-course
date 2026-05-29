# Walk Onboarding Prompt

> **How to use this file:** paste the block below into GitHub Copilot Chat at the start of a new Walk exercise session. It gives Copilot the context it needs to help you follow the plan-first, evidence-backed workflow.

---

## Paste this into Copilot Chat

```
## Context — Squidex GHCP Walk track

Repository: squidex-ghcp-course (fork of Squidex headless CMS)
Language: C# / ASP.NET Core / .NET 10, xUnit v3, Coverlet

### Key paths
- Entry point:        backend/src/Squidex/Program.cs
- Main domain object: backend/src/Squidex.Domain.Apps.Entities/Apps/DomainObject/AppDomainObject.cs
- Core model:         backend/src/Squidex.Domain.Apps.Core.Model/
- Domain events:      backend/src/Squidex.Domain.Apps.Events/
- Infrastructure:     backend/src/Squidex.Infrastructure/
- GHCP exercise tests:
    backend/tests/Squidex.Domain.Apps.Core.Tests/Model/Apps/LanguagesConfigTests.cs
    backend/tests/Squidex.Domain.Apps.Entities.Tests/Apps/DomainObject/AppDomainObjectTests.cs

### Architecture overview
See docs/architecture.md — layer table + Mermaid component diagram + 3 data flows.

### Conventions (must follow)
- Use ValueStopwatch (not System.Diagnostics.Stopwatch) for timing
- Structured logging only: log.LogInformation("key={Value}", value)
- No PII in logs (see ai-track-docs/security-hygiene.md)
- StyleCop enforced on Core.Model — run: dotnet build -warnaserror
- Tests: dotnet test … --filter "FullyQualifiedName~AppDomainObjectTests"

### Walk workflow (plan-first)
1. Write a numbered plan before any code: files to change, reason, impact, rollback
2. Produce diffs file-by-file; max 2-4 files per exercise
3. Run tests and paste real output as evidence — never fabricate numbers
4. Fill every PR template section: Summary / Evidence / Risk & Rollback / Review Focus / Track

### PR template sections (copy into every PR)
## Summary         — what + why + plan + files touched
## Evidence        — dotnet test output, coverage % (line + branch)
## Risk & Rollback — low/medium/high + git revert <sha> or toggle <flag>
## Review Focus    — key diff areas + commands reviewer can run to verify
## Track           — Level: Walk | Exercise: Walk ExN

### Branching
git checkout -b walk-exercise-N walk-exercise-(N-1)

### Test commands
# Targeted
cd backend && dotnet test tests/Squidex.Domain.Apps.Entities.Tests/… \
  --filter "FullyQualifiedName~AppDomainObjectTests" --logger "console;verbosity=minimal"

# Coverage
cd backend/tests && dotnet test Squidex.Domain.Apps.Entities.Tests/… \
  --collect "XPlat Code Coverage" --results-directory ./_coverage-out \
  --settings coverlet.runsettings.xml

### My current task
[REPLACE THIS LINE with the exercise description]
```

---

## What to expect from Copilot

After pasting the block above and adding your task, Copilot should:

1. **Propose a numbered plan** — files to change, reason, expected impact
2. **Pause for your approval** before producing any diffs
3. **Show diffs file-by-file** — one file at a time, waiting for review
4. **Provide the exact test command** to run for evidence
5. **Draft the PR description** using the five-section template

If Copilot skips the plan and jumps straight to code, redirect it:

> "Stop. Give me a numbered plan first — list each file, what changes, and why. No diffs yet."

---

## Baseline coverage (Walk Ex2)

| Suite | Line % | Branch % |
|---|---|---|
| `Squidex.Domain.Apps.Core.Tests` | 57.2 % | 57.2 % |
| `Squidex.Domain.Apps.Entities.Tests` | 56.9 % | 48.4 % |

Include the delta vs this baseline in every PR Evidence section.
