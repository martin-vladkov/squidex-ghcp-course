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
// 1. Call the shared helper from each operation method:
private void <Operation>(<Command> command)
{
    var sw = ValueStopwatch.StartNew();   // allocation-free; already used across Squidex.Infrastructure
    Raise(command, new <Event>());
    LogLanguageOp("<OperationName>", command.Language, sw.Stop());
}

// 2. The shared helper centralises the guard + message template:
private void LogLanguageOp(string op, Language language, long elapsedMs)
{
    if (logLanguageOps)
    {
        log.LogInformation("op={Op} status={Status} elapsed_ms={ElapsedMs} language={Language}",
            op, "ok", elapsedMs, language);
    }
}
```

> **Why `ValueStopwatch` instead of `System.Diagnostics.Stopwatch`?** `ValueStopwatch` is a `readonly struct` — no heap allocation. It is the timing primitive already used everywhere else in `Squidex.Infrastructure` (e.g. `LogCommandMiddleware`, `RequestLogPerformanceMiddleware`). Add domain-specific fields after `language=` as needed.

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
// Toggle resolved once at construction (fail-open):
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

// Shared logging helper (Walk Ex3 refactor — replaces 3×8-line duplicated blocks):
private void LogLanguageOp(string op, Language language, long elapsedMs)
{
    if (logLanguageOps)
    {
        log.LogInformation("op={Op} status={Status} elapsed_ms={ElapsedMs} language={Language}",
            op, "ok", elapsedMs, language);
    }
}
```

**Fail-open design:** if `IConfiguration` is unavailable or the value is unparseable (e.g. `"yes"` instead of `"true"`), construction never throws and language ops continue normally with logging enabled. A bad config value silences nothing and breaks nothing.

---

## Flag lifecycle — `Features:LogLanguageOps`

| Stage | Detail |
|-------|--------|
| **Created** | Crawl Ex13 — `AppDomainObject.cs` constructor reads the key once at startup |
| **Default** | `true` (logging **on**) — fail-open; missing or malformed value also defaults to `true` |
| **Enable** | Set `Features:LogLanguageOps=true` in appsettings, env var, or secrets |
| **Disable** | Set `Features:LogLanguageOps=false` — suppresses the three `LogInformation` calls; the OTel counter (`squidex.app.language_ops`) still fires regardless |
| **Env-var form** | `Features__LogLanguageOps=false` (double-underscore = config hierarchy separator in .NET) |
| **Removal criteria** | Remove when the structured log is either promoted to always-on (delete the `if` guard) or permanently deleted. Do not remove while any operator uses the flag to reduce log volume. |

### Validated states

| State | Test name | Assertion |
|-------|-----------|-----------|
| ON (default) | `AddLanguage_should_log_structured_operation` | `LogInformation` called ≥ 1× |
| ON (default) | `LogLanguageOp_emits_all_required_structured_fields` | op / status / elapsed_ms / language fields present |
| OFF | `AddLanguage_should_not_log_when_toggle_is_off` | `LogInformation` never called |
| Fail-open (throws) | `AddLanguage_should_fail_open_when_config_throws` | construction succeeds; logging on |
| Fail-open (malformed) | `AddLanguage_should_fail_open_when_toggle_value_is_malformed` | construction succeeds; logging on |

CI matrix: `.github/workflows/ghcp-walk-featureflags.yml` — runs `flag-on` and `flag-off` jobs in parallel with `Features__LogLanguageOps` set explicitly in each job's `env:`.

