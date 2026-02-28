#!/usr/bin/env bash
set -euo pipefail

FORCE="${1:-}"
export POS_SEED_DEMO_DATA=1

if [[ "$FORCE" == "--force" ]]; then
  export POS_SEED_DEMO_FORCE=1
fi

dotnet run -- --seed-demo-data --seed-only
