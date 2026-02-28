using Core.Enums;
using System.Collections.Generic;

namespace Core.Entities
{
    public sealed class User : BaseEntity
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Cashier;
        public bool IsActive { get; set; } = true;

        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
