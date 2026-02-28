using Application.DTOs;

namespace Application.Services
{
    public interface IWarrantyManagementService
    {
        Task<IReadOnlyList<WarrantyManagementDto>> GetWarrantiesAsync(string? searchTerm = null, CancellationToken cancellationToken = default);
        Task<long> RegisterDeviceWarrantyAsync(RegisterDeviceWarrantyRequestDto request, CancellationToken cancellationToken = default);
        Task CancelWarrantyAsync(long warrantyId, CancellationToken cancellationToken = default);
        Task ReactivateWarrantyAsync(long warrantyId, CancellationToken cancellationToken = default);
        Task DeleteWarrantyAsync(long warrantyId, CancellationToken cancellationToken = default);
    }
}
