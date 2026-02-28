namespace Core.Entities
{
    public sealed class AuditLog : BaseEntity
    {
        public long UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;

        public User User { get; set; } = null!;
    }
}
