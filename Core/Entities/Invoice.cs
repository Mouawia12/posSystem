using Core.Enums;
using System.Collections.Generic;

namespace Core.Entities
{
    public sealed class Invoice : BaseEntity
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public long? CustomerId { get; set; }
        public long UserId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public decimal Profit { get; set; }
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

        public Customer? Customer { get; set; }
        public User User { get; set; } = null!;
        public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
