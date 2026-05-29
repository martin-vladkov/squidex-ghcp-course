# Performance Baseline

> **Module:** `Squidex.Domain.Apps.Core.Model` — `LanguagesConfig`  
> **Measured:** Crawl Exercise 6 (2026-05-29)  
> **Machine:** macOS arm64 (Apple Silicon), .NET 10, Release build via `dotnet test`  
> **Method:** `Stopwatch` over 100 000 iterations with 1 000-iteration warm-up; results via `ITestOutputHelper`

---

## Baseline: `LanguagesConfig.GetPriorities`

**Setup:** 3-language config (`en` master, `de` non-optional, `it` optional with `de` fallback). Measured on path: non-master language with one explicit fallback.

| Run | Total (ms) | Per op (ns) |
|-----|-----------|-------------|
| 1   | 33.4      | 334         |
| 2   | 32.2      | 322         |
| 3   | 31.3      | 313         |
| 4   | 31.7      | 317         |

**Summary**

| Metric | Value |
|--------|-------|
| Min    | 313 ns/op |
| Max    | 334 ns/op |
| Range  | ~21 ns (~7%) |

**Variance notes:**
- Spread of ~7% is normal for a `Stopwatch` benchmark without process isolation — thermal/OS scheduling noise.
- BenchmarkDotNet would give tighter numbers (~1–2% variance) but the magnitude would be similar.
- The result is dominated by the `yield return` iterator + `ToList()` allocation, not the dictionary lookup.

---

## How to re-run

```bash
cd backend
dotnet test tests/Squidex.Domain.Apps.Core.Tests/Squidex.Domain.Apps.Core.Tests.csproj \
  --filter "Category=Perf" \
  --logger "console;verbosity=detailed"
```

Filter the output for `Per op` lines. Run 3–5 times and record the range.

---

## Optimization candidates (not yet actioned)

| Opportunity | Estimated saving | Risk |
|-------------|-----------------|------|
| Replace `GetPriorities` iterator with `ArrayPool`-backed list | ~50 ns/op | medium — changes return type contract |
| Cache fallback list as `string[]` instead of re-iterating `ReadonlyList` | ~20 ns/op | low |

> **Do not optimize yet.** Record candidates here; act only when a profiled hot-path justifies it.

---

## Future baselines to add

- [ ] `LanguagesConfig.Set` (mutation — allocates new dict)
- [ ] `LanguagesConfig.Contains` (trivial — expected < 10 ns/op)
- [ ] `LanguagesConfig.GetPriorities` on master language (short-circuit path)
