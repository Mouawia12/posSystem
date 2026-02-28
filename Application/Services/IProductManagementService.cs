using Application.DTOs;

namespace Application.Services
{
    public interface IProductManagementService
    {
        Task<IReadOnlyList<ProductManagementDto>> GetProductsAsync(string? searchTerm = null, CancellationToken cancellationToken = default);
        Task<long> UpsertAsync(UpsertProductRequestDto request, CancellationToken cancellationToken = default);
        Task AdjustStockAsync(AdjustStockRequestDto request, CancellationToken cancellationToken = default);
        Task DeactivateAsync(long productId, CancellationToken cancellationToken = default);
    }
}
