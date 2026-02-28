using System.Collections.Generic;

namespace Core.Entities
{
    public sealed class Product : BaseEntity
    {
        public string SKU { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal CostPrice { get; set; }
        public decimal SalePrice { get; set; }
        public decimal QuantityOnHand { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
        public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
    }
}
