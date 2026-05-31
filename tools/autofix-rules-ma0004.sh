#!/usr/bin/env bash
# =============================================================================
#  autofix-rules-ma0004.sh
#  Autofix script for MA0004 (UseConfigureAwait) in Rules/*.cs
#
#  Purpose:
#    Automatically adds .ConfigureAwait(false) to all bare `await <Task>` calls
#    in the specified target files using `dotnet format`.  Meziantou.Analyzer
#    includes a code fixer for MA0004 that dotnet format can apply.
#
#  Usage:
#    ./tools/autofix-rules-ma0004.sh [--dry-run] [--target <path>]
#
#    --dry-run     Report files that would be changed but do not modify them.
#    --target      Path to the .csproj file (default: Squidex.Domain.Apps.Entities)
#
#  Prerequisites:
#    dotnet SDK >= 8.0
#    Run from the repository root or the backend/ directory.
#
#  Background:
#    MA0004 was enabled as a warning for backend/src/Squidex.Domain.Apps.Entities/Rules/
#    in Ex14.  This script automates the fix for files that still have violations
#    after the manual hot-path fixes committed in that exercise.
#    See docs/backlog.md for the clean-up backlog items (BACK-6 through BACK-10).
# =============================================================================
set -euo pipefail

# ── defaults ──────────────────────────────────────────────────────────────────
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
BACKEND_ROOT="${REPO_ROOT}/backend"

CSPROJ="${BACKEND_ROOT}/src/Squidex.Domain.Apps.Entities/Squidex.Domain.Apps.Entities.csproj"
TARGET_FOLDER="${BACKEND_ROOT}/src/Squidex.Domain.Apps.Entities/Rules"
DRY_RUN=false

# ── argument parsing ──────────────────────────────────────────────────────────
while [[ $# -gt 0 ]]; do
  case "$1" in
    --dry-run)   DRY_RUN=true; shift ;;
    --target)    CSPROJ="$2"; shift 2 ;;
    *)           echo "Unknown argument: $1" >&2; exit 1 ;;
  esac
done

# ── validation ────────────────────────────────────────────────────────────────
if [[ ! -f "${CSPROJ}" ]]; then
  echo "ERROR: Project file not found: ${CSPROJ}" >&2
  exit 1
fi

# ── step 1: baseline count ────────────────────────────────────────────────────
echo ""
echo "=== MA0004 autofix — Rules/ folder ==="
echo "Project: ${CSPROJ}"
echo "Target:  ${TARGET_FOLDER}"
echo ""

BEFORE_COUNT=$(
  dotnet build "${CSPROJ}" --no-restore --no-incremental 2>&1 \
    | grep "warning MA0004" \
    | grep -F "${TARGET_FOLDER}" \
    | wc -l
)
echo "Baseline MA0004 warnings in ${TARGET_FOLDER}: ${BEFORE_COUNT}"

if [[ "${BEFORE_COUNT}" -eq 0 ]]; then
  echo "No MA0004 warnings found — nothing to do."
  exit 0
fi

# ── step 2: apply fixes ───────────────────────────────────────────────────────
FORMAT_ARGS=(
  "${CSPROJ}"
  --diagnostics MA0004
  --include "Rules/**/*.cs"
)

if [[ "${DRY_RUN}" == "true" ]]; then
  echo ""
  echo "[DRY RUN] Would run:"
  echo "  dotnet format ${FORMAT_ARGS[*]} --verify-no-changes"
  dotnet format "${FORMAT_ARGS[@]}" --verify-no-changes 2>&1 || {
    echo "[DRY RUN] dotnet format would make changes (exit $?)."
  }
else
  echo ""
  echo "Applying fixes with: dotnet format --diagnostics MA0004 ..."
  dotnet format "${FORMAT_ARGS[@]}" 2>&1
fi

# ── step 3: after count ───────────────────────────────────────────────────────
if [[ "${DRY_RUN}" == "false" ]]; then
  AFTER_COUNT=$(
    dotnet build "${CSPROJ}" --no-restore --no-incremental 2>&1 \
      | grep "warning MA0004" \
      | grep -F "${TARGET_FOLDER}" \
      | wc -l
  )
  echo ""
  echo "=== Results ==="
  echo "Before: ${BEFORE_COUNT} MA0004 warnings"
  echo "After:  ${AFTER_COUNT}  MA0004 warnings"
  FIXED=$(( BEFORE_COUNT - AFTER_COUNT ))
  echo "Fixed:  ${FIXED}"
fi

echo ""
echo "Done."
