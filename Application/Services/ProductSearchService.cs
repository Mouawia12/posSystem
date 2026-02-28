using Application.DTOs;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Constants;
using Shared.Helpers;
using System.Diagnostics;
using System.IO;

namespace Application.Services
{
    public sealed class ProductSearchService : IProductSearchService
    {
        private readonly IDbContextFactory<PosDbContext> _dbContextFactory;

        public ProductSearchService(IDbContextFactory<PosDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<IReadOnlyList<ProductSearchDto>> SearchAsync(string searchTerm, int take = 100, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var normalized = searchTerm?.Trim() ?? string.Empty;
            var cappedTake = Math.Clamp(take, 1, 300);

            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var query = db.Products.AsNoTracking().Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(normalized))
            {
                query = query.Where(x => x.Name.Contains(normalized) || x.SKU.Contains(normalized));
            }

            var result = await query
                .OrderBy(x => x.Name)
                .Take(cappedTake)
                .Select(x => new ProductSearchDto
                {
                    Id = x.Id,
                    SKU = x.SKU,
                    Name = x.Name,
                    SalePrice = x.SalePrice,
                    QuantityOnHand = x.QuantityOnHand
                })
                .ToListAsync(cancellationToken);

            stopwatch.Stop();
            if (stopwatch.ElapsedMilliseconds > PerformanceTargets.SearchMilliseconds)
            {
                File.AppendAllText(
                    AppPaths.GetLogPath(),
                    $"{DateTime.UtcNow:u} PERF WARNING: Product search took {stopwatch.ElapsedMilliseconds} ms (target <= {PerformanceTargets.SearchMilliseconds} ms). Term='{normalized}', take={cappedTake}.{Environment.NewLine}");
            }

            return result;
        }
    }
}
