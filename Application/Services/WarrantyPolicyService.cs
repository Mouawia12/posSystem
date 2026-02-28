using Application.BusinessRules;
using Core.Entities;
using Core.Enums;

namespace Application.Services
{
    public sealed class WarrantyPolicyService : IWarrantyPolicyService
    {
        public Warranty BuildDefaultWarranty(Device device, long customerId, long invoiceId, DateTime soldAtUtc)
        {
            return new Warranty
            {
                DeviceId = device.Id,
                CustomerId = customerId,
                InvoiceId = invoiceId,
                StartDate = soldAtUtc.Date,
                EndDate = soldAtUtc.Date.AddYears(2),
                Status = WarrantyStatus.Active,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
