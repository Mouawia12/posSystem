using Core.Enums;

namespace Core.Entities
{
    public sealed class Payment : BaseEntity
    {
        public long InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;

        public Invoice Invoice { get; set; } = null!;
    }
}
