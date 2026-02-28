using Application.DTOs;
using Core.Entities;
using Core.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public sealed class ProductManagementService : IProductManagementService
    {
        private readonly IDbContextFactory<PosDbContext> _dbContextFactory;

        public ProductManagementService(IDbContextFactory<PosDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<IReadOnlyList<ProductManagementDto>> GetProductsAsync(string? searchTerm = null, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var query = db.Products.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var normalized = searchTerm.Trim();
                query = query.Where(x => x.Name.Contains(normalized) || x.SKU.Contains(normalized));
            }

            return await query
                .OrderBy(x => x.Name)
                .Take(500)
                .Select(x => new ProductManagementDto
                {
                    Id = x.Id,
                    SKU = x.SKU,
                    Name = x.Name,
                    CostPrice = x.CostPrice,
                    SalePrice = x.SalePrice,
                    QuantityOnHand = x.QuantityOnHand,
                    IsActive = x.IsActive
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<long> UpsertAsync(UpsertProductRequestDto request, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var sku = request.SKU.Trim();
            var duplicateSku = await db.Products
                .AsNoTracking()
                .AnyAsync(x => x.SKU == sku && x.Id != request.Id, cancellationToken);
            if (duplicateSku)
            {
                throw new InvalidOperationException("SKU already exists.");
            }

            if (request.Id is null)
            {
                var product = new Product
                {
                    SKU = sku,
                    Name = request.Name.Trim(),
                    CostPrice = request.CostPrice,
                    SalePrice = request.SalePrice,
                    QuantityOnHand = 0m,
                    IsActive = request.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                await db.Products.AddAsync(product, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return product.Id;
            }

            var existing = await db.Products.FirstOrDefaultAsync(x => x.Id == request.Id.Value, cancellationToken)
                ?? throw new InvalidOperationException("Product not found.");

            existing.SKU = sku;
            existing.Name = request.Name.Trim();
            existing.CostPrice = request.CostPrice;
            existing.SalePrice = request.SalePrice;
            existing.IsActive = request.IsActive;

            await db.SaveChangesAsync(cancellationToken);
            return existing.Id;
        }

        public async Task AdjustStockAsync(AdjustStockRequestDto request, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

            var product = await db.Products.FirstOrDefaultAsync(x => x.Id == request.ProductId, cancellationToken)
                ?? throw new InvalidOperationException("Product not found.");

            var delta = request.MovementType switch
            {
                InventoryMovementType.In => request.Quantity,
                InventoryMovementType.Out => -request.Quantity,
                InventoryMovementType.Adjust => request.Quantity,
                _ => throw new InvalidOperationException("Unsupported movement type.")
            };

            var nextQty = request.MovementType == InventoryMovementType.Adjust
                ? request.Quantity
                : product.QuantityOnHand + delta;

            if (nextQty < 0)
            {
                throw new InvalidOperationException("Stock cannot be negative.");
            }

            product.QuantityOnHand = nextQty;

            await db.InventoryMovements.AddAsync(new InventoryMovement
            {
                ProductId = product.Id,
                Type = request.MovementType,
                Quantity = request.Quantity,
                Reason = string.IsNullOrWhiteSpace(request.Reason) ? "Manual stock operation" : request.Reason.Trim(),
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);

            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }

        public async Task DeactivateAsync(long productId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var product = await db.Products.FirstOrDefaultAsync(x => x.Id == productId, cancellationToken)
                ?? throw new InvalidOperationException("Product not found.");

            product.IsActive = false;
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
