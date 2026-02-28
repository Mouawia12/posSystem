namespace Core.Entities
{
    public sealed class Device : BaseEntity
    {
        public string DeviceType { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public long CustomerId { get; set; }
        public long? SoldInvoiceId { get; set; }
        public DateTime? SoldAt { get; set; }

        public Customer Customer { get; set; } = null!;
        public Invoice? SoldInvoice { get; set; }
    }
}
