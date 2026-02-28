using Application.DTOs;

namespace Application.Services
{
    public interface IReportingService
    {
        Task<ReportSummaryDto> GetSummaryAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);
    }
}
