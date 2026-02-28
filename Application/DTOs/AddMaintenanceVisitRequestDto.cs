namespace Application.DTOs
{
    public sealed class AddMaintenanceVisitRequestDto
    {
        public long ScheduleId { get; init; }
        public DateTime VisitDate { get; init; } = DateTime.UtcNow;
        public string WorkType { get; init; } = string.Empty;
        public string? Notes { get; init; }
        public bool WarrantyCovered { get; init; }
        public decimal CostAmount { get; init; }
    }
}
