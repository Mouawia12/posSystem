namespace Application.DTOs
{
    public sealed class RegisterDeviceWarrantyRequestDto
    {
        public long CustomerId { get; init; }
        public long InvoiceId { get; init; }
        public string DeviceType { get; init; } = string.Empty;
        public string Model { get; init; } = string.Empty;
        public string SerialNumber { get; init; } = string.Empty;
        public DateTime SoldAt { get; init; } = DateTime.UtcNow;
    }
}
