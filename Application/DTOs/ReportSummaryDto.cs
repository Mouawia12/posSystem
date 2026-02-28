namespace Application.DTOs
{
    public sealed class ReportSummaryDto
    {
        public DateTime FromUtc { get; init; }
        public DateTime ToUtc { get; init; }
        public int TotalInvoices { get; init; }
        public decimal GrossSales { get; init; }
        public decimal Discounts { get; init; }
        public decimal Taxes { get; init; }
        public decimal NetSales { get; init; }
        public decimal Profit { get; init; }
        public decimal TotalPayments { get; init; }
        public IReadOnlyList<ReportDailySalesDto> DailySales { get; init; } = [];
        public IReadOnlyList<ReportTopProductDto> TopProducts { get; init; } = [];
    }
}
