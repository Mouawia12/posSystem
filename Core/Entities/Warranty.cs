using Core.Enums;

namespace Core.Entities
{
    public sealed class Warranty : BaseEntity
    {
        public long DeviceId { get; set; }
        public long CustomerId { get; set; }
        public long InvoiceId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public WarrantyStatus Status { get; set; } = WarrantyStatus.Active;

        public Device Device { get; set; } = null!;
        public Customer Customer { get; set; } = null!;
        public Invoice Invoice { get; set; } = null!;
    }
}
