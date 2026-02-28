# posSystem

## Demo Data

The app seeds demo data automatically on first run if the database is empty.

Manual seeding options:

- PowerShell (Windows/macOS with pwsh):
  - `./Scripts/seed-demo-data.ps1`
  - Force reseed: `./Scripts/seed-demo-data.ps1 -Force`
- Bash:
  - `./Scripts/seed-demo-data.sh`
  - Force reseed: `./Scripts/seed-demo-data.sh --force`

You can also run:

- `dotnet run -- --seed-demo-data --seed-only`
- Force reseed:
  - set `POS_SEED_DEMO_FORCE=1`
  - run `dotnet run -- --seed-demo-data --seed-only`

Default demo users:

- `owner / owner12345`
- `manager.demo / Manager123!`
- `cashier.demo / Cashier123!`

## Currency

From Settings, currency is now selectable between:

- `USD` (`$`)
- `SAR` (`ر.س`)
