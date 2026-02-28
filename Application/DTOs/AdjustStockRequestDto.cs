using Core.Enums;

namespace Application.DTOs
{
    public sealed class AdjustStockRequestDto
    {
        public long ProductId { get; init; }
        public InventoryMovementType MovementType { get; init; }
        public decimal Quantity { get; init; }
        public string Reason { get; init; } = string.Empty;
    }
}
