using Application.DTOs;
using Core.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public sealed class DashboardService : IDashboardService
    {
        private readonly IDbContextFactory<PosDbContext> _dbContextFactory;

        public DashboardService(IDbContextFactory<PosDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var utcNow = DateTime.UtcNow;
            var startOfToday = utcNow.Date;
            var endOfToday = startOfToday.AddDays(1);

            var totalProductsTask = db.Products.AsNoTracking().CountAsync(cancellationToken);
            var activeCustomersTask = db.Customers.AsNoTracking().CountAsync(cancellationToken);
            var invoicesTodayTask = db.Invoices.AsNoTracking()
                .CountAsync(x => x.CreatedAt >= startOfToday && x.CreatedAt < endOfToday, cancellationToken);
            var salesTodayTask = db.Invoices.AsNoTracking()
                .Where(x => x.CreatedAt >= startOfToday && x.CreatedAt < endOfToday)
                .Select(x => (decimal?)x.Total)
                .SumAsync(cancellationToken);
            var dueMaintenanceTask = db.MaintenanceSchedules.AsNoTracking()
                .CountAsync(x => x.Status == MaintenanceStatus.Due, cancellationToken);
            var activeWarrantyTask = db.Warranties.AsNoTracking()
                .CountAsync(x => x.Status == WarrantyStatus.Active, cancellationToken);

            await Task.WhenAll(
                totalProductsTask,
                activeCustomersTask,
                invoicesTodayTask,
                salesTodayTask,
                dueMaintenanceTask,
                activeWarrantyTask);

            return new DashboardSummaryDto
            {
                TotalProducts = totalProductsTask.Result,
                ActiveCustomers = activeCustomersTask.Result,
                InvoicesToday = invoicesTodayTask.Result,
                SalesToday = salesTodayTask.Result ?? 0m,
                DueMaintenanceCount = dueMaintenanceTask.Result,
                ActiveWarrantyCount = activeWarrantyTask.Result
            };
        }
    }
}
