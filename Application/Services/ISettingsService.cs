using Application.DTOs;

namespace Application.Services
{
    public interface ISettingsService
    {
        Task<SettingsDto> GetAsync(CancellationToken cancellationToken = default);
        Task SaveAsync(SettingsDto settings, CancellationToken cancellationToken = default);
    }
}
