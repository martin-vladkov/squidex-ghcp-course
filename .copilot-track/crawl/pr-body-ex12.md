## Summary
- Added `ai-track-docs/BACKLOG-EX12.md` — 5 backlog items with acceptance criteria and code links, derived from findings across Crawl exercises 1–11; no tracker access so committed as markdown
- No code or test changes
- Files touched: `ai-track-docs/BACKLOG-EX12.md`

## Review focus
- **Critical file:** `ai-track-docs/BACKLOG-EX12.md` — verify each item traces to a real finding, has ≥3 acceptance criteria checkboxes, and includes a working code link
- **Check that:** items 1–5 are distinct, prioritised, and actionable without ambiguity; effort estimates are realistic; no item duplicates work already completed in Ex1–11
- **Not in scope:** actually implementing any of the 5 items; importing into Azure Boards (no tracker access)

## Evidence
- Tests/logs/metrics:
  ```
  ./test-crawl.sh
  PASSED: Core.Model — LanguagesConfigTests   (22 passed)
  PASSED: Entities — AppDomainObjectTests     (33 passed)
  Results: 2 passed, 0 failed
  ```
- Prompt used with Copilot: "Generate 5 backlog items with acceptance criteria and code links based on repo findings."

## Risk & Rollback
- Risk: low — documentation-only commit; no source code changed
- Rollback: `git revert <this commit>`

## Track
- Level: Crawl
- Exercise: Ex12
