param(
    [switch]$Force
)

$ErrorActionPreference = "Stop"

$env:POS_SEED_DEMO_DATA = "1"
if ($Force) {
    $env:POS_SEED_DEMO_FORCE = "1"
}

dotnet run -- --seed-demo-data --seed-only
