# Apps/Templates Subsystem

> **Path:** `backend/src/Squidex.Domain.Apps.Entities/Apps/Templates/`  
> **Tests:** `backend/tests/Squidex.Domain.Apps.Entities.Tests/Apps/Templates/`  
> **Last reviewed:** 2026-05 (Run Exercise 4)

---

## Overview

The Templates subsystem provides two independent capabilities inside the Squidex
app-creation flow:

| Capability | Entry point | What it does |
|---|---|---|
| **Template provisioning** | `TemplateCommandMiddleware` | After a `CreateApp` command completes, fetches a template from a remote Git repository and applies it via the Squidex CLI sync engine. |
| **AI schema generation** | `SchemaAIGenerator` | Given a natural-language prompt, calls an LLM to generate a schema definition and optionally seed content items. |

Both capabilities are **optional**: template provisioning activates only when
`CreateApp.Template` is non-null; AI generation is invoked explicitly.

---

## File map

```
Apps/Templates/
├── Template.cs                  Record DTO returned from TemplatesClient
├── TemplatesClient.cs           HTTP client — fetches template listings from GitHub
├── TemplatesOptions.cs          IOptions<T> configuration bound from appsettings
├── TemplateRepository.cs        Config value — one configured remote repository
├── TemplateCommandMiddleware.cs ICommandMiddleware — wires template provisioning
│                                into the CreateApp command pipeline
├── RetryPolicy.cs               Value object: MaxAttempts + InitialDelayMs
├── HttpRetryHelper.cs           Static retry executor with exponential backoff
├── SessionFactory.cs            Builds a Squidex CLI Session for template apply
├── StringLogger.cs              ILogger adapter that captures CLI output as a string
├── SchemaAIGenerator.cs         AI-driven schema generation (prompt → schema + content)
├── SchemaAIResult.cs            Result DTO: log lines + generated schema name
└── AIQueryCache.cs              IDistributedCache adapter for LLM response caching
```

---

## Template provisioning flow

```
POST /api/apps  (CreateApp command)
       │
       ▼
TemplateCommandMiddleware.HandleAsync()
       │  calls next(ctx, ct) first — app is created before template is applied
       │
       ▼  if createApp.Template != null
TemplatesClient.GetRepositoryUrl(name)
       │  iterates options.Repositories, fetches README.md via HTTP
       │  retries transient failures via HttpRetryHelper (RetryPolicy.Default: 3 attempts, 200 ms)
       │
       ▼  repository URL found
SessionFactory.CreateSession(app)
       │  reads first AppClient for credentials; uses options.LocalUrl or IUrlGenerator.Root()
       │
       ▼
Squidex CLI sync engine (Squidex.CLI)
       │  clones/reads the template repository, applies schemas, assets, content
       │
       ▼
StringLogger captures any warnings/errors → logged via ISemanticLog
```

**Key files:** [TemplateCommandMiddleware.cs](../backend/src/Squidex.Domain.Apps.Entities/Apps/Templates/TemplateCommandMiddleware.cs) · [TemplatesClient.cs](../backend/src/Squidex.Domain.Apps.Entities/Apps/Templates/TemplatesClient.cs) · [SessionFactory.cs](../backend/src/Squidex.Domain.Apps.Entities/Apps/Templates/SessionFactory.cs)

---

## AI schema generation flow

```
Caller invokes SchemaAIGenerator.ExecuteAsync(app, prompt, numberOfItems, execute)
       │
       ▼
AIQueryCache.GetOrCreateAsync(prompt)    ← IDistributedCache; avoids duplicate LLM calls
       │  cache miss → LLM call
       ▼
LLM response (GeneratedContent) parsed
       │
       ├─ WriteSchema()  → CLI creates schema
       └─ WriteContent() → CLI seeds up to MaxContentItems (= 20) content items
       │
       ▼
SchemaAIResult(Log, SchemaName) returned to caller
```

**Key files:** [SchemaAIGenerator.cs](../backend/src/Squidex.Domain.Apps.Entities/Apps/Templates/SchemaAIGenerator.cs) · [AIQueryCache.cs](../backend/src/Squidex.Domain.Apps.Entities/Apps/Templates/AIQueryCache.cs)

---

## Configuration reference

Bound via `IOptions<TemplatesOptions>` (section name matches the class name in DI registration).

```json
{
  "templates": {
    "localUrl": "https://localhost:5001",
    "repositories": [
      {
        "contentUrl": "https://raw.githubusercontent.com/Squidex/templates/main",
        "gitUrl": "https://github.com/Squidex/templates"
      }
    ]
  }
}
```

| Property | Type | Default | Description |
|---|---|---|---|
| `localUrl` | `string?` | `null` | Override the Squidex URL used by the CLI sync session. When `null`, `IUrlGenerator.Root()` is used. Useful in local dev/Docker where the public URL differs from the internal one. |
| `repositories[].contentUrl` | `string` | — | Base URL for raw file access (GitHub raw content). Used to fetch `README.md` and template details. |
| `repositories[].gitUrl` | `string?` | `null` | Git clone URL passed to the CLI sync engine. Falls back to `contentUrl` when null. |

**Real path:** [TemplatesOptions.cs](../backend/src/Squidex.Domain.Apps.Entities/Apps/Templates/TemplatesOptions.cs) · [TemplateRepository.cs](../backend/src/Squidex.Domain.Apps.Entities/Apps/Templates/TemplateRepository.cs)

---

## Retry policy

Template HTTP calls are wrapped by `HttpRetryHelper.ExecuteWithRetryAsync` with `RetryPolicy.Default`:

| Parameter | Default | Range | Notes |
|---|---|---|---|
| `MaxAttempts` | 3 | 1–10 | Total tries including the first. Set to 1 to disable. |
| `InitialDelayMs` | 200 ms | — | Base backoff; doubles each attempt (200 → 400 → 800 ms). Keep ≤ 2000 ms for interactive flows. |

To use a custom policy at a specific call site:
```csharp
await HttpRetryHelper.ExecuteWithRetryAsync(
    innerCt => httpClient.GetAsync(url, innerCt),
    policy: new RetryPolicy(5, 100),   // 5 attempts, 100 ms base delay
    ct: ct);
```

**Real paths:** [RetryPolicy.cs](../backend/src/Squidex.Domain.Apps.Entities/Apps/Templates/RetryPolicy.cs) · [HttpRetryHelper.cs](../backend/src/Squidex.Domain.Apps.Entities/Apps/Templates/HttpRetryHelper.cs)

---

## Extension guidance

### Add a new template repository
1. Add an entry to `TemplatesOptions.Repositories` in `appsettings.json` (or environment-variable override `TEMPLATES__REPOSITORIES__1__CONTENTURL=...`).
2. The `README.md` at `contentUrl/README.md` must list templates in the expected Markdown format (parsed by `TemplatesClient`'s `RegexTemplate`).
3. No code changes are required.

### Change the retry policy globally
Edit `RetryPolicy.Default` in [RetryPolicy.cs](../backend/src/Squidex.Domain.Apps.Entities/Apps/Templates/RetryPolicy.cs):
```csharp
public static readonly RetryPolicy Default = new RetryPolicy(5, 500);
```
All three call sites in `TemplatesClient` will pick up the change automatically.

### Change the retry policy for a single call site
Pass an explicit `RetryPolicy` to `HttpRetryHelper.ExecuteWithRetryAsync` as shown above.

### Add a new template-aware command
1. Register a new `ICommandMiddleware` that checks for your command type (see `TemplateCommandMiddleware` as the reference).
2. Re-use `SessionFactory` to obtain a CLI session and `HttpRetryHelper` for any outbound HTTP calls.

---

## Risk notes

| Risk | Severity | Mitigation |
|---|---|---|
| GitHub raw-content rate limiting | Medium | `HttpRetryHelper` retries up to 3 times with exponential backoff. For heavy traffic, increase `MaxAttempts` or cache responses in `IDistributedCache`. |
| Template application failure (CLI sync) | Medium | Failures are logged via `ISemanticLog` but are **non-fatal** — the app is already created before the template is applied. The user will have an empty app rather than an error. |
| Missing `AppClient` for CLI session | High | `SessionFactory.CreateSession` calls `.First()` on `app.Clients` with no null guard. If an app is created without any client, this will throw. Ensure `InitialSettings` always seeds at least one client. |
| AI quota exhaustion | Low | `AIQueryCache` caches LLM responses in `IDistributedCache`. The same prompt within the cache TTL will not re-invoke the LLM. |
| `localUrl` misconfiguration | Low | If `localUrl` points to an unreachable host, the CLI sync session will fail silently (logged as a warning). Set `localUrl: null` in production to use `IUrlGenerator`. |
| `gitUrl` null fallback | Low | When `gitUrl` is null, `contentUrl` is passed to the CLI as the git remote. This works for GitHub raw URLs but may break for other hosts that separate raw and git endpoints. |

---

## Testing

| Test file | What it covers |
|---|---|
| [HttpRetryHelperTests.cs](../backend/tests/Squidex.Domain.Apps.Entities.Tests/Apps/Templates/HttpRetryHelperTests.cs) | Retry on `HttpRequestException`, timeout, exhaustion, non-transient pass-through |
| [TemplatesClientTests.cs](../backend/tests/Squidex.Domain.Apps.Entities.Tests/Apps/Templates/TemplatesClientTests.cs) | Template listing and detail fetching against a stubbed HTTP handler |

Run with:
```bash
dotnet test backend/tests/Squidex.Domain.Apps.Entities.Tests/Squidex.Domain.Apps.Entities.Tests.csproj \
  --filter "FullyQualifiedName~Apps.Templates"
```
