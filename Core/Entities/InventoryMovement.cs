using Core.Enums;

namespace Core.Entities
{
    public sealed class InventoryMovement : BaseEntity
    {
        public long ProductId { get; set; }
        public InventoryMovementType Type { get; set; }
        public decimal Quantity { get; set; }
        public string Reason { get; set; } = string.Empty;

        public Product Product { get; set; } = null!;
    }
}
