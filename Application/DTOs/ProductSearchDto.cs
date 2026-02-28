namespace Application.DTOs
{
    public sealed class ProductSearchDto
    {
        public long Id { get; init; }
        public string SKU { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public decimal SalePrice { get; init; }
        public decimal QuantityOnHand { get; init; }
    }
}
