namespace Application.DTOs
{
    public sealed class SettingsDto
    {
        public string CompanyName { get; init; } = string.Empty;
        public string? Address { get; init; }
        public string? Phone { get; init; }
        public string InvoicePrefix { get; init; } = "INV";
        public int NextInvoiceNumber { get; init; } = 1;
        public string Currency { get; init; } = "USD";
        public string? PrinterName { get; init; }
    }
}
