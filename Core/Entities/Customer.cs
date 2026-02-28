using System.Collections.Generic;

namespace Core.Entities
{
    public sealed class Customer : BaseEntity
    {
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? Notes { get; set; }

        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public ICollection<Device> Devices { get; set; } = new List<Device>();
        public ICollection<Warranty> Warranties { get; set; } = new List<Warranty>();
        public ICollection<MaintenancePlan> MaintenancePlans { get; set; } = new List<MaintenancePlan>();
    }
}
