namespace Application.DTOs
{
    public sealed class UpsertProductRequestDto
    {
        public long? Id { get; init; }
        public string SKU { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public decimal CostPrice { get; init; }
        public decimal SalePrice { get; init; }
        public bool IsActive { get; init; } = true;
    }
}
