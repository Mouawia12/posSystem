using Application.DTOs;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Helpers;

namespace Application.Services
{
    public sealed class UserManagementService : IUserManagementService
    {
        private readonly IDbContextFactory<PosDbContext> _dbContextFactory;

        public UserManagementService(IDbContextFactory<PosDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<IReadOnlyList<UserManagementDto>> GetUsersAsync(string? searchTerm = null, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var query = db.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var normalized = searchTerm.Trim();
                query = query.Where(x => x.Username.Contains(normalized));
            }

            return await query
                .OrderBy(x => x.Username)
                .Take(200)
                .Select(x => new UserManagementDto
                {
                    Id = x.Id,
                    Username = x.Username,
                    Role = x.Role,
                    IsActive = x.IsActive
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<long> UpsertAsync(UpsertUserRequestDto request, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var username = request.Username.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new InvalidOperationException("Username is required.");
            }

            var duplicateUser = await db.Users
                .AsNoTracking()
                .AnyAsync(x => x.Username == username && x.Id != request.Id, cancellationToken);

            if (duplicateUser)
            {
                throw new InvalidOperationException("Username already exists.");
            }

            if (request.Id is null)
            {
                if (string.IsNullOrWhiteSpace(request.Password))
                {
                    throw new InvalidOperationException("Password is required for a new user.");
                }

                var user = new User
                {
                    Username = username,
                    PasswordHash = PasswordHashing.HashPassword(request.Password),
                    Role = request.Role,
                    IsActive = request.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                await db.Users.AddAsync(user, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return user.Id;
            }

            var existing = await db.Users.FirstOrDefaultAsync(x => x.Id == request.Id.Value, cancellationToken)
                ?? throw new InvalidOperationException("User not found.");

            existing.Username = username;
            existing.Role = request.Role;
            existing.IsActive = request.IsActive;

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                existing.PasswordHash = PasswordHashing.HashPassword(request.Password);
            }

            await db.SaveChangesAsync(cancellationToken);
            return existing.Id;
        }

        public async Task DeactivateAsync(long userId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
                ?? throw new InvalidOperationException("User not found.");

            if (string.Equals(user.Username, "owner", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("The default owner account cannot be deactivated.");
            }

            user.IsActive = false;
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
