#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
output_dir="$(pwd)/${1:-artifacts}"
mkdir -p "$output_dir"
run_dir="$(mktemp -d)"
api_pid=""
cleanup() {
  if [[ -n "$api_pid" ]]; then
    kill "$api_pid" 2>/dev/null || true
    wait "$api_pid" 2>/dev/null || true
  fi
  rm -rf "$run_dir"
}
trap cleanup EXIT
dotnet build Lab02.sln --configuration Release --nologo
dotnet run --project tests/Lab02.Checks -c Release --no-build \
  | tee "$output_dir/checks.txt"
dotnet run --project src/Part1.Interfaces -c Release --no-build \
  -- "$run_dir/work.log" | tee "$output_dir/part1.txt"
cp "$run_dir/work.log" "$output_dir/file-logger.txt"
dotnet run --project src/Part2.RepositoryDemo -c Release --no-build \
  -- "$run_dir/library.db" | tee "$output_dir/part2.txt"
cp "$run_dir/library.db" "$output_dir/library.db"
api_port="${LAB02_PORT:-5080}"
api_url="http://127.0.0.1:$api_port"
DatabasePath="$run_dir/users.db" \
  dotnet run --project src/Part3.UsersApi -c Release --no-build \
    --no-launch-profile --urls "$api_url" > "$output_dir/api-server.txt" 2>&1 &
api_pid=$!
for ((attempt = 0; attempt < 100; attempt++)); do
  if curl -fsS "$api_url/" >/dev/null 2>&1; then break; fi
  if ! kill -0 "$api_pid" 2>/dev/null; then
    cat "$output_dir/api-server.txt"
    exit 1
  fi
  sleep 0.2
done
python3 scripts/api_smoke.py --base-url "$api_url" \
  --output "$output_dir/api-smoke.json" | tee "$output_dir/api-checks.txt"
echo "All verification stages passed"
