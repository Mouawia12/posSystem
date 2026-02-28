using Application.DTOs;

namespace Application.Services
{
    public interface ICustomerManagementService
    {
        Task<IReadOnlyList<CustomerManagementDto>> GetCustomersAsync(string? searchTerm = null, CancellationToken cancellationToken = default);
        Task<long> UpsertAsync(UpsertCustomerRequestDto request, CancellationToken cancellationToken = default);
        Task DeactivateAsync(long customerId, CancellationToken cancellationToken = default);
        Task ReactivateAsync(long customerId, CancellationToken cancellationToken = default);
        Task DeleteAsync(long customerId, CancellationToken cancellationToken = default);
    }
}
