# Security & Secrets Hygiene

> **Updated:** Crawl Exercise 8 (2026-05-29)

---

## Scope

This document covers secret hygiene for the Squidex fork used in the GHCP Crawl track. It audits what is (and isn't) committed, documents deliberate trade-offs, and records remediations made in Ex8.

---

## Findings (audit 2026-05-29)

### 🔴 REMEDIATED — Private key committed

| Finding | File | Risk | Action |
|---------|------|------|--------|
| Raw RSA private key committed | `dev/squidex-dev.key` | Medium — key material in git history; localhost cert, not production | Untracked via `git rm --cached`; `dev/*.key` added to `.gitignore` |

**Note on git history:** The key is still present in commits prior to Ex8. If this were a production key, full history rewrite (`git filter-repo`) would be required. Because it is a localhost self-signed development certificate, history rewrite is out of scope.

---

### 🟡 ACCEPTED RISK — Dev PFX committed with known password

| Finding | File | Risk | Disposition |
|---------|------|------|-------------|
| `dev/squidex-dev.pfx` contains private key | `dev/squidex-dev.pfx` | Low — `install-cert.ps1` reads this file from `./` path; password `"password"` hardcoded in both scripts | Accepted for dev convenience. **Never use these certs outside localhost.** |

The PFX password is trivially known (`"password"` in `dev/create-cert.ps1` and `dev/install-cert.ps1`). This is by design for a shared dev workflow. Run `create-cert.ps1` on your own machine to generate a unique cert if required.

---

### 🟡 ACCEPTED RISK — Test docker-compose credentials

| File | Credentials | Disposition |
|------|-------------|-------------|
| `tools/TestSuite/docker-compose-postgres.yml` | `POSTGRES_PASSWORD=secret` | Local ephemeral DB only — never exposed outside host |
| `tools/TestSuite/docker-compose-mysql.yml` | `MYSQL_PASSWORD=secret`, `MYSQL_ROOT_PASSWORD=secret` | Same |
| `tools/TestSuite/docker-compose-ferretdb.yml` | `POSTGRES_PASSWORD=password` | Same |

These are intentional localhost-only test credentials. Using env-var substitution would complicate local onboarding with no security benefit since the containers are ephemeral and port-bound to `127.0.0.1`.

---

### ✅ Already handled — App settings

| Pattern | Coverage |
|---------|----------|
| `appsettings.Development.json` | Ignored ✅ |
| `appsettings.Production.json` | Ignored ✅ |
| `appsettings.*.json` | Ignored by new `.gitignore` entry ✅ |
| `launchSettings.json` | Ignored ✅ |
| `frontend/app-config/localhost-key.pem` | Ignored ✅ |
| `frontend/app-config/localhost.pem` | Ignored ✅ |

---

## .gitignore improvements (Ex8)

Added the following patterns to `.gitignore`:

```gitignore
# Private keys & certificates
dev/*.key
dev/*.p12
*.key
*.p12

# Environment / secret files
.env
.env.*
*.env
!*.env.example
secrets.json
**/secrets.json
**/*secrets*.json
!**/*.example.json

# C# user secrets override
appsettings.*.json
!appsettings.json
```

---

## Rules going forward

1. **No raw private keys in git** — use `.gitignore` entries; generate per-developer if needed
2. **No production credentials anywhere in the repo** — use CI/CD secret stores (GitHub Actions secrets, Azure Key Vault, etc.)
3. **`.env.example` pattern** — provide `.env.example` with placeholder values; actual `.env` stays ignored
4. **PFX files** — only commit if: (a) self-signed localhost only, (b) password is committed alongside (so no false security), and (c) the use is explicitly documented here
5. **Before adding any credential** — check: could this file be accessed if the repo were public?

---

## If a real secret is ever committed

1. **Rotate immediately** — revoke/regenerate the credential before anything else
2. **Rewrite history**: `git filter-repo --path path/to/secret --invert-paths`
3. **Force-push** all branches; notify all collaborators to re-clone
4. **Audit access logs** of the exposed service

---

## Tools for ongoing scanning

```bash
# Scan for secrets with trufflehog (install: brew install trufflehog)
trufflehog git file://. --only-verified

# Quick regex scan (baseline — not exhaustive)
grep -rn "BEGIN PRIVATE KEY\|BEGIN RSA\|AKIA[0-9A-Z]\|password\s*=\s*['\"][^'\"]" \
  --include="*.json" --include="*.yml" --include="*.yaml" --include="*.env" \
  . | grep -v node_modules | grep -v ".git" | grep -v "obj/"
```
