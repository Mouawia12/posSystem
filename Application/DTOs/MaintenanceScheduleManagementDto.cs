using Core.Enums;

namespace Application.DTOs
{
    public sealed class MaintenanceScheduleManagementDto
    {
        public long ScheduleId { get; init; }
        public long PlanId { get; init; }
        public long DeviceId { get; init; }
        public string SerialNumber { get; init; } = string.Empty;
        public string CustomerName { get; init; } = string.Empty;
        public DateTime DueDate { get; init; }
        public int PeriodMonths { get; init; }
        public MaintenanceStatus Status { get; init; }
    }
}
