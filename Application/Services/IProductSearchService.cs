using Application.DTOs;

namespace Application.Services
{
    public interface IProductSearchService
    {
        Task<IReadOnlyList<ProductSearchDto>> SearchAsync(string searchTerm, int take = 100, CancellationToken cancellationToken = default);
    }
}
