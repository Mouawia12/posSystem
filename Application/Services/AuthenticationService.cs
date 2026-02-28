using Application.DTOs;
using Core.Entities;
using Core.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Helpers;
using System.Collections.Concurrent;

namespace Application.Services
{
    public sealed class AuthenticationService : IAuthenticationService
    {
        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

        private readonly IDbContextFactory<PosDbContext> _dbContextFactory;
        private readonly ConcurrentDictionary<string, LoginAttemptState> _attempts = new(StringComparer.OrdinalIgnoreCase);

        public AuthenticationService(IDbContextFactory<PosDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task EnsureDefaultOwnerAsync(string defaultPassword, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(defaultPassword))
            {
                throw new InvalidOperationException("Default owner password is required.");
            }

            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var owner = await db.Users.FirstOrDefaultAsync(
                x => x.Username == "owner",
                cancellationToken);

            if (owner is null)
            {
                owner = new User
                {
                    Username = "owner",
                    PasswordHash = PasswordHashing.HashPassword(defaultPassword),
                    Role = UserRole.Owner,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await db.Users.AddAsync(owner, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return;
            }

            if (string.IsNullOrWhiteSpace(owner.PasswordHash))
            {
                owner.PasswordHash = PasswordHashing.HashPassword(defaultPassword);
                owner.IsActive = true;
                owner.Role = UserRole.Owner;
                await db.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<AuthenticationResultDto> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            var normalizedUsername = (username ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedUsername) || string.IsNullOrWhiteSpace(password))
            {
                return new AuthenticationResultDto
                {
                    Succeeded = false,
                    Message = "Username and password are required.",
                    RemainingAttempts = MaxFailedAttempts
                };
            }

            var now = DateTime.UtcNow;
            var state = _attempts.GetOrAdd(normalizedUsername, _ => new LoginAttemptState());
            lock (state)
            {
                if (state.LockedUntilUtc.HasValue && state.LockedUntilUtc.Value > now)
                {
                    return new AuthenticationResultDto
                    {
                        Succeeded = false,
                        Message = "Account is temporarily locked.",
                        LockedUntilUtc = state.LockedUntilUtc,
                        RemainingAttempts = 0
                    };
                }

                if (state.LockedUntilUtc.HasValue && state.LockedUntilUtc.Value <= now)
                {
                    state.LockedUntilUtc = null;
                    state.FailedAttempts = 0;
                }
            }

            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var user = await db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Username == normalizedUsername, cancellationToken);

            var valid = user is not null
                && user.IsActive
                && PasswordHashing.VerifyPassword(password, user.PasswordHash);

            if (valid)
            {
                _attempts.TryRemove(normalizedUsername, out _);
                return new AuthenticationResultDto
                {
                    Succeeded = true,
                    Message = "Authenticated.",
                    UserId = user!.Id,
                    Username = user.Username,
                    Role = user.Role,
                    RemainingAttempts = MaxFailedAttempts
                };
            }

            return RegisterFailure(normalizedUsername, now);
        }

        private AuthenticationResultDto RegisterFailure(string username, DateTime now)
        {
            var state = _attempts.GetOrAdd(username, _ => new LoginAttemptState());
            lock (state)
            {
                state.FailedAttempts += 1;

                if (state.FailedAttempts >= MaxFailedAttempts)
                {
                    state.LockedUntilUtc = now.Add(LockoutDuration);
                    state.FailedAttempts = 0;

                    return new AuthenticationResultDto
                    {
                        Succeeded = false,
                        Message = "Too many failed attempts. Account is locked.",
                        LockedUntilUtc = state.LockedUntilUtc,
                        RemainingAttempts = 0
                    };
                }

                return new AuthenticationResultDto
                {
                    Succeeded = false,
                    Message = "Invalid username or password.",
                    RemainingAttempts = MaxFailedAttempts - state.FailedAttempts
                };
            }
        }

        private sealed class LoginAttemptState
        {
            public int FailedAttempts { get; set; }
            public DateTime? LockedUntilUtc { get; set; }
        }
    }
}
