namespace Core.Entities
{
    public sealed class InvoiceItem : BaseEntity
    {
        public long InvoiceId { get; set; }
        public long ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitCost { get; set; }
        public decimal LineTotal { get; set; }
        public decimal LineProfit { get; set; }

        public Invoice Invoice { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
