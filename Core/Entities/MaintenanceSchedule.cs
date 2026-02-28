using Core.Enums;
using System.Collections.Generic;

namespace Core.Entities
{
    public sealed class MaintenanceSchedule : BaseEntity
    {
        public long PlanId { get; set; }
        public DateTime DueDate { get; set; }
        public int PeriodMonths { get; set; }
        public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Due;

        public MaintenancePlan Plan { get; set; } = null!;
        public ICollection<MaintenanceVisit> Visits { get; set; } = new List<MaintenanceVisit>();
    }
}
