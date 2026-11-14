#!/usr/bin/env bash
# Starts the spec-workflow dashboard.
# Usage: scripts/spec-dashboard.sh [port]   (default port: 5000)
set -euo pipefail

if ! command -v npx >/dev/null 2>&1; then
  echo "npx not found. Install Node.js first." >&2
  exit 1
fi

args=(--dashboard)
if [[ $# -ge 1 ]]; then
  args+=(--port "$1")
fi

exec npx -y @pimzino/spec-workflow-mcp@latest "${args[@]}"
