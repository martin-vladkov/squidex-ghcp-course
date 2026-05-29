# Backlog — GHCP Crawl Ex12

> Generated from findings across Crawl exercises 1–11.  
> No tracker access — items are captured here until imported into Azure Boards / GitHub Issues.

---

## Item 1 — Extend structured logging to all `AppDomainObject` command handlers

**Source:** Ex9 — only `AddLanguage`, `RemoveLanguage`, `UpdateLanguage` were instrumented.  
**Code link:** [`AppDomainObject.cs`](../backend/src/Squidex.Domain.Apps.Entities/Apps/DomainObject/AppDomainObject.cs)  
**Gap:** 3 of 26 `case` handlers emit structured logs; the other 23 (roles, workflows, contributors, clients, settings, image, plan, …) are silent.

**Acceptance criteria:**
- [ ] Every private mutation method in `AppDomainObject` emits `LogInformation` with at least `op`, `status`, `elapsed_ms`
- [ ] Domain-specific fields added where meaningful (e.g. `role`, `workflow_id`, `contributor_id`)
- [ ] Existing `AddLanguage_should_log_structured_operation` test kept; parallel tests added for at least 2 other handlers
- [ ] No `LogDebug` / `LogTrace` duplication for the same call site

**Effort:** M (26 handlers; ~2 lines each + 2–3 new tests)  
**Priority:** Medium

---

## Item 2 — Remove `dev/squidex-dev.pfx` from git history (accepted risk, plan removal)

**Source:** Ex8 — `.pfx` accepted as "needed by `install-cert.ps1`" but it bundles a private key.  
**Code link:** [`dev/squidex-dev.pfx`](../dev/squidex-dev.pfx), [`dev/install-cert.ps1`](../dev/install-cert.ps1)  
**Gap:** Anyone who clones the repo gets a private key bundle (even though it is a self-signed localhost cert with a known password).

**Acceptance criteria:**
- [ ] `dev/squidex-dev.pfx` and `dev/squidex-dev.crt` removed from git tracking (`git rm --cached`) and added to `.gitignore`
- [ ] `dev/install-cert.ps1` updated to read from a path the developer provides, or `README-dev-certs.md` added with `create-cert.ps1` instructions
- [ ] Old commits scrubbed with `git filter-repo` OR a note added to `security-hygiene.md` explaining the residual history risk
- [ ] `dev/squidex-dev.cer` (public cert only) may remain tracked — document the decision

**Effort:** S (file removal + script update + note)  
**Priority:** Medium

---

## Item 3 — Lock Node.js version with `.nvmrc` (complement `engines` field)

**Source:** Ex7 — `frontend/package.json` now has `"engines": {"node": ">=22.0.0"}` but no `.nvmrc` for `nvm use` ergonomics.  
**Code link:** [`frontend/package.json` L3–5](../frontend/package.json)  
**Gap:** Developers using `nvm` have no `.nvmrc` to `nvm use` automatically; CI uses `setup-node` with `10.0.x` .NET version but Node is `22` locally — mismatch potential.

**Acceptance criteria:**
- [ ] `.nvmrc` created at repo root with content `22` (matches `engines.node >=22.0.0` and local `22.22.0`)
- [ ] `.github/workflows/ghcp-crawl.yml` updated to use `node-version-file: .nvmrc` instead of a hardcoded version string
- [ ] `ai-track-docs/dependencies.md` updated to reference `.nvmrc` as the pin location
- [ ] `echo "22" > .nvmrc && nvm use` tested locally

**Effort:** XS (one file + two-line workflow change)  
**Priority:** Low

---

## Item 4 — Add exact-pin lockdown for high-churn frontend dependencies

**Source:** Ex7 — 38 frontend packages use `^` range pins; production deps like `@floating-ui/dom`, `graphql-ws`, and `ace-builds` could silently update.  
**Code link:** [`frontend/package.json`](../frontend/package.json)  
**Gap:** `@angular/*` is exact-pinned, but 38 other packages can auto-upgrade on `npm install`, including packages that render UI (`ace-builds`, `ng2-charts`, `ngx-scrollbar`).

**Acceptance criteria:**
- [ ] All production `dependencies` (non-devDependencies) converted from `^x.y.z` to `x.y.z` exact pins
- [ ] `devDependencies` ranges left as-is or narrowed to `~` (minor-only); `vitest` and `eslint` may keep `^` — document why in `dependencies.md`
- [ ] `npm ci` still passes after pin changes (lockfile must be consistent)
- [ ] `ai-track-docs/dependencies.md` "Recommendation" note updated to reflect the change

**Effort:** S (`npm shrinkwrap` / sed + `npm ci` validation)  
**Priority:** Low

---

## Item 5 — Add CI status badge to `README.md`

**Source:** Ex10 — `ghcp-crawl.yml` workflow exists but is invisible on the repo landing page.  
**Code link:** [`.github/workflows/ghcp-crawl.yml`](../.github/workflows/ghcp-crawl.yml), [`README.md`](../README.md)  
**Gap:** New contributors see no immediate signal about whether the exercise-branch tests pass.

**Acceptance criteria:**
- [ ] GitHub Actions badge for `ghcp-crawl.yml` added to `README.md` (or a new `TRACK.md` if upstream `README.md` should stay clean)
- [ ] Badge links to the Actions run list filtered to `ghcp-crawl.yml`
- [ ] Badge renders correctly in GitHub's Markdown preview (test with a push to an `exercise-*` branch)
- [ ] Format: `[![GHCP Crawl Tests](https://github.com/martin-vladkov/squidex-ghcp-course/actions/workflows/ghcp-crawl.yml/badge.svg?branch=exercise-12)](…)`

**Effort:** XS (one-line markdown change)  
**Priority:** Low
