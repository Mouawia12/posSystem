using Application.DTOs;

namespace Application.Services
{
    public interface IUserManagementService
    {
        Task<IReadOnlyList<UserManagementDto>> GetUsersAsync(string? searchTerm = null, CancellationToken cancellationToken = default);
        Task<long> UpsertAsync(UpsertUserRequestDto request, CancellationToken cancellationToken = default);
        Task DeactivateAsync(long userId, CancellationToken cancellationToken = default);
    }
}
