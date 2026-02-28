using System.Collections.Generic;

namespace Core.Entities
{
    public sealed class MaintenancePlan : BaseEntity
    {
        public long DeviceId { get; set; }
        public long CustomerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public Device Device { get; set; } = null!;
        public Customer Customer { get; set; } = null!;
        public ICollection<MaintenanceSchedule> Schedules { get; set; } = new List<MaintenanceSchedule>();
    }
}
