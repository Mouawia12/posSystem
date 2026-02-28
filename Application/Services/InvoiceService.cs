using Application.DTOs;
using Application.BusinessRules;
using Core.Entities;
using Core.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public sealed class InvoiceService : IInvoiceService
    {
        private readonly IDbContextFactory<PosDbContext> _dbContextFactory;
        private readonly IMaintenanceScheduleGenerator _maintenanceScheduleGenerator;
        private readonly IWarrantyPolicyService _warrantyPolicyService;

        public InvoiceService(
            IDbContextFactory<PosDbContext> dbContextFactory,
            IMaintenanceScheduleGenerator maintenanceScheduleGenerator,
            IWarrantyPolicyService warrantyPolicyService)
        {
            _dbContextFactory = dbContextFactory;
            _maintenanceScheduleGenerator = maintenanceScheduleGenerator;
            _warrantyPolicyService = warrantyPolicyService;
        }

        public async Task<long> CreateInvoiceAsync(CreateInvoiceRequestDto request, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

            var settings = await db.Settings.FirstOrDefaultAsync(cancellationToken);
            if (settings is null)
            {
                settings = new Setting
                {
                    CompanyName = "Store",
                    InvoicePrefix = "INV",
                    NextInvoiceNumber = 1,
                    Currency = "USD"
                };
                await db.Settings.AddAsync(settings, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }

            var invoiceNumber = $"{settings.InvoicePrefix}-{settings.NextInvoiceNumber:D7}";

            var productIds = request.Items.Select(x => x.ProductId).ToArray();
            var productMap = await db.Products
                .Where(x => productIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            var subtotal = request.Items.Sum(x => x.UnitPrice * x.Quantity);
            var total = subtotal - request.Discount + request.Tax;
            var profit = request.Items.Sum(x =>
            {
                var product = productMap[x.ProductId];
                return (x.UnitPrice - product.CostPrice) * x.Quantity;
            });

            var invoice = new Invoice
            {
                CustomerId = request.CustomerId,
                UserId = request.UserId,
                InvoiceNumber = invoiceNumber,
                Subtotal = subtotal,
                Discount = request.Discount,
                Tax = request.Tax,
                Total = total,
                Profit = profit,
                Status = request.PaymentAmount >= total ? InvoiceStatus.Paid : InvoiceStatus.Partial,
                CreatedAt = DateTime.UtcNow
            };

            await db.Invoices.AddAsync(invoice, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            var invoiceItems = request.Items.Select(item =>
            {
                var product = productMap[item.ProductId];
                var lineTotal = item.UnitPrice * item.Quantity;
                var lineProfit = (item.UnitPrice - product.CostPrice) * item.Quantity;

                product.QuantityOnHand -= item.Quantity;
                db.InventoryMovements.Add(new InventoryMovement
                {
                    ProductId = product.Id,
                    Type = InventoryMovementType.Out,
                    Quantity = item.Quantity,
                    Reason = $"Invoice {invoice.InvoiceNumber}",
                    CreatedAt = DateTime.UtcNow
                });

                return new InvoiceItem
                {
                    InvoiceId = invoice.Id,
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    UnitCost = product.CostPrice,
                    LineTotal = lineTotal,
                    LineProfit = lineProfit,
                    CreatedAt = DateTime.UtcNow
                };
            });

            await db.InvoiceItems.AddRangeAsync(invoiceItems, cancellationToken);

            if (request.PaymentAmount > 0)
            {
                await db.Payments.AddAsync(new Payment
                {
                    InvoiceId = invoice.Id,
                    Amount = request.PaymentAmount,
                    Method = request.PaymentMethod,
                    PaidAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                }, cancellationToken);
            }

            settings.NextInvoiceNumber += 1;

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return invoice.Id;
        }
    }
}
