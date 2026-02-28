namespace Application.DTOs
{
    public sealed class CreateInvoiceItemDto
    {
        public long ProductId { get; init; }
        public decimal Quantity { get; init; }
        public decimal UnitPrice { get; init; }
    }
}
