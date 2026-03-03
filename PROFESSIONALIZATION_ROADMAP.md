# Professionalization Roadmap

## Phase 1 (Done)
- Centralized application logging via `Shared/Helpers/AppLogger.cs`.
- Safer startup/shutdown lifecycle with explicit fatal error handling.
- Added handling for unobserved task exceptions.
- Strengthened validation in warranty registration and maintenance visits.

## Phase 2 (Recommended next)
- Add automated test project:
  - Service-layer unit tests for warranty policy and maintenance scheduling.
  - Integration tests for `InvoiceService` auto warranty/schedule creation.
- Add CI pipeline (GitHub Actions):
  - Build
  - Test
  - Static analysis

## Phase 3 (UX/Product)
- Convert remaining hardcoded UI strings to localization resources.
- Add unified “Service Center” module (single tab for warranty + maintenance + reporting).
- Add device category flag (`WarrantyEligible`) to avoid creating warranty for non-device items.

## Phase 4 (Operational)
- Rotation/retention strategy for `app.log`.
- Backup verification job + restore drills.
- Release checklist automation for installer packaging.
