<!--
  PR Description — GHCP Course (Crawl + Walk tracks)

  Title formats:
    Crawl: "GHCP -- Crawl: Ex<N> <short name>"
    Walk:  "GHCP -- Walk: Ex<N> <short name>"

  Rules:
    • Fill in EVERY section before submitting — delete placeholder lines.
    • Evidence must be actual command output (no fabricated results).
    • Review Focus requires 3-5 specific bullets: file + what to check + why.
    • Verification Steps must be copy-pasteable commands a reviewer can run.
    • Rollback must name the exact git command or config toggle to undo the change.
-->

## Summary
<!-- What changed and why. Include the plan you followed and every file touched. -->
- **What changed and why:**
- **Plan:**

## Evidence
<!--
  Paste ACTUAL command output — test results, CI run links, metric readings.
  No fabricated or placeholder results.
-->
- Tests:
  ```
  <dotnet test output or CI run URL>
  ```
- Build / lint:
  ```
  <0 warnings / 0 errors or CI link>
  ```

## Risk & Rollback
<!--
  Risk level: low / medium / high — one sentence explaining why.
  Rollback: exact command (git revert <SHA>, delete file X, flip toggle Y).
-->
- **Risk:**
- **Rollback:** `git revert <SHA>`  OR  `<describe config toggle / file delete>`

## Review Focus
<!--
  Write 3–5 bullets. Each bullet must name:
    1. The specific file or section to inspect
    2. What the reviewer should verify and why it matters
  Focus on the riskiest or most subtle parts of the change.
-->
- **`<file>` line <N>** —
- **`<file>` line <N>** —
- **`<file>` line <N>** —
- **Not in scope / intentionally excluded:**

## Verification steps
<!--
  Copy-pasteable commands a reviewer can run locally to confirm the change is correct.
  Include expected output so the reviewer knows what "passing" looks like.
-->
```bash
# Step 1 — <description>
<command>
# Expected: <what to see>

# Step 2 — <description>
<command>
# Expected: <what to see>
```

## Track
- Level: <!-- Crawl | Walk -->
- Exercise: Ex<!-- N -->
