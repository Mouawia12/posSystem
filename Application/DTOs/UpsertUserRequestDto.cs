using Core.Enums;

namespace Application.DTOs
{
    public sealed class UpsertUserRequestDto
    {
        public long? Id { get; init; }
        public string Username { get; init; } = string.Empty;
        public string? Password { get; init; }
        public UserRole Role { get; init; } = UserRole.Cashier;
        public bool IsActive { get; init; } = true;
    }
}
