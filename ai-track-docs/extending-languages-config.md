# Extending `LanguagesConfig`

Module: `backend/src/Squidex.Domain.Apps.Core.Model/Apps/`  
Test coverage: `backend/tests/Squidex.Domain.Apps.Core.Tests/Model/Apps/LanguagesConfigTests.cs`

---

## What it does

`LanguagesConfig` is an **immutable value object** that maps ISO 639-1 language keys to their configuration (optional flag, fallback priority list). Every app has exactly one `LanguagesConfig`; the master language is always required and non-optional.

---

## Key invariants (enforced by `Cleanup`)

| Invariant | Where enforced |
|-----------|----------------|
| At least one language must exist | `Build` returns `this` if result would be empty |
| Master is always a key in `Values` | `Cleanup` falls back to first key if requested master is missing |
| Master can never be optional or have fallbacks | `Cleanup` resets master's config to `LanguageConfig.Default` |
| Fallback list cannot contain the language itself or keys not in `Values` | `LanguageConfig.Cleanup` strips invalid entries |
| Returns the same instance if nothing changed | `Build` identity check via `EqualsDictionary` |

---

## How to add a new read-only query

1. Add a method/property to `LanguagesConfig.cs`.
2. Add an XML `///` doc comment explaining the return value and null-safety.
3. Add a `[Fact]` test in `LanguagesConfigTests.cs` following the `Should_<verb>_<what>` naming pattern.

```csharp
// Example: count configured languages
/// <summary>Total number of configured languages, including the master.</summary>
public int Count => values.Count;
```

---

## How to add a new mutation

All mutations must be `[Pure]` — return a **new** `LanguagesConfig`, never mutate `this`.

```csharp
[Pure]
public LanguagesConfig MyMutation(Language language)
{
    Guard.NotNull(language);

    var newLanguages = new Dictionary<string, LanguageConfig>(values);
    // ... modify newLanguages ...
    return Build(newLanguages, master); // Build handles identity + Cleanup
}
```

Call `Build` — never construct `new LanguagesConfig(...)` directly from a mutation method. `Build` enforces all invariants via `Cleanup` and returns `this` when unchanged.

---

## How to add a fallback/priority behaviour change

`GetPriorities` drives content fallback resolution. If you need a new resolution strategy:
1. Do **not** modify `GetPriorities` directly — it is called on hot paths.
2. Add a parallel method (e.g., `GetPrioritiesStrict`) so existing consumers are unaffected.
3. Cover the new method with at least one happy-path and one edge-case test.

---

## Test pattern

```csharp
[Fact]
public void Should_<verb>_<what>()
{
    var config =
        LanguagesConfig.English   // start from the well-known singleton
            .Set(Language.DE)
            .Set(Language.IT, true, Language.DE);

    // assert using plain xUnit — no mocks, no I/O
    Assert.True(config.Contains((string)Language.DE));
}
```

Run the test suite:

```bash
cd backend
dotnet test tests/Squidex.Domain.Apps.Core.Tests/Squidex.Domain.Apps.Core.Tests.csproj \
  --filter "FullyQualifiedName~LanguagesConfigTests"
```
