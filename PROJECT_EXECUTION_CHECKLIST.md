# PROJECT EXECUTION CHECKLIST

## Major Phases
- [x] Phase 1: Foundation architecture and project scaffolding
- [x] Phase 2: Database schema and EF Core setup
- [x] Phase 3: MVVM base infrastructure
- [x] Phase 4: Main Fluent shell UI
- [ ] Phase 5: POS transaction flow completion
- [ ] Phase 6: Warranty and maintenance workflows completion
- [x] Phase 7: Reports and analytics
- [ ] Phase 8: Hardening, QA, installer, and release

## Modules
- [x] Core module structure
- [x] Infrastructure module structure
- [x] Application module structure
- [x] Presentation module structure
- [x] Shared module structure
- [ ] Dashboard module implementation
- [x] POS module implementation
- [x] Products module implementation
- [x] Customers module implementation
- [x] Warranty module implementation
- [x] Maintenance module implementation
- [x] Reports module implementation
- [x] Settings module implementation
- [ ] Users module implementation

## Database Tables
- [x] Users
- [x] Customers
- [x] Products
- [x] InventoryMovements
- [x] Invoices
- [x] InvoiceItems
- [x] Payments
- [x] Devices
- [x] Warranties
- [x] MaintenancePlans
- [x] MaintenanceSchedule
- [x] MaintenanceVisits
- [x] Settings
- [x] AuditLogs
- [x] Initial EF Core migration generated

## UI Screens
- [x] Main Fluent shell (Sidebar + Header + Content area)
- [x] POS screen (search + barcode input + cart + payment actions)
- [ ] Dashboard screen
- [x] Products management screen
- [x] Customers management screen
- [x] Warranty management screen
- [x] Maintenance schedule screen
- [x] Reports screen
- [x] Settings screen
- [ ] Users screen (owner only)

## Features
- [x] Layered folder architecture (`Core/Infrastructure/Application/Presentation/Shared`)
- [x] BaseViewModel with CommunityToolkit.Mvvm
- [x] Async product search service
- [x] DataGrid virtualization setup
- [x] Keyboard shortcuts scaffold (F2/F3/F4/Esc)
- [x] Main FluentWindow shell layout scaffold
- [x] EF Core migrations auto-apply at startup
- [x] Transactional invoice service scaffold
- [x] Profit calculation scaffold
- [x] Warranty creation policy scaffold
- [x] Maintenance schedule generator scaffold
- [x] POS cart and checkout async workflow scaffold
- [x] Barcode keyboard-input workflow scaffold
- [x] Products CRUD and stock adjustment workflow scaffold
- [x] Customers CRUD workflow scaffold
- [x] Warranty registration/cancel workflow scaffold
- [x] Maintenance schedule status/visit workflow scaffold
- [ ] Full barcode scanner workflow
- [x] Complete role-based permissions enforcement
- [ ] Warranty impact policy when maintenance skipped
- [x] Full reporting engine
- [ ] Startup performance validation under 3 seconds
- [ ] Search latency validation under 300ms

## Security Items
- [ ] Password hashing policy with salt strategy
- [ ] Login lockout / brute-force protection
- [x] Permission matrix and action guards
- [x] Global exception logging
- [ ] Sensitive data encryption at rest (if required by deployment policy)

## Backup
- [x] Manual backup command
- [x] Manual restore command
- [x] Backup integrity check

## Printing
- [x] A4 invoice template
- [x] Thermal 80mm invoice template
- [x] QR code generation with InvoiceId
- [x] Printer selection and fallback handling

## Localization
- [x] English resource dictionary scaffold
- [x] Arabic resource dictionary scaffold
- [x] Runtime language switcher
- [x] RTL/LTR auto-flow switch integration across all views
- [x] Arabic/English style direction switching point added (commercial requirement)

## Installer
- [ ] Installer packaging (MSIX/Setup)
- [ ] Auto-update strategy
- [ ] First-run prerequisites check
- [ ] Production deployment checklist
