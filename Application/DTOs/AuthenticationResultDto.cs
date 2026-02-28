using Core.Enums;

namespace Application.DTOs
{
    public sealed class AuthenticationResultDto
    {
        public bool Succeeded { get; init; }
        public string Message { get; init; } = string.Empty;
        public int RemainingAttempts { get; init; }
        public DateTime? LockedUntilUtc { get; init; }
        public long UserId { get; init; }
        public string Username { get; init; } = string.Empty;
        public UserRole Role { get; init; } = UserRole.Cashier;
    }
}
