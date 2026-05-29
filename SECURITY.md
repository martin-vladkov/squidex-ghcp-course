# Security Policy

## Supported versions

| Version | Supported |
|---------|-----------|
| latest `main` | ✅ Yes |
| older releases | ❌ No — please upgrade |

## Reporting a vulnerability

**Do not open a public GitHub issue for security vulnerabilities.**

Send an email to **security@squidex.io** with:

- A description of the vulnerability and its potential impact
- Steps to reproduce (proof-of-concept code or a clear description)
- Any suggested remediation, if known

You will receive an acknowledgement within **48 hours** and a triage update
within **7 business days**.

## Secret scanning

This repository uses [gitleaks](https://gitleaks.io/) to prevent accidental
credential commits.  Scanning runs automatically on every push and pull request
targeting a `walk-*` branch via the workflow at
`.github/workflows/ghcp-walk-security.yml`.

### Adding an allowlist entry

If gitleaks flags a value that is **not** a real credential (example config,
test fixture, local dev cert, etc.):

1. Confirm the value has **no production impact** — rotate it if there is any
   doubt.
2. Open `.gitleaks.toml` and add an entry to the `[allowlist]` section.
3. Include a **justification comment** explaining why the value is safe.
4. Run the scan locally to confirm it passes:

   ```bash
   gitleaks detect --source . --no-git
   ```

5. Commit `.gitleaks.toml` together with your changes.

### Running the scan locally

```bash
# Install (macOS)
brew install gitleaks

# Scan all files (no git history)
gitleaks detect --source . --no-git

# Scan git history
gitleaks detect --source .

# Produce a JSON report
gitleaks detect --source . --no-git \
  --report-format json --report-path gitleaks-report.json
```

### Known-safe values in this repository

All 28 findings from the initial baseline scan are documented in
`.gitleaks.toml`.  The categories are:

| Category | Files | Reason safe |
|----------|-------|-------------|
| Local dev TLS certificates | `dev/squidex-dev.{key,pfx,crt}` | Generated locally by `dev/create-cert.ps1`; not used in production |
| Localhost example tokens | `wwwroot/scripts/embed-sample.html`, `sdk/src/template.html` | Base64 JSON blobs encoding `https://localhost:5001` — documentation only |
| Google Maps placeholder | `backend/src/Squidex/appsettings.json` | Adjacent comment reads "CREATE YOUR OWN PLEASE" |
| Integration test secrets | `tools/TestSuite/**` | Used against a local self-hosted instance; env-var override at runtime |
| appsettings.json examples | `backend/src/Squidex/appsettings.json` | Placeholder documentation values; real secrets injected via env vars |
| Test fixture UUID | `Squidex.Data.Tests/…/UsageRepositoryTests.cs` | Fixed GUID used as a test seed value |
