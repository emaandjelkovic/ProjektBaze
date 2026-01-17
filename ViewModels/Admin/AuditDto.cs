namespace AccountManager.ViewModels.Admin
{
    public class AuditDto
    {
        public string AuditType { get; set; } = ""; // "USER" / "ACCOUNT"
        public int? EntityId { get; set; }          // userId ili accountId
        public int? UserId { get; set; }            // povezani userId
        public string Action { get; set; } = "";    // INSERT/UPDATE/DELETE
        public DateTime ChangedAt { get; set; }     // UTC timestamptz
        public string? OldData { get; set; }
        public string? NewData { get; set; }
    }
}
