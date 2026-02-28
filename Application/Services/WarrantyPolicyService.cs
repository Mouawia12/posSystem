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

        public void ApplyMaintenanceImpact(Warranty warranty, MaintenanceStatus maintenanceStatus, DateTime affectedAtUtc)
        {
            if (maintenanceStatus != MaintenanceStatus.Skipped || warranty.Status != WarrantyStatus.Active)
            {
                return;
            }

            var effectiveDate = affectedAtUtc.Date;
            if (effectiveDate < warranty.EndDate)
            {
                warranty.EndDate = effectiveDate;
            }

            warranty.Status = WarrantyStatus.Canceled;
        }
    }
}
