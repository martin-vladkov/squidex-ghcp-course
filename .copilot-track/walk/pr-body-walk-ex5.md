## Summary
- What changed and why: identified the three language-event types (`AppLanguageAdded`, `AppLanguageRemoved`, `AppLanguageUpdated`) as the critical contract boundary between `AppDomainObject` (producer) and event-store consumers (read-model projectors, webhooks, audit log); existing tests used `VerifySutAsync` / full snapshots which are useful but slow to read — added three focused event-shape contract tests that assert each required field directly on `LastEvents`; documented the contract surface and update instructions
- Plan:
  1. **Audit boundary** — read event types + base classes to enumerate required fields per event
  2. **File 1 (`AppDomainObjectTests.cs`):** add `using Squidex.Domain.Apps.Events.Apps`; add 3 contract tests (`AddLanguage_event_contract_payload_has_required_fields`, `RemoveLanguage_…`, `UpdateLanguage_…`) each asserting: correct event type, `Language` = command value, `AppId` ≠ default, `Actor` not null, `FromRule = false`; `UpdateLanguage` also asserts `IsOptional`, `IsMaster`, `Fallback`
  3. **File 2 (`ai-track-docs/contracts.md`):** document the event contract surface (field tables), the 3 contract test names, run command, step-by-step update instructions, and rationale
- Files/paths touched:
  - `backend/tests/Squidex.Domain.Apps.Entities.Tests/Apps/DomainObject/AppDomainObjectTests.cs` (1 using + 3 tests added)
  - `ai-track-docs/contracts.md` (new)

## Evidence
- Tests/logs/metrics:
  ```
  dotnet test … --filter "FullyQualifiedName~AppDomainObjectTests"
  → Passed! - Failed: 0, Passed: 40, Skipped: 0, Total: 40, Duration: 13 s
  ```
  (Was 37; +3 new contract tests: AddLanguage, RemoveLanguage, UpdateLanguage)
- Coverage: line/branch unchanged from Walk Ex2 baseline (no new production code); contract tests exercise the existing `AppDomainObject` language methods

## Risk & Rollback
- Risk: low — test-only and docs-only change; no production code modified
- Rollback: `git revert <this commit>`

## Review Focus
- **3 new contract tests** — verify each assertion comment matches the field's actual purpose (e.g. `// consumer: fallback chain must round-trip`); confirm `Assert.Single(LastEvents)` is correct (each language command raises exactly one event)
- **`AppLanguageUpdated` contract test** — `IsMaster` is asserted `false` because DE is not the master language (EN is by default after `CreateApp`); confirm this matches the domain rule
- **`ai-track-docs/contracts.md` § Updating the contract** — reviewer should read the 6-step process and confirm it covers all real-world cases (especially step 4: searching for downstream consumers)
- Reviewer can run: `dotnet test … --filter "FullyQualifiedName~AppDomainObjectTests&FullyQualifiedName~contract"` to isolate the 3 contract tests

## Track
- Level: Walk
- Exercise: Walk Ex5
