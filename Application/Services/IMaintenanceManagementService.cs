using Application.DTOs;
using Core.Enums;

namespace Application.Services
{
    public interface IMaintenanceManagementService
    {
        Task<IReadOnlyList<MaintenanceScheduleManagementDto>> GetSchedulesAsync(string? searchTerm = null, CancellationToken cancellationToken = default);
        Task SetScheduleStatusAsync(long scheduleId, MaintenanceStatus status, CancellationToken cancellationToken = default);
        Task<long> AddVisitAsync(AddMaintenanceVisitRequestDto request, CancellationToken cancellationToken = default);
    }
}
