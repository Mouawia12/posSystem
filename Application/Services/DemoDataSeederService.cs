using Application.BusinessRules;
using Core.Entities;
using Core.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Helpers;

namespace Application.Services
{
    public sealed class DemoDataSeederService : IDemoDataSeederService
    {
        private readonly IDbContextFactory<PosDbContext> _dbContextFactory;
        private readonly IWarrantyPolicyService _warrantyPolicyService;
        private readonly IMaintenanceScheduleGenerator _maintenanceScheduleGenerator;

        public DemoDataSeederService(
            IDbContextFactory<PosDbContext> dbContextFactory,
            IWarrantyPolicyService warrantyPolicyService,
            IMaintenanceScheduleGenerator maintenanceScheduleGenerator)
        {
            _dbContextFactory = dbContextFactory;
            _warrantyPolicyService = warrantyPolicyService;
            _maintenanceScheduleGenerator = maintenanceScheduleGenerator;
        }

        public async Task EnsureSeededAsync(bool force = false, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var hasData = await db.Products.AnyAsync(cancellationToken)
                || await db.Customers.AnyAsync(cancellationToken)
                || await db.Invoices.AnyAsync(cancellationToken);

            if (hasData && !force)
            {
                return;
            }

            if (force)
            {
                await ClearExistingDemoDataAsync(db, cancellationToken);
            }

            var settings = await db.Settings.FirstOrDefaultAsync(cancellationToken);
            if (settings is null)
            {
                settings = new Setting
                {
                    CompanyName = "My Retail Store",
                    InvoicePrefix = "INV",
                    NextInvoiceNumber = 1001,
                    Currency = "USD"
                };
                await db.Settings.AddAsync(settings, cancellationToken);
            }

            settings.Currency = NormalizeCurrency(settings.Currency);
            settings.NextInvoiceNumber = Math.Max(settings.NextInvoiceNumber, 1001);

            await EnsureDemoUsersAsync(db, cancellationToken);

            var customers = await SeedCustomersAsync(db, cancellationToken);
            var products = await SeedProductsAsync(db, cancellationToken);
            var ownerUser = await db.Users.FirstAsync(x => x.Username == "owner", cancellationToken);

            await db.SaveChangesAsync(cancellationToken);

            await SeedInvoiceAsync(db, settings, ownerUser, customers[0], products, cancellationToken);
            await SeedWarrantyAndMaintenanceAsync(db, customers[0], cancellationToken);

            await db.SaveChangesAsync(cancellationToken);
        }

        private static async Task EnsureDemoUsersAsync(PosDbContext db, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var users = new[]
            {
                new { Username = "owner", Role = UserRole.Owner, Password = "owner12345" },
                new { Username = "manager.demo", Role = UserRole.Manager, Password = "Manager123!" },
                new { Username = "cashier.demo", Role = UserRole.Cashier, Password = "Cashier123!" }
            };

            foreach (var seed in users)
            {
                var existing = await db.Users.FirstOrDefaultAsync(x => x.Username == seed.Username, cancellationToken);
                if (existing is null)
                {
                    await db.Users.AddAsync(new User
                    {
                        Username = seed.Username,
                        Role = seed.Role,
                        IsActive = true,
                        PasswordHash = PasswordHashing.HashPassword(seed.Password),
                        CreatedAt = now
                    }, cancellationToken);
                    continue;
                }

                existing.Role = seed.Role;
                existing.IsActive = true;
                if (string.IsNullOrWhiteSpace(existing.PasswordHash))
                {
                    existing.PasswordHash = PasswordHashing.HashPassword(seed.Password);
                }
            }
        }

        private static async Task<List<Customer>> SeedCustomersAsync(PosDbContext db, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var seeds = new[]
            {
                new Customer { FullName = "Ahmed Alqahtani", Phone = "0551112233", Location = "Riyadh", Notes = "VIP" },
                new Customer { FullName = "Sara Alharbi", Phone = "0554445566", Location = "Jeddah", Notes = "Prefers WhatsApp" },
                new Customer { FullName = "Omar Alshehri", Phone = "0557778899", Location = "Dammam", Notes = "Corporate account" }
            };

            var result = new List<Customer>();
            foreach (var seed in seeds)
            {
                var existing = await db.Customers.FirstOrDefaultAsync(x => x.Phone == seed.Phone, cancellationToken);
                if (existing is null)
                {
                    seed.CreatedAt = now;
                    await db.Customers.AddAsync(seed, cancellationToken);
                    result.Add(seed);
                }
                else
                {
                    result.Add(existing);
                }
            }

            return result;
        }

        private static async Task<List<Product>> SeedProductsAsync(PosDbContext db, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var seeds = new[]
            {
                new Product { SKU = "IPH-15-128-BLK", Name = "iPhone 15 128GB", CostPrice = 2650m, SalePrice = 2999m, QuantityOnHand = 20m, IsActive = true },
                new Product { SKU = "SAM-S24-256", Name = "Samsung S24 256GB", CostPrice = 2400m, SalePrice = 2799m, QuantityOnHand = 18m, IsActive = true },
                new Product { SKU = "AIRP-2", Name = "AirPods 2", CostPrice = 420m, SalePrice = 549m, QuantityOnHand = 40m, IsActive = true },
                new Product { SKU = "CABL-TYPEC-2M", Name = "Type-C Cable 2m", CostPrice = 18m, SalePrice = 39m, QuantityOnHand = 120m, IsActive = true },
                new Product { SKU = "CHGR-20W", Name = "Fast Charger 20W", CostPrice = 35m, SalePrice = 79m, QuantityOnHand = 75m, IsActive = true }
            };

            var result = new List<Product>();
            foreach (var seed in seeds)
            {
                var existing = await db.Products.FirstOrDefaultAsync(x => x.SKU == seed.SKU, cancellationToken);
                if (existing is null)
                {
                    seed.CreatedAt = now;
                    await db.Products.AddAsync(seed, cancellationToken);
                    result.Add(seed);
                }
                else
                {
                    result.Add(existing);
                }
            }

            return result;
        }

        private static async Task SeedInvoiceAsync(
            PosDbContext db,
            Setting settings,
            User ownerUser,
            Customer customer,
            IReadOnlyList<Product> products,
            CancellationToken cancellationToken)
        {
            if (await db.Invoices.AnyAsync(cancellationToken))
            {
                return;
            }

            var now = DateTime.UtcNow;
            var selectedItems = products.Take(3).ToList();
            var invoiceNumber = $"{settings.InvoicePrefix}-{settings.NextInvoiceNumber:D7}";

            var subtotal = selectedItems.Sum(x => x.SalePrice);
            const decimal discount = 50m;
            const decimal tax = 20m;
            var total = subtotal - discount + tax;
            var profit = selectedItems.Sum(x => x.SalePrice - x.CostPrice);

            var invoice = new Invoice
            {
                InvoiceNumber = invoiceNumber,
                CustomerId = customer.Id,
                UserId = ownerUser.Id,
                Subtotal = subtotal,
                Discount = discount,
                Tax = tax,
                Total = total,
                Profit = profit,
                Status = InvoiceStatus.Paid,
                CreatedAt = now
            };

            await db.Invoices.AddAsync(invoice, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            foreach (var product in selectedItems)
            {
                product.QuantityOnHand -= 1m;

                await db.InvoiceItems.AddAsync(new InvoiceItem
                {
                    InvoiceId = invoice.Id,
                    ProductId = product.Id,
                    Quantity = 1m,
                    UnitPrice = product.SalePrice,
                    UnitCost = product.CostPrice,
                    LineTotal = product.SalePrice,
                    LineProfit = product.SalePrice - product.CostPrice,
                    CreatedAt = now
                }, cancellationToken);

                await db.InventoryMovements.AddAsync(new InventoryMovement
                {
                    ProductId = product.Id,
                    Type = InventoryMovementType.Out,
                    Quantity = 1m,
                    Reason = $"Demo invoice {invoice.InvoiceNumber}",
                    CreatedAt = now
                }, cancellationToken);
            }

            await db.Payments.AddAsync(new Payment
            {
                InvoiceId = invoice.Id,
                Amount = total,
                Method = PaymentMethod.Cash,
                PaidAt = now,
                CreatedAt = now
            }, cancellationToken);

            settings.NextInvoiceNumber += 1;
        }

        private async Task SeedWarrantyAndMaintenanceAsync(PosDbContext db, Customer customer, CancellationToken cancellationToken)
        {
            if (await db.Warranties.AnyAsync(cancellationToken) || await db.MaintenancePlans.AnyAsync(cancellationToken))
            {
                return;
            }

            var invoice = await db.Invoices.OrderByDescending(x => x.Id).FirstOrDefaultAsync(cancellationToken);
            if (invoice is null)
            {
                return;
            }

            var soldAt = DateTime.UtcNow.Date.AddDays(-20);
            var device = new Device
            {
                DeviceType = "Smartphone",
                Model = "iPhone 15",
                SerialNumber = "D-IPH15-0001",
                CustomerId = customer.Id,
                SoldInvoiceId = invoice.Id,
                SoldAt = soldAt,
                CreatedAt = DateTime.UtcNow
            };

            await db.Devices.AddAsync(device, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            var warranty = _warrantyPolicyService.BuildDefaultWarranty(device, customer.Id, invoice.Id, soldAt);
            await db.Warranties.AddAsync(warranty, cancellationToken);

            var plan = new MaintenancePlan
            {
                DeviceId = device.Id,
                CustomerId = customer.Id,
                StartDate = soldAt,
                EndDate = soldAt.AddYears(2),
                CreatedAt = DateTime.UtcNow
            };

            await db.MaintenancePlans.AddAsync(plan, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            var schedules = _maintenanceScheduleGenerator.Generate(plan.StartDate, plan.EndDate)
                .Take(3)
                .Select(x => new MaintenanceSchedule
                {
                    PlanId = plan.Id,
                    DueDate = x.DueDate,
                    PeriodMonths = x.PeriodMonths,
                    Status = MaintenanceStatus.Due,
                    CreatedAt = DateTime.UtcNow
                });

            await db.MaintenanceSchedules.AddRangeAsync(schedules, cancellationToken);
        }

        private static async Task ClearExistingDemoDataAsync(PosDbContext db, CancellationToken cancellationToken)
        {
            db.Payments.RemoveRange(db.Payments);
            db.InvoiceItems.RemoveRange(db.InvoiceItems);
            db.InventoryMovements.RemoveRange(db.InventoryMovements);
            db.MaintenanceVisits.RemoveRange(db.MaintenanceVisits);
            db.MaintenanceSchedules.RemoveRange(db.MaintenanceSchedules);
            db.MaintenancePlans.RemoveRange(db.MaintenancePlans);
            db.Warranties.RemoveRange(db.Warranties);
            db.Devices.RemoveRange(db.Devices);
            db.Invoices.RemoveRange(db.Invoices);
            db.Products.RemoveRange(db.Products);
            db.Customers.RemoveRange(db.Customers);

            await db.SaveChangesAsync(cancellationToken);
        }

        private static string NormalizeCurrency(string? currency)
        {
            return string.Equals(currency, "SAR", StringComparison.OrdinalIgnoreCase) ? "SAR" : "USD";
        }
    }
}
