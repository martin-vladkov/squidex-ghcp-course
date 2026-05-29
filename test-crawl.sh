#!/usr/bin/env bash
# test-crawl.sh — run all GHCP Crawl exercise unit tests locally
# Usage: ./test-crawl.sh
# Exit code: 0 = all passed, non-zero = at least one suite failed

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACKEND="$REPO_ROOT/backend"

GREEN='\033[0;32m'
RED='\033[0;31m'
BOLD='\033[1m'
RESET='\033[0m'

pass=0
fail=0

run_suite() {
  local label="$1"
  local project="$2"
  local filter="$3"

  echo ""
  echo -e "${BOLD}=== $label ===${RESET}"
  if dotnet test "$project" \
      --filter "$filter" \
      --logger "console;verbosity=normal" \
      --nologo; then
    echo -e "${GREEN}PASSED: $label${RESET}"
    pass=$((pass + 1))
  else
    echo -e "${RED}FAILED: $label${RESET}"
    fail=$((fail + 1))
  fi
}

run_suite \
  "Core.Model — LanguagesConfigTests" \
  "$BACKEND/tests/Squidex.Domain.Apps.Core.Tests/Squidex.Domain.Apps.Core.Tests.csproj" \
  "FullyQualifiedName~LanguagesConfigTests"

run_suite \
  "Entities — AppDomainObjectTests" \
  "$BACKEND/tests/Squidex.Domain.Apps.Entities.Tests/Squidex.Domain.Apps.Entities.Tests.csproj" \
  "FullyQualifiedName~AppDomainObjectTests"

echo ""
echo -e "${BOLD}Results: ${GREEN}$pass passed${RESET}${BOLD}, ${RED}$fail failed${RESET}"

if [[ $fail -gt 0 ]]; then
  exit 1
fi
