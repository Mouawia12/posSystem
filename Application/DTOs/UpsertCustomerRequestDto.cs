namespace Application.DTOs
{
    public sealed class UpsertCustomerRequestDto
    {
        public long? Id { get; init; }
        public string FullName { get; init; } = string.Empty;
        public string Phone { get; init; } = string.Empty;
        public string? Location { get; init; }
        public string? Notes { get; init; }
        public bool IsActive { get; init; } = true;
    }
}
