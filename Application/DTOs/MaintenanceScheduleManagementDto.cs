using Core.Enums;

namespace Application.DTOs
{
    public sealed class MaintenanceScheduleManagementDto
    {
        public long ScheduleId { get; init; }
        public long PlanId { get; init; }
        public long DeviceId { get; init; }
        public long InvoiceId { get; init; }
        public string InvoiceNumber { get; init; } = string.Empty;
        public string DeviceType { get; init; } = string.Empty;
        public string Model { get; init; } = string.Empty;
        public string SerialNumber { get; init; } = string.Empty;
        public string CustomerName { get; init; } = string.Empty;
        public string CustomerPhone { get; init; } = string.Empty;
        public string CustomerLocation { get; init; } = string.Empty;
        public DateTime DueDate { get; init; }
        public int PeriodMonths { get; init; }
        public string RequiredMaintenanceType { get; init; } = string.Empty;
        public bool IsWithinWarranty { get; init; }
        public MaintenanceStatus Status { get; init; }
    }
}
