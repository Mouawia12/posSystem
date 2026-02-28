using Core.Entities;
using Core.Enums;

namespace Application.BusinessRules
{
    public interface IWarrantyPolicyService
    {
        Warranty BuildDefaultWarranty(Device device, long customerId, long invoiceId, DateTime soldAtUtc);
        void ApplyMaintenanceImpact(Warranty warranty, MaintenanceStatus maintenanceStatus, DateTime affectedAtUtc);
    }
}
