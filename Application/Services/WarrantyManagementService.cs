using Application.BusinessRules;
using Application.DTOs;
using Core.Entities;
using Core.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public sealed class WarrantyManagementService : IWarrantyManagementService
    {
        private readonly IDbContextFactory<PosDbContext> _dbContextFactory;
        private readonly IWarrantyPolicyService _warrantyPolicyService;
        private readonly IMaintenanceScheduleGenerator _maintenanceScheduleGenerator;

        public WarrantyManagementService(
            IDbContextFactory<PosDbContext> dbContextFactory,
            IWarrantyPolicyService warrantyPolicyService,
            IMaintenanceScheduleGenerator maintenanceScheduleGenerator)
        {
            _dbContextFactory = dbContextFactory;
            _warrantyPolicyService = warrantyPolicyService;
            _maintenanceScheduleGenerator = maintenanceScheduleGenerator;
        }

        public async Task<IReadOnlyList<WarrantyManagementDto>> GetWarrantiesAsync(string? searchTerm = null, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var query = db.Warranties
                .AsNoTracking()
                .Include(x => x.Device)
                .Include(x => x.Customer)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var normalized = searchTerm.Trim();
                query = query.Where(x =>
                    x.Device.SerialNumber.Contains(normalized) ||
                    x.Customer.FullName.Contains(normalized) ||
                    x.Device.Model.Contains(normalized));
            }

            return await query
                .OrderByDescending(x => x.CreatedAt)
                .Take(500)
                .Select(x => new WarrantyManagementDto
                {
                    WarrantyId = x.Id,
                    DeviceId = x.DeviceId,
                    DeviceType = x.Device.DeviceType,
                    Model = x.Device.Model,
                    SerialNumber = x.Device.SerialNumber,
                    CustomerId = x.CustomerId,
                    CustomerName = x.Customer.FullName,
                    InvoiceId = x.InvoiceId,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Status = x.Status
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<long> RegisterDeviceWarrantyAsync(RegisterDeviceWarrantyRequestDto request, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

            var customerExists = await db.Customers.AnyAsync(x => x.Id == request.CustomerId, cancellationToken);
            if (!customerExists)
            {
                throw new InvalidOperationException("Customer not found.");
            }

            var invoiceExists = await db.Invoices.AnyAsync(x => x.Id == request.InvoiceId, cancellationToken);
            if (!invoiceExists)
            {
                throw new InvalidOperationException("Invoice not found.");
            }

            var serial = request.SerialNumber.Trim();
            var serialExists = await db.Devices.AnyAsync(x => x.SerialNumber == serial, cancellationToken);
            if (serialExists)
            {
                throw new InvalidOperationException("Serial number already registered.");
            }

            var soldAt = request.SoldAt.ToUniversalTime();

            var device = new Device
            {
                DeviceType = request.DeviceType.Trim(),
                Model = request.Model.Trim(),
                SerialNumber = serial,
                CustomerId = request.CustomerId,
                SoldInvoiceId = request.InvoiceId,
                SoldAt = soldAt,
                CreatedAt = DateTime.UtcNow
            };

            await db.Devices.AddAsync(device, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            var warranty = _warrantyPolicyService.BuildDefaultWarranty(device, request.CustomerId, request.InvoiceId, soldAt);
            await db.Warranties.AddAsync(warranty, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            var plan = new MaintenancePlan
            {
                DeviceId = device.Id,
                CustomerId = request.CustomerId,
                StartDate = soldAt.Date,
                EndDate = soldAt.Date.AddYears(6),
                CreatedAt = DateTime.UtcNow
            };
            await db.MaintenancePlans.AddAsync(plan, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            var schedules = _maintenanceScheduleGenerator.Generate(plan.StartDate, plan.EndDate)
                .Select(x => new MaintenanceSchedule
                {
                    PlanId = plan.Id,
                    DueDate = x.DueDate,
                    PeriodMonths = x.PeriodMonths,
                    Status = MaintenanceStatus.Due,
                    CreatedAt = DateTime.UtcNow
                })
                .ToList();

            await db.MaintenanceSchedules.AddRangeAsync(schedules, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            return warranty.Id;
        }

        public async Task CancelWarrantyAsync(long warrantyId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var warranty = await db.Warranties.FirstOrDefaultAsync(x => x.Id == warrantyId, cancellationToken)
                ?? throw new InvalidOperationException("Warranty not found.");

            warranty.Status = WarrantyStatus.Canceled;
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task ReactivateWarrantyAsync(long warrantyId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var warranty = await db.Warranties.FirstOrDefaultAsync(x => x.Id == warrantyId, cancellationToken)
                ?? throw new InvalidOperationException("Warranty not found.");

            if (warranty.EndDate.Date < DateTime.UtcNow.Date)
            {
                throw new InvalidOperationException("Cannot reactivate an expired warranty.");
            }

            warranty.Status = WarrantyStatus.Active;
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteWarrantyAsync(long warrantyId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var warranty = await db.Warranties.FirstOrDefaultAsync(x => x.Id == warrantyId, cancellationToken)
                ?? throw new InvalidOperationException("Warranty not found.");

            db.Warranties.Remove(warranty);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
