using System.ComponentModel.DataAnnotations;

namespace AccountManager.Models
{
    public class UserSession
    {
        public Guid Id { get; set; }   // uuid

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public DateTime LastSeenAt { get; set; }

        [MaxLength(50)]
        public string? IpAddress { get; set; }

        [MaxLength(300)]
        public string? UserAgent { get; set; }

        public DateTime? RevokedAt { get; set; }
    }
}
