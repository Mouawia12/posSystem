using Core.Enums;

namespace Application.Services
{
    public interface IUserContextService
    {
        long UserId { get; }
        string Username { get; }
        UserRole Role { get; }
        void SetUser(long userId, string username, UserRole role);
    }
}
