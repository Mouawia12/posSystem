namespace Core.Entities
{
    public sealed class MaintenanceVisit : BaseEntity
    {
        public long ScheduleId { get; set; }
        public DateTime VisitDate { get; set; }
        public string WorkType { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public bool WarrantyCovered { get; set; }
        public decimal CostAmount { get; set; }

        public MaintenanceSchedule Schedule { get; set; } = null!;
    }
}
