# AUTO UPDATE STRATEGY

## Goal
Provide predictable, low-risk updates for store deployments with rollback support.

## Channel Model
- `stable`: production stores.
- `pilot`: limited stores for early validation.
- `internal`: QA and support environments.

## Update Metadata
Publish a signed JSON manifest per channel with:
- `version`
- `releasedAtUtc`
- `minimumSupportedVersion`
- `downloadUrl`
- `sha256`
- `notes`

## Client Behavior
1. Check manifest at startup and once every 24 hours.
2. If `version` is newer than installed version, show update prompt.
3. Download package to temporary folder.
4. Verify `sha256` before installation.
5. Apply update on next app restart.
6. Keep last known good package for rollback.

## Safety Rules
- Block downgrade unless forced by support override.
- Block update if database migration fails pre-check.
- Require backup creation before update starts.
- Keep update logs with timestamp and result code.

## Operational Rollout
1. Publish to `internal`.
2. Run smoke tests and migration checks.
3. Promote same artifact to `pilot`.
4. After 48-72 hours without critical incidents, promote to `stable`.

## Rollback
- Trigger rollback if error rate rises above agreed threshold.
- Reinstall last known good package.
- Restore latest validated backup if schema rollback is required.
