namespace AccountManager.Models
{
    public class UserAuditLog
    {
        public int Id { get; set; }          // bigint identity

        public int UserId { get; set; }      // FK (nullable, ON DELETE SET NULL)

        public string Action { get; set; } = string.Empty; // INSERT / UPDATE / DELETE

        public DateTime ChangedAt { get; set; } // timestamptz

        public string? OldData { get; set; }   // JSON snapshot
        public string? NewData { get; set; }   // JSON snapshot
    }
}
