## GHCP — Crawl Exercise 7: Dependency hygiene

### What changed
- **New:** `ai-track-docs/dependencies.md` — full dependency audit covering:
  - Policy table (exact pins, no unreviewed major upgrades, CVE exemption)
  - Runtime version matrix (.NET 10, Node 22)
  - All direct NuGet packages for `Squidex.Domain.Apps.Core.Model` and its test project
  - Frontend npm critical vs. tooling deps with pin-style assessment
  - Identified pinning gaps + disposition for each
  - Update cadence recommendations per layer
  - CLI commands to check for outdated packages
- **Modified:** `frontend/package.json` — added `"engines": {"node": ">=22.0.0"}` to document the required Node runtime (only gap requiring a file change)

### Why these changes
All backend NuGet packages already use exact version pins. Frontend production deps (`@angular/*`) are pinned exactly; only tooling deps (`vitest`, `eslint`) use `^` ranges — acceptable for now. The only undocumented constraint was the Node runtime version, fixed with the `engines` field.

### Tests
No backend test changes. Existing 22 `LanguagesConfigTests` pass unchanged — `dotnet test` confirms `Failed: 0, Passed: 22`.

### Copilot guidance used
- Prompt: "List all direct NuGet packages in the csproj and assess pinning strategy"
- Prompt: "What gaps exist in runtime version documentation for this project?"

### Checklist
- [x] `ai-track-docs/dependencies.md` created
- [x] Node engines constraint added to `frontend/package.json`
- [x] No version upgrades performed
- [x] Existing tests green
