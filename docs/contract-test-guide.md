# Contract Test Guide

This document describes the contract test strategy for Squidex's domain layer, the
rationale for the tests added in Run Exercise 5, and the process for keeping them
current as the codebase evolves.

## What is a "contract" in this codebase?

Squidex uses a CQRS architecture.  The REST API layer (DTOs in
`backend/src/Squidex/Areas/Api/Controllers/Apps/Models/`) translates HTTP requests
into domain commands.  The **domain Guard classes** then validate those commands
before they are applied to the aggregate root.

The Guard layer is the true API contract:

```
HTTP Request
  → DTO (data annotations: [LocalizedRequired], [LocalizedStringLength], …)
  → ToCommand()
  → Guard.CanXxx(command, app)   ← contract boundary
  → DomainObject.Apply(command)
```

Any invariant that must hold across every client (REST, gRPC, internal) must be
enforced in the Guard class and tested here.

## Where contract tests live

| Layer | Project | Path |
|-------|---------|------|
| Domain Guard tests | `Squidex.Domain.Apps.Entities.Tests` | `Apps/DomainObject/Guards/` |

There is **no** separate "contract test" project; the Guard test files _are_ the
contract tests.

### Existing Guard test files

| File | Guards covered |
|------|----------------|
| `GuardAppTests.cs` | `GuardApp` — create, upload image, settings, transfer, plan |
| `GuardAppClientsTests.cs` | `GuardAppClients` — attach, revoke, update client |
| `GuardAppRolesTests.cs` | `GuardAppRoles` — add, delete, update role |
| `GuardAppContributorsTests.cs` | `GuardAppContributors` — assign, remove contributor |
| `GuardAppLanguagesTests.cs` | `GuardAppLanguages` — add, remove, update language |
| `GuardAppWorkflowTests.cs` | `GuardAppWorkflows` — add, delete, update workflow |

## Tests added in Run Exercise 5

### Boundary: `GuardAppLanguages.CanUpdate` — language promotion path

**File**: `GuardAppLanguagesTests.cs`

**Gap identified**: `GuardAppLanguages.CanUpdate` contains the following conditional:

```csharp
if (languages.IsMaster(language) || command.IsMaster)
{
    if (command.IsOptional)        { e("Master language cannot be made optional.", …); }
    if (command.Fallback?.Length > 0) { e("Master language cannot have fallback languages.", …); }
}
```

The pre-existing tests only exercised the `languages.IsMaster(language)` branch
(i.e., updating `Language.EN` which is already the master).  The `command.IsMaster`
branch — used when a client promotes a non-master language to master — was
completely untested.  A client could send:

```json
PATCH /api/apps/{app}/languages/de
{ "isMaster": true, "isOptional": true }
```

and the Guard would still reject it, but no test verified this behaviour was
preserved across refactors.

**Tests added**:

| Test name | What it verifies |
|-----------|-----------------|
| `CanUpdateLanguage_should_throw_exception_when_promoting_to_master_with_optional_flag` | `command.IsMaster = true` + `IsOptional = true` → validation error |
| `CanUpdateLanguage_should_throw_exception_when_promoting_to_master_with_fallbacks` | `command.IsMaster = true` + `Fallback != null` → validation error |

**Rationale**: These tests pin the `command.IsMaster` path so that future changes
to the promotion logic cannot silently remove the constraint.

## How to add new contract tests

### Step 1 — Identify the Guard method

Find the Guard method that enforces the invariant you want to pin.  Guard classes
live in:

```
backend/src/Squidex.Domain.Apps.Entities/Apps/DomainObject/Guards/
```

### Step 2 — Map every branch

Read the Guard method and list every `if` / `else` branch.  For each branch ask:

- Is there already a test that reaches it?
- Does the test use a realistic input (not one that coincidentally satisfies
  multiple conditions)?

### Step 3 — Write the test

Follow the existing naming convention:

```
Can<Action>_should_[not_]throw_exception_if_<condition>
```

Use `ValidationAssert.Throws` for expected validation errors and
`Assert.Throws<DomainObjectNotFoundException>` for not-found errors.  For happy
paths, simply call the Guard method without wrapping it — an exception from the
Guard will fail the test automatically.

**Minimal template**:

```csharp
[Fact]
public void CanXxx_should_throw_exception_if_<condition>()
{
    var command = new XxxCommand { /* fields that trigger the branch */ };

    // Mutate App state if the branch depends on app data:
    // App = App with { … };

    ValidationAssert.Throws(() => GuardAppXxx.CanXxx(command, App),
        new ValidationError("<expected message>", "<expected field>"));
}
```

### Step 4 — Run the tests

```bash
dotnet test backend/tests/Squidex.Domain.Apps.Entities.Tests/Squidex.Domain.Apps.Entities.Tests.csproj \
  --no-restore \
  --filter "FullyQualifiedName~GuardApp" \
  -v normal
```

### Step 5 — Keep tests in sync with Guard changes

When a Guard method is modified:

1. Re-read the full method and regenerate the branch map (Step 2).
2. Check that every added / changed / removed branch has a corresponding test
   change.
3. Run the Guard filter (Step 4) to confirm everything is green before committing.

## Running all Guard tests

```bash
dotnet test backend/tests/Squidex.Domain.Apps.Entities.Tests/Squidex.Domain.Apps.Entities.Tests.csproj \
  --no-restore \
  --filter "FullyQualifiedName~GuardApp"
```

Expected output (at the time this guide was written): **56 passing, 0 failing**.
