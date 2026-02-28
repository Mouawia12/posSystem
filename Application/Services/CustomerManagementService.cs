using Application.DTOs;
using Core.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public sealed class CustomerManagementService : ICustomerManagementService
    {
        private readonly IDbContextFactory<PosDbContext> _dbContextFactory;

        public CustomerManagementService(IDbContextFactory<PosDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<IReadOnlyList<CustomerManagementDto>> GetCustomersAsync(string? searchTerm = null, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var query = db.Customers.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var normalized = searchTerm.Trim();
                query = query.Where(x => x.FullName.Contains(normalized) || x.Phone.Contains(normalized));
            }

            return await query
                .OrderBy(x => x.FullName)
                .Take(500)
                .Select(x => new CustomerManagementDto
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    Phone = x.Phone,
                    Location = x.Location,
                    Notes = x.Notes,
                    IsActive = true
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<long> UpsertAsync(UpsertCustomerRequestDto request, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            if (request.Id is null)
            {
                var entity = new Customer
                {
                    FullName = request.FullName.Trim(),
                    Phone = request.Phone.Trim(),
                    Location = request.Location?.Trim(),
                    Notes = request.Notes?.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                await db.Customers.AddAsync(entity, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return entity.Id;
            }

            var existing = await db.Customers.FirstOrDefaultAsync(x => x.Id == request.Id.Value, cancellationToken)
                ?? throw new InvalidOperationException("Customer not found.");

            existing.FullName = request.FullName.Trim();
            existing.Phone = request.Phone.Trim();
            existing.Location = request.Location?.Trim();
            existing.Notes = request.Notes?.Trim();

            await db.SaveChangesAsync(cancellationToken);
            return existing.Id;
        }

        public async Task DeactivateAsync(long customerId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var customer = await db.Customers.FirstOrDefaultAsync(x => x.Id == customerId, cancellationToken)
                ?? throw new InvalidOperationException("Customer not found.");

            customer.Notes = $"{customer.Notes} [DEACTIVATED {DateTime.UtcNow:u}]".Trim();
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
