using Core.Enums;

namespace Application.Services
{
    public sealed class UserContextService : IUserContextService
    {
        public long UserId { get; private set; } = 1;
        public string Username { get; private set; } = "owner";
        public UserRole Role { get; private set; } = UserRole.Owner;

        public void SetUser(long userId, string username, UserRole role)
        {
            UserId = userId;
            Username = username;
            Role = role;
        }
    }
}
