## GHCP — Crawl Exercise 8: Security & secrets hygiene

### What changed
- **Remediation:** `git rm --cached dev/squidex-dev.key` — untracked committed RSA private key (`BEGIN PRIVATE KEY`); file kept locally for developer use, no longer in git index going forward
- **Updated:** `.gitignore` — added patterns for:
  - `dev/*.key`, `dev/*.p12`, `*.key`, `*.p12` — private key files
  - `.env`, `.env.*`, `*.env` (with `!*.env.example` exception) — environment secret files
  - `secrets.json`, `**/secrets.json`, `**/*secrets*.json` — secret JSON files
  - `appsettings.*.json` with `!appsettings.json` exception — broader coverage of config overrides
- **New:** `ai-track-docs/security-hygiene.md` — full audit document covering:
  - Remediated finding (private key) with risk classification
  - Accepted-risk findings (dev PFX, docker-compose test passwords) with rationale
  - Existing coverage confirmed (appsettings, launchSettings, PEM certs)
  - Hygiene rules for future contributors
  - Incident response steps if a real secret is ever committed
  - Scanning tools (trufflehog, grep baseline)

### Evidence
- `git check-ignore -v dev/squidex-dev.key` → matched by `*.key` rule
- `git status` shows `D  dev/squidex-dev.key` (staged deletion) and `M .gitignore`
- Backend tests unchanged: 22 LanguagesConfigTests still pass

### Risk & Rollback
- Risk: low — the private key is a self-signed localhost cert; not a production credential
- Rollback: `git revert <this commit>` then `git checkout HEAD~1 -- dev/squidex-dev.key` to restore tracking

### Track
- Level: Crawl
- Exercise: Ex8
