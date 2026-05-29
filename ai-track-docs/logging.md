# Structured Logging

> **Updated:** Crawl Exercise 9 (2026-05-29)

---

## Pattern

All language-lifecycle operations in `AppDomainObject` emit a structured log entry with these consistent fields:

| Field | Type | Description |
|-------|------|-------------|
| `op` | string | Operation name (`AddLanguage`, `RemoveLanguage`, `UpdateLanguage`) |
| `status` | string | `"ok"` on success (errors surface via the framework exception handler) |
| `elapsed_ms` | long | Wall-clock milliseconds for the `Raise()` call |
| `language` | string | ISO 639-1 language code (e.g. `"de"`) |

### Example log output (JSON sink)

```json
{
  "Timestamp": "2026-05-29T14:02:01.123Z",
  "Level": "Information",
  "MessageTemplate": "op={Op} status={Status} elapsed_ms={ElapsedMs} language={Language}",
  "Properties": {
    "Op": "AddLanguage",
    "Status": "ok",
    "ElapsedMs": 0,
    "Language": "de",
    "SourceContext": "Squidex.Domain.Apps.Entities.Apps.DomainObject.AppDomainObject"
  }
}
```

---

## Where it lives

**File:** `backend/src/Squidex.Domain.Apps.Entities/Apps/DomainObject/AppDomainObject.cs`

Three private methods each follow the same pattern:

```csharp
private void AddLanguage(AddLanguage command)
{
    var sw = Stopwatch.StartNew();
    Raise(command, new AppLanguageAdded());
    log.LogInformation("op={Op} status={Status} elapsed_ms={ElapsedMs} language={Language}",
        "AddLanguage", "ok", sw.ElapsedMilliseconds, command.Language);
}
```

> `log` is the `ILogger<AppDomainObject>` injected via the primary constructor. The base `DomainObject<T>` also receives it but stores it as private — `AppDomainObject` captures it independently for use in these methods.

---

## How to view logs locally

### Console (default dev sink)

Run the backend and watch stdout. With the default `appsettings.Development.json` template, Squidex writes plain-text logs to console. The log line looks like:

```
info: Squidex.Domain.Apps.Entities.Apps.DomainObject.AppDomainObject[0]
      op=AddLanguage status=ok elapsed_ms=0 language=de
```

### JSON structured output (Serilog)

Squidex uses Serilog. To switch to JSON format, set in `appsettings.Development.json`:

```json
{
  "Serilog": {
    "WriteTo": [
      { "Name": "Console", "Args": { "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact" } }
    ]
  }
}
```

Then pipe to `jq` for readable output:

```bash
dotnet run --project backend/src/Squidex 2>&1 | jq 'select(.Op == "AddLanguage")'
```

### Filter by operation in production

When using a structured log aggregator (e.g. Seq, Datadog, Azure Monitor), query:

```
Op = "AddLanguage" AND Status = "ok"
```

or for latency tracking:

```
Op IN ["AddLanguage","RemoveLanguage","UpdateLanguage"] | stats avg(ElapsedMs)
```

---

## Extending the pattern

When adding structured logs to new domain object operations, follow this template:

```csharp
private void <Operation>(<Command> command)
{
    var sw = Stopwatch.StartNew();
    Raise(command, new <Event>());
    log.LogInformation("op={Op} status={Status} elapsed_ms={ElapsedMs}",
        "<OperationName>", "ok", sw.ElapsedMilliseconds);
    // Add domain-specific fields after elapsed_ms as needed
}
```

**Do not log sensitive data** (PII, token values, connection strings) — see [security-hygiene.md](security-hygiene.md).

---

## Toggle — `Features:LogLanguageOps`

Language-op logging can be silenced without redeploying by setting a configuration key:

| Key | Type | Default | Effect |
|-----|------|---------|--------|
| `Features:LogLanguageOps` | `bool` | `true` | `false` suppresses the three `LogInformation` calls in `AddLanguage`, `RemoveLanguage`, `UpdateLanguage` |

### How to disable (example `appsettings.Development.json`)

```json
{
  "Features": {
    "LogLanguageOps": false
  }
}
```

### How it works

`AppDomainObject` reads the key once at construction time via a resilient static helper:

```csharp
private readonly bool logLanguageOps = ResolveLogLanguageOps(serviceProvider);

private static bool ResolveLogLanguageOps(IServiceProvider serviceProvider)
{
    try
    {
        return serviceProvider.GetService<IConfiguration>()
            ?.GetValue<bool>("Features:LogLanguageOps", defaultValue: true) ?? true;
    }
    catch (Exception)
    {
        // Config value is malformed or config service threw — fail-open.
        return true;
    }
}
```

**Fail-open design:** if `IConfiguration` is unavailable or the value is unparseable (e.g. `"yes"` instead of `"true"`), construction never throws and language ops continue normally with logging enabled. A bad config value silences nothing and breaks nothing.
