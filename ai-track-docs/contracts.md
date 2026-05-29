# Event Contracts — Language Operations

This document describes the **event envelope contract** for the three language-lifecycle operations in `AppDomainObject`. Any event-store consumer (read-model projector, webhook trigger, audit log) that reads these events depends on this contract.

---

## Boundary

**Producer:** `AppDomainObject`
(`backend/src/Squidex.Domain.Apps.Entities/Apps/DomainObject/AppDomainObject.cs`)

**Contract surface:** The event payload written to the event store when a language command is processed. Consumers read these events via the `EventSourcing` infrastructure (`backend/src/Squidex.Infrastructure/EventSourcing/`).

---

## Event types

### `AppLanguageAdded`

File: `backend/src/Squidex.Domain.Apps.Events/Apps/AppLanguageAdded.cs`

| Field | Type | Required | Consumer reads |
|---|---|---|---|
| `Language` | `Language` | Yes | Which language was added |
| `AppId` | `NamedId<DomainId>` | Yes | Which app owns this event |
| `Actor` | `RefToken` | Yes | Who triggered the command |
| `FromRule` | `bool` | Yes | `false` when user-initiated |

### `AppLanguageRemoved`

File: `backend/src/Squidex.Domain.Apps.Events/Apps/AppLanguageRemoved.cs`

Same fields as `AppLanguageAdded` — `Language`, `AppId`, `Actor`, `FromRule`.

### `AppLanguageUpdated`

File: `backend/src/Squidex.Domain.Apps.Events/Apps/AppLanguageUpdated.cs`

| Field | Type | Required | Consumer reads |
|---|---|---|---|
| `Language` | `Language` | Yes | Which language was updated |
| `AppId` | `NamedId<DomainId>` | Yes | Which app |
| `Actor` | `RefToken` | Yes | Who triggered the command |
| `FromRule` | `bool` | Yes | `false` when user-initiated |
| `IsOptional` | `bool` | Yes | Whether content in this language is optional |
| `IsMaster` | `bool` | Yes | Whether this is now the master language |
| `Fallback` | `Language[]?` | No | Ordered fallback chain (nullable) |

---

## Contract tests

The contract is validated by three focused tests in `AppDomainObjectTests`:

```
AppDomainObjectTests.AddLanguage_event_contract_payload_has_required_fields
AppDomainObjectTests.RemoveLanguage_event_contract_payload_has_required_fields
AppDomainObjectTests.UpdateLanguage_event_contract_payload_has_required_fields
```

These tests use `LastEvents` (the in-memory event list captured by `HandlerTestBase`) and assert each required field directly — without comparing the full sut snapshot. They fail fast if a field is renamed, removed, or gets an unexpected default value.

Run them:

```bash
cd backend
dotnet test tests/Squidex.Domain.Apps.Entities.Tests/… \
  --filter "FullyQualifiedName~AppDomainObjectTests&FullyQualifiedName~contract" \
  --logger "console;verbosity=normal"
```

---

## Updating the contract

> Do this when a field is **intentionally** added, renamed, or removed. If you are just refactoring internals without changing the event shape, no contract update is needed.

1. **Change the event type** in `backend/src/Squidex.Domain.Apps.Events/Apps/`.
2. **Update the contract test assertion** in `AppDomainObjectTests` — the test with `_event_contract_payload_has_required_fields` in its name.
3. **Update the table** in this file (the field table above).
4. **Check downstream consumers** — search for usages of the event type:
   ```bash
   grep -r "AppLanguageAdded\|AppLanguageRemoved\|AppLanguageUpdated" backend/src --include="*.cs" -l
   ```
5. **Update the snapshot** (`Verify/*.verified.txt`) if `VerifySutAsync` calls reference the old shape. Run the test suite and let Verify regenerate the snapshot, then review and commit the new `.verified.txt` file.
6. **Document the breaking change** in `CHANGELOG.md` if the event is consumed by external projectors or webhooks.

---

## Why these tests exist

The Squidex architecture uses event sourcing: the event payload is the **only durable record** of a write operation. If a field is silently dropped or renamed in the event type, every consumer that depends on it will break — potentially silently (projectors get `null` instead of a value). These contract tests catch that at the unit-test level, before the change reaches CI or production.
