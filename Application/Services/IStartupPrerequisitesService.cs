using Application.DTOs;

namespace Application.Services
{
    public interface IStartupPrerequisitesService
    {
        Task<PrerequisiteCheckResultDto> ValidateAndPrepareAsync(CancellationToken cancellationToken = default);
    }
}
