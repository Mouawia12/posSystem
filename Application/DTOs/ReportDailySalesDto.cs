namespace Application.DTOs
{
    public sealed class ReportDailySalesDto
    {
        public DateTime Date { get; init; }
        public int InvoiceCount { get; init; }
        public decimal GrossSales { get; init; }
        public decimal NetSales { get; init; }
        public decimal Profit { get; init; }
    }
}
