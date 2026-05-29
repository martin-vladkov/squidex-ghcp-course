## Summary
- Added `.github/PULL_REQUEST_TEMPLATE.md` — GitHub's built-in PR template; auto-populates the PR body field on GitHub with the improved format (includes `## Review focus`, guidance comments, and placeholder structure)
- Updated `.copilot-track/crawl/README.md` — PR Description Template section now includes `## Review focus` with three sub-bullets: Critical files, Check that, Not in scope
- No code or test changes; this is a process/template improvement
- Files touched: `.github/PULL_REQUEST_TEMPLATE.md`, `.copilot-track/crawl/README.md`

## Review focus
- **Critical files:** `.github/PULL_REQUEST_TEMPLATE.md` — this is what every future PR body will be pre-populated with; check the section names and guidance comments are clear
- **Check that:** `## Review focus` appears between `## Summary` and `## Evidence`; the three sub-bullets (`Critical files`, `Check that`, `Not in scope`) give reviewers concrete direction; HTML comments don't render in the submitted PR body
- **Not in scope:** retroactively updating Ex1–Ex10 PR bodies (they're already merged/open); adding `## Review focus` to the `pr-body-exN.md` files for past exercises

## Evidence
- Tests/logs/metrics:
  ```
  ./test-crawl.sh
  PASSED: Core.Model — LanguagesConfigTests   (22 passed)
  PASSED: Entities — AppDomainObjectTests     (33 passed)
  Results: 2 passed, 0 failed
  ```
- Prompt used with Copilot: "Draft a PR summary with review focus, risks, verification steps, and rollback. Propose commit message improvements."

## Risk & Rollback
- Risk: low — template files only; existing PRs and code unaffected; GitHub ignores `PULL_REQUEST_TEMPLATE.md` if you clear the body manually
- Rollback: `git revert <this commit>`

## Track
- Level: Crawl
- Exercise: Ex11
