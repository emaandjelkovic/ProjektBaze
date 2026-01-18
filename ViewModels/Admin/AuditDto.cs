namespace AccountManager.ViewModels.Admin
{
    public class AuditDto
    {
        public string AuditType { get; set; } = ""; 
        public int? EntityId { get; set; }          
        public int? UserId { get; set; }            
        public string Action { get; set; } = "";    
        public DateTime ChangedAt { get; set; }     
        public string? OldData { get; set; }
        public string? NewData { get; set; }
    }
}
