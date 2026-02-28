using Application.DTOs;

namespace Application.Services
{
    public interface IInvoiceService
    {
        Task<long> CreateInvoiceAsync(CreateInvoiceRequestDto request, CancellationToken cancellationToken = default);
    }
}
