using Application.DTOs;
using Core.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public sealed class ReportingService : IReportingService
    {
        private readonly IDbContextFactory<PosDbContext> _dbContextFactory;

        public ReportingService(IDbContextFactory<PosDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<ReportSummaryDto> GetSummaryAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var from = fromUtc.ToUniversalTime();
            var to = toUtc.ToUniversalTime();

            var invoiceRows = await db.Invoices
                .AsNoTracking()
                .Where(x => x.CreatedAt >= from && x.CreatedAt <= to)
                .Where(x => x.Status == InvoiceStatus.Paid || x.Status == InvoiceStatus.Partial)
                .Select(x => new
                {
                    x.Id,
                    x.CreatedAt,
                    x.Subtotal,
                    x.Discount,
                    x.Tax,
                    x.Total,
                    x.Profit
                })
                .ToListAsync(cancellationToken);

            var totalInvoices = invoiceRows.Count;
            var grossSales = invoiceRows.Sum(x => x.Subtotal);
            var discounts = invoiceRows.Sum(x => x.Discount);
            var taxes = invoiceRows.Sum(x => x.Tax);
            var netSales = invoiceRows.Sum(x => x.Total);
            var profit = invoiceRows.Sum(x => x.Profit);

            var payments = await db.Payments
                .AsNoTracking()
                .Where(x => x.PaidAt >= from && x.PaidAt <= to)
                .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

            var dailySales = invoiceRows
                .GroupBy(x => x.CreatedAt.Date)
                .OrderBy(x => x.Key)
                .Select(g => new ReportDailySalesDto
                {
                    Date = g.Key,
                    InvoiceCount = g.Count(),
                    GrossSales = g.Sum(x => x.Subtotal),
                    NetSales = g.Sum(x => x.Total),
                    Profit = g.Sum(x => x.Profit)
                })
                .ToList();

            var invoiceIds = invoiceRows.Select(x => x.Id).ToHashSet();
            var topProductsRows = await db.InvoiceItems
                .AsNoTracking()
                .Where(x => invoiceIds.Contains(x.InvoiceId))
                .Select(x => new
                {
                    x.ProductId,
                    ProductName = x.Product.Name,
                    x.Quantity,
                    x.LineTotal,
                    x.LineProfit
                })
                .ToListAsync(cancellationToken);

            var topProducts = topProductsRows
                .GroupBy(x => new { x.ProductId, x.ProductName })
                .Select(g => new ReportTopProductDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    QuantitySold = g.Sum(x => x.Quantity),
                    SalesAmount = g.Sum(x => x.LineTotal),
                    ProfitAmount = g.Sum(x => x.LineProfit)
                })
                .OrderByDescending(x => x.SalesAmount)
                .Take(10)
                .ToList();

            return new ReportSummaryDto
            {
                FromUtc = from,
                ToUtc = to,
                TotalInvoices = totalInvoices,
                GrossSales = grossSales,
                Discounts = discounts,
                Taxes = taxes,
                NetSales = netSales,
                Profit = profit,
                TotalPayments = payments,
                DailySales = dailySales,
                TopProducts = topProducts
            };
        }
    }
}
