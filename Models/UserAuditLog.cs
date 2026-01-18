namespace AccountManager.Models
{
    public class UserAuditLog
    {
        public int Id { get; set; }         

        public int UserId { get; set; }      

        public string Action { get; set; } = string.Empty; 

        public DateTime ChangedAt { get; set; } 

        public string? OldData { get; set; }   
        public string? NewData { get; set; }  
    }
}
