#!/usr/bin/env bash
set -euo pipefail

PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

declare -a CANDIDATES=(
  "${GODOT_BIN:-}"
  "/home/patolizo/Downloads/Godot_v4.7-dev3_mono_linux_x86_64/Godot_v4.7-dev3_mono_linux.x86_64"
  "$(command -v godot4 2>/dev/null || true)"
  "$(command -v godot 2>/dev/null || true)"
)

for candidate in "${CANDIDATES[@]}"; do
  if [[ -n "$candidate" && -x "$candidate" ]]; then
    exec "$candidate" --path "$PROJECT_DIR" "$@"
  fi
done

echo "Godot binary not found." >&2
echo "Set GODOT_BIN or install Godot in a standard path." >&2
exit 1
