# PRODUCTION DEPLOYMENT CHECKLIST

## 1. Environment
- [ ] Target OS is Windows 10/11 with required updates.
- [ ] `.NET Desktop Runtime` compatible with the build is installed.
- [ ] Application runs with a dedicated non-admin user account.
- [ ] `POS_DEFAULT_OWNER_PASSWORD` environment variable is configured on first deployment.

## 2. Data and Security
- [ ] Application data directory is backed up (`%LOCALAPPDATA%/posSystem`).
- [ ] Backup restore test was executed on a separate machine.
- [ ] Default `owner` password was changed immediately after first login.
- [ ] `POS_BACKUP_ENCRYPTION_KEY` is configured when encrypted backups are required.
- [ ] Log file retention policy is defined for `app.log`.

## 3. Functional Validation
- [ ] POS sell flow validated end-to-end (scan, add cart, pay, print).
- [ ] Warranty registration and maintenance update flows validated.
- [ ] Reporting range and totals validated against sample data.
- [ ] User permissions verified for `Owner`, `Manager`, and `Cashier`.

## 4. Performance and Stability
- [ ] Startup warnings checked in logs (`PERF WARNING`).
- [ ] Search latency warnings checked in logs under expected load.
- [ ] 24-hour soak run completed without unhandled exceptions.

## 5. Printing and Devices
- [ ] Target printer profile selected and tested (A4 and 80mm).
- [ ] QR code on invoice is readable and links to expected invoice id.
- [ ] Barcode scanner wedge mode tested with real hardware.

## 6. Release Operations
- [ ] Release package signed and checksum archived.
- [ ] Rollback package for previous stable version prepared.
- [ ] Support runbook shared with operations team.
- [ ] Go-live date/time and owner are documented.
