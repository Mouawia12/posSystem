using Core.Entities;

namespace Application.BusinessRules
{
    public interface IWarrantyPolicyService
    {
        Warranty BuildDefaultWarranty(Device device, long customerId, long invoiceId, DateTime soldAtUtc);
    }
}
