using Core.Enums;
using System.Collections.Generic;

namespace Application.DTOs
{
    public sealed class CreateInvoiceRequestDto
    {
        public long UserId { get; init; }
        public long? CustomerId { get; init; }
        public decimal Discount { get; init; }
        public decimal Tax { get; init; }
        public decimal PaymentAmount { get; init; }
        public PaymentMethod PaymentMethod { get; init; }
        public IReadOnlyList<CreateInvoiceItemDto> Items { get; init; } = [];
    }
}
