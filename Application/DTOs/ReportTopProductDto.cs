namespace Application.DTOs
{
    public sealed class ReportTopProductDto
    {
        public long ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public decimal QuantitySold { get; init; }
        public decimal SalesAmount { get; init; }
        public decimal ProfitAmount { get; init; }
    }
}
