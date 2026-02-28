using Core.Enums;

namespace Application.DTOs
{
    public sealed class UserManagementDto
    {
        public long Id { get; init; }
        public string Username { get; init; } = string.Empty;
        public UserRole Role { get; init; }
        public bool IsActive { get; init; }
    }
}
