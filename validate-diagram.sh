#!/usr/bin/env bash
# validate-diagram.sh — Validate Mermaid diagram in docs/architecture.md
# Usage: ./validate-diagram.sh
# Returns 0 if the diagram block exists and starts with a valid Mermaid directive.
# CI uses mmdc for full render validation; this script is a fast local pre-check.

set -euo pipefail

ARCH_DOC="docs/architecture.md"

if [[ ! -f "$ARCH_DOC" ]]; then
    echo "ERROR: $ARCH_DOC not found" >&2
    exit 1
fi

# Extract the first mermaid fenced block
DIAGRAM=$(awk '/^```mermaid/{flag=1; next} /^```/{if(flag) exit} flag' "$ARCH_DOC")

if [[ -z "$DIAGRAM" ]]; then
    echo "ERROR: No \`\`\`mermaid block found in $ARCH_DOC" >&2
    exit 1
fi

LINE_COUNT=$(echo "$DIAGRAM" | wc -l | tr -d ' ')

# Check the block opens with a recognised Mermaid diagram type
FIRST_WORD=$(echo "$DIAGRAM" | awk 'NF{print $1; exit}')
VALID_TYPES="graph|flowchart|sequenceDiagram|classDiagram|erDiagram|gantt|pie|gitGraph|stateDiagram"

if ! echo "$FIRST_WORD" | grep -qE "^($VALID_TYPES)"; then
    echo "ERROR: Diagram block starts with '$FIRST_WORD', which is not a recognised Mermaid type." >&2
    echo "       Expected one of: $VALID_TYPES" >&2
    exit 1
fi

echo "OK: Mermaid block found in $ARCH_DOC"
echo "    Type   : $FIRST_WORD"
echo "    Lines  : $LINE_COUNT"
echo ""
echo "Diagram preview (first 5 lines):"
echo "$DIAGRAM" | head -5
