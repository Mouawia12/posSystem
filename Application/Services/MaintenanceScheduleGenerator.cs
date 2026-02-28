using Application.BusinessRules;
using Core.Entities;
using Core.Enums;
using System.Collections.Generic;

namespace Application.Services
{
    public sealed class MaintenanceScheduleGenerator : IMaintenanceScheduleGenerator
    {
        private static readonly int[] RequiredMilestones = [3, 6, 9, 12, 15, 18, 21, 24];

        public IReadOnlyList<MaintenanceSchedule> Generate(DateTime startDateUtc, DateTime endDateUtc)
        {
            var schedules = new List<MaintenanceSchedule>();

            foreach (var milestone in RequiredMilestones)
            {
                var dueDate = startDateUtc.AddMonths(milestone);
                if (dueDate <= endDateUtc)
                {
                    schedules.Add(new MaintenanceSchedule
                    {
                        DueDate = dueDate,
                        PeriodMonths = milestone,
                        Status = MaintenanceStatus.Due,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            var extension = 30;
            while (startDateUtc.AddMonths(extension) <= startDateUtc.AddYears(6))
            {
                schedules.Add(new MaintenanceSchedule
                {
                    DueDate = startDateUtc.AddMonths(extension),
                    PeriodMonths = extension,
                    Status = MaintenanceStatus.Due,
                    CreatedAt = DateTime.UtcNow
                });
                extension += 6;
            }

            return schedules;
        }
    }
}
