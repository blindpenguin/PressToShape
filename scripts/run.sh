#!/usr/bin/env bash
# Runs the desktop app.
# Usage: scripts/run.sh [extra dotnet run args]
set -euo pipefail

if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet not found. Install the .NET 10 SDK first." >&2
  exit 1
fi

cd "$(dirname "${BASH_SOURCE[0]}")/.."
exec dotnet run --project src/PressToShape.Desktop "$@"
