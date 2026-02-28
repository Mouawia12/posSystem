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

            var invoicesQuery = db.Invoices
                .AsNoTracking()
                .Where(x => x.CreatedAt >= from && x.CreatedAt <= to)
                .Where(x => x.Status == InvoiceStatus.Paid || x.Status == InvoiceStatus.Partial);

            var totalInvoices = await invoicesQuery.CountAsync(cancellationToken);
            var grossSales = await invoicesQuery.SumAsync(x => (decimal?)x.Subtotal, cancellationToken) ?? 0m;
            var discounts = await invoicesQuery.SumAsync(x => (decimal?)x.Discount, cancellationToken) ?? 0m;
            var taxes = await invoicesQuery.SumAsync(x => (decimal?)x.Tax, cancellationToken) ?? 0m;
            var netSales = await invoicesQuery.SumAsync(x => (decimal?)x.Total, cancellationToken) ?? 0m;
            var profit = await invoicesQuery.SumAsync(x => (decimal?)x.Profit, cancellationToken) ?? 0m;

            var payments = await db.Payments
                .AsNoTracking()
                .Where(x => x.PaidAt >= from && x.PaidAt <= to)
                .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

            var dailySalesRaw = await invoicesQuery
                .Select(x => new { Day = x.CreatedAt.Date, x.Subtotal, x.Total, x.Profit })
                .ToListAsync(cancellationToken);

            var dailySales = dailySalesRaw
                .GroupBy(x => x.Day)
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

            var topProducts = await db.InvoiceItems
                .AsNoTracking()
                .Where(x => x.Invoice.CreatedAt >= from && x.Invoice.CreatedAt <= to)
                .Where(x => x.Invoice.Status == InvoiceStatus.Paid || x.Invoice.Status == InvoiceStatus.Partial)
                .GroupBy(x => new { x.ProductId, x.Product.Name })
                .Select(g => new ReportTopProductDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    QuantitySold = g.Sum(x => x.Quantity),
                    SalesAmount = g.Sum(x => x.LineTotal),
                    ProfitAmount = g.Sum(x => x.LineProfit)
                })
                .OrderByDescending(x => x.SalesAmount)
                .Take(10)
                .ToListAsync(cancellationToken);

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
