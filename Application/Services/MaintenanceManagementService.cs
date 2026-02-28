using Application.DTOs;
using Core.Entities;
using Core.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public sealed class MaintenanceManagementService : IMaintenanceManagementService
    {
        private readonly IDbContextFactory<PosDbContext> _dbContextFactory;

        public MaintenanceManagementService(IDbContextFactory<PosDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<IReadOnlyList<MaintenanceScheduleManagementDto>> GetSchedulesAsync(string? searchTerm = null, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var query = db.MaintenanceSchedules
                .AsNoTracking()
                .Include(x => x.Plan)
                    .ThenInclude(x => x.Device)
                .Include(x => x.Plan)
                    .ThenInclude(x => x.Customer)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var normalized = searchTerm.Trim();
                query = query.Where(x =>
                    x.Plan.Device.SerialNumber.Contains(normalized) ||
                    x.Plan.Customer.FullName.Contains(normalized));
            }

            return await query
                .OrderBy(x => x.DueDate)
                .Take(500)
                .Select(x => new MaintenanceScheduleManagementDto
                {
                    ScheduleId = x.Id,
                    PlanId = x.PlanId,
                    DeviceId = x.Plan.DeviceId,
                    SerialNumber = x.Plan.Device.SerialNumber,
                    CustomerName = x.Plan.Customer.FullName,
                    DueDate = x.DueDate,
                    PeriodMonths = x.PeriodMonths,
                    Status = x.Status
                })
                .ToListAsync(cancellationToken);
        }

        public async Task SetScheduleStatusAsync(long scheduleId, MaintenanceStatus status, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var schedule = await db.MaintenanceSchedules.FirstOrDefaultAsync(x => x.Id == scheduleId, cancellationToken)
                ?? throw new InvalidOperationException("Schedule not found.");

            schedule.Status = status;
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<long> AddVisitAsync(AddMaintenanceVisitRequestDto request, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var schedule = await db.MaintenanceSchedules.FirstOrDefaultAsync(x => x.Id == request.ScheduleId, cancellationToken)
                ?? throw new InvalidOperationException("Schedule not found.");

            var visit = new MaintenanceVisit
            {
                ScheduleId = request.ScheduleId,
                VisitDate = request.VisitDate.ToUniversalTime(),
                WorkType = request.WorkType.Trim(),
                Notes = request.Notes?.Trim(),
                WarrantyCovered = request.WarrantyCovered,
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
