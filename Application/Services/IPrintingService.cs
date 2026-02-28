using Core.Enums;

namespace Application.Services
{
    public interface IPrintingService
    {
        Task<bool> PrintInvoiceAsync(long invoiceId, PrintTemplateType templateType, string? printerName = null, CancellationToken cancellationToken = default);
    }
}
