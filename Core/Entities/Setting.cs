namespace Core.Entities
{
    public sealed class Setting
    {
        public int Id { get; set; } = 1;
        public string CompanyName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string InvoicePrefix { get; set; } = "INV";
        public int NextInvoiceNumber { get; set; } = 1;
        public string Currency { get; set; } = "USD";
        public string? PrinterName { get; set; }
    }
}
