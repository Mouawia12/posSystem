using Core.Enums;

namespace Application.Services
{
    public interface IPermissionService
    {
        bool CanAccessUsersModule(UserRole role);
        bool CanProcessSales(UserRole role);
        bool CanViewReports(UserRole role);
        bool CanManageSettings(UserRole role);
    }
}
