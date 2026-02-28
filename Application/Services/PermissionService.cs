using Core.Enums;

namespace Application.Services
{
    public sealed class PermissionService : IPermissionService
    {
        public bool CanAccessUsersModule(UserRole role)
        {
            return role == UserRole.Owner;
        }

        public bool CanProcessSales(UserRole role)
        {
            return role is UserRole.Owner or UserRole.Manager or UserRole.Cashier;
        }

        public bool CanViewReports(UserRole role)
        {
            return role is UserRole.Owner or UserRole.Manager;
        }

        public bool CanManageSettings(UserRole role)
        {
            return role is UserRole.Owner or UserRole.Manager;
        }
    }
}
