using Application.DTOs;

namespace Application.Services
{
    public interface IAuthenticationService
    {
        Task EnsureDefaultOwnerAsync(string defaultPassword, CancellationToken cancellationToken = default);
        Task<AuthenticationResultDto> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);
    }
}
