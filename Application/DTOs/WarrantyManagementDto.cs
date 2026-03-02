using Core.Enums;

namespace Application.DTOs
{
    public sealed class WarrantyManagementDto
    {
        public long WarrantyId { get; init; }
        public long DeviceId { get; init; }
        public string DeviceType { get; init; } = string.Empty;
        public string Model { get; init; } = string.Empty;
        public string SerialNumber { get; init; } = string.Empty;
        public long CustomerId { get; init; }
        public string CustomerName { get; init; } = string.Empty;
        public string CustomerPhone { get; init; } = string.Empty;
        public string CustomerLocation { get; init; } = string.Empty;
        public long InvoiceId { get; init; }
        public string InvoiceNumber { get; init; } = string.Empty;
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public WarrantyStatus Status { get; init; }
        public bool IsOutOfWarranty => Status != WarrantyStatus.Active || EndDate.Date < DateTime.UtcNow.Date;
    }
}
