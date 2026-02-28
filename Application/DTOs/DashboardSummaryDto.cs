namespace Application.DTOs
{
    public sealed class DashboardSummaryDto
    {
        public int TotalProducts { get; init; }
        public int ActiveCustomers { get; init; }
        public int InvoicesToday { get; init; }
        public decimal SalesToday { get; init; }
        public int DueMaintenanceCount { get; init; }
        public int ActiveWarrantyCount { get; init; }
    }
}
