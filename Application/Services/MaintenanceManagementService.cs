using Application.DTOs;
using Application.BusinessRules;
using Core.Entities;
using Core.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public sealed class MaintenanceManagementService : IMaintenanceManagementService
    {
        private readonly IDbContextFactory<PosDbContext> _dbContextFactory;
        private readonly IWarrantyPolicyService _warrantyPolicyService;

        public MaintenanceManagementService(
            IDbContextFactory<PosDbContext> dbContextFactory,
            IWarrantyPolicyService warrantyPolicyService)
        {
            _dbContextFactory = dbContextFactory;
            _warrantyPolicyService = warrantyPolicyService;
        }

        public async Task<IReadOnlyList<MaintenanceScheduleManagementDto>> GetSchedulesAsync(string? searchTerm = null, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var query = db.MaintenanceSchedules
                .AsNoTracking()
                .Include(x => x.Plan)
                    .ThenInclude(x => x.Device)
                        .ThenInclude(x => x.SoldInvoice)
                .Include(x => x.Plan)
                    .ThenInclude(x => x.Customer)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var normalized = searchTerm.Trim();
                query = query.Where(x =>
                    x.Plan.Device.SerialNumber.Contains(normalized) ||
                    x.Plan.Customer.FullName.Contains(normalized) ||
                    x.Plan.Customer.Phone.Contains(normalized) ||
                    x.Plan.Device.Model.Contains(normalized) ||
                    (x.Plan.Device.SoldInvoice != null && x.Plan.Device.SoldInvoice.InvoiceNumber.Contains(normalized)));
            }

            return await query
                .OrderBy(x => x.DueDate)
                .Take(500)
                .Select(x => new MaintenanceScheduleManagementDto
                {
                    ScheduleId = x.Id,
                    PlanId = x.PlanId,
                    DeviceId = x.Plan.DeviceId,
                    InvoiceId = x.Plan.Device.SoldInvoiceId ?? 0,
                    InvoiceNumber = x.Plan.Device.SoldInvoice != null ? x.Plan.Device.SoldInvoice.InvoiceNumber : string.Empty,
                    DeviceType = x.Plan.Device.DeviceType,
                    Model = x.Plan.Device.Model,
                    SerialNumber = x.Plan.Device.SerialNumber,
                    CustomerName = x.Plan.Customer.FullName,
                    CustomerPhone = x.Plan.Customer.Phone,
                    CustomerLocation = x.Plan.Customer.Location ?? string.Empty,
                    DueDate = x.DueDate,
                    PeriodMonths = x.PeriodMonths,
                    RequiredMaintenanceType = $"Periodic maintenance - {x.PeriodMonths} months",
                    IsWithinWarranty = x.DueDate.Date <= x.Plan.StartDate.AddYears(2).Date,
                    Status = x.Status
                })
                .ToListAsync(cancellationToken);
        }

        public async Task SetScheduleStatusAsync(long scheduleId, MaintenanceStatus status, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var schedule = await db.MaintenanceSchedules
                .Include(x => x.Plan)
                .FirstOrDefaultAsync(x => x.Id == scheduleId, cancellationToken)
                ?? throw new InvalidOperationException("Schedule not found.");

            schedule.Status = status;

            var warranty = await db.Warranties
                .FirstOrDefaultAsync(
                    x => x.DeviceId == schedule.Plan.DeviceId && x.Status == WarrantyStatus.Active,
                    cancellationToken);

            if (warranty is not null)
            {
                _warrantyPolicyService.ApplyMaintenanceImpact(warranty, status, DateTime.UtcNow);
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<long> AddVisitAsync(AddMaintenanceVisitRequestDto request, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var schedule = await db.MaintenanceSchedules
                .Include(x => x.Plan)
                .FirstOrDefaultAsync(x => x.Id == request.ScheduleId, cancellationToken)
                ?? throw new InvalidOperationException("Schedule not found.");

            var warranty = await db.Warranties
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.DeviceId == schedule.Plan.DeviceId, cancellationToken);

            var visitDate = request.VisitDate.ToUniversalTime();
            var isWithinWarrantyWindow = warranty is not null
                && warranty.Status == WarrantyStatus.Active
                && visitDate.Date <= warranty.EndDate.Date;

            var visit = new MaintenanceVisit
            {
                ScheduleId = request.ScheduleId,
                VisitDate = visitDate,
                WorkType = request.WorkType.Trim(),
                Notes = request.Notes?.Trim(),
                WarrantyCovered = request.WarrantyCovered && isWithinWarrantyWindow,
                CostAmount = request.CostAmount,
                CreatedAt = DateTime.UtcNow
            };

            await db.MaintenanceVisits.AddAsync(visit, cancellationToken);
            schedule.Status = MaintenanceStatus.Done;
            await db.SaveChangesAsync(cancellationToken);
            return visit.Id;
        }
    }
}
