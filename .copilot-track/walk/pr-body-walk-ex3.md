## Summary
- What changed and why: plan-first multi-file refactor to eliminate the 3×8-line duplicated guard+log blocks in `AppDomainObject` language methods — extracted into a single `LogLanguageOp` private helper; also switched from `System.Diagnostics.Stopwatch` to `ValueStopwatch` (the allocation-free struct already used everywhere else in `Squidex.Infrastructure`)
- Plan:
  1. **Identify:** `AddLanguage`, `RemoveLanguage`, `UpdateLanguage` each repeated the same 8-line pattern (`var sw = Stopwatch.StartNew()` → `if (logLanguageOps)` → `log.LogInformation(...)`)
  2. **File 1 (`AppDomainObject.cs`):** extract `LogLanguageOp(string op, Language language, long elapsedMs)` private method; replace 3 duplicated blocks with a single helper call each; swap `Stopwatch` for `ValueStopwatch`; remove `using System.Diagnostics`
  3. **File 2 (`AppDomainObjectTests.cs`):** add `LogLanguageOp_emits_all_required_structured_fields` contract test asserting `op=`, `status=`, `elapsed_ms=`, `language=` field names all survive the refactor
  4. **File 3 (`ai-track-docs/logging.md`):** update "Extending the pattern" template and "How it works" snippet to reflect the extracted helper and `ValueStopwatch`
- Files/paths touched:
  - `backend/src/Squidex.Domain.Apps.Entities/Apps/DomainObject/AppDomainObject.cs`
  - `backend/tests/Squidex.Domain.Apps.Entities.Tests/Apps/DomainObject/AppDomainObjectTests.cs`
  - `ai-track-docs/logging.md`

## Evidence
- Tests/logs/metrics:
  ```
  dotnet test … --filter "FullyQualifiedName~AppDomainObjectTests"
  → Passed! - Failed: 0, Passed: 37, Skipped: 0, Total: 37

  dotnet test … --filter "FullyQualifiedName~LanguagesConfigTests"
  → Passed! - Failed: 0, Passed: 22, Skipped: 0, Total: 22

  dotnet build src/Squidex.Domain.Apps.Entities/… -warnaserror
  → Build succeeded.
  ```
- Coverage: behavior unchanged (pure refactor); same Cobertura baseline as Walk Ex2 (Core 57.2%, Entities 56.9%)

## Risk & Rollback
- Risk: low — pure structural refactor; no behavior change; log message template string is identical
- Rollback: `git revert <this commit>`

## Review Focus
- **`AppDomainObject.cs` diff** — verify the `LogLanguageOp` helper body is identical to each removed block; confirm `using System.Diagnostics` import is gone; confirm `ValueStopwatch.Stop()` returns `long` (milliseconds) matching `sw.ElapsedMilliseconds` semantics
- **New contract test** — `LogLanguageOp_emits_all_required_structured_fields` asserts all four field prefixes are present in the formatted message; this will catch any future template change that silently drops a field
- **Reviewer can run:** `dotnet test … --filter "FullyQualifiedName~AppDomainObjectTests"` and confirm 37/37 pass

## Track
- Level: Walk
- Exercise: Walk Ex3
