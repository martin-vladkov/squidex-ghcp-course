## Summary
- What changed and why: created `docs/architecture.md` — maps every conceptual layer (HTTP, entities, core model, infrastructure, persistence) to its real file path; includes a Mermaid component diagram and three data flows (write path, read path, bootstrap)
- Plan: explore entry points → draft layer table + component diagram → add 3 data flows → add CI render validation workflow + local pre-check script
- Files/paths touched:
  - `docs/architecture.md` (new) — architecture doc with Mermaid diagram + data flows
  - `.github/workflows/ghcp-walk-architecture.yml` (new) — CI job that installs `mmdc` and renders the diagram on every `walk-*` push/PR
  - `validate-diagram.sh` (new) — lightweight local script that validates the Mermaid block exists and starts with a recognised diagram type (no mmdc install required)

## Evidence
- Tests/logs/metrics:
  ```
  $ ./validate-diagram.sh
  OK: Mermaid block found in docs/architecture.md
      Type   : graph
      Lines  : 40

  Diagram preview (first 5 lines):
  graph TD
      subgraph HTTP["HTTP Layer"]
          A["Program.cs\nbackend/src/Squidex/Program.cs"]
  ```
- Coverage: n/a (documentation exercise); CI workflow uses `mmdc` full render as the contract

## Risk & Rollback
- Risk: low — docs-only addition; no production code changed; CI workflow is additive
- Rollback: `git revert <this commit>` or delete `docs/architecture.md` + the two new files

## Review Focus
- **`docs/architecture.md`** — verify each path in the layer table exists in the repo; check that the three data flows (write, read, bootstrap) match your mental model of the system
- **`.github/workflows/ghcp-walk-architecture.yml`** — the `mmdc` render step uses `--puppeteerConfigFile <(echo '...')` process substitution; verify this works on the CI runner (ubuntu-latest)
- **`validate-diagram.sh`** — reviewer can run `./validate-diagram.sh` locally to confirm extraction logic; exit code 0 = pass

## Track
- Level: Walk
- Exercise: Walk Ex1
