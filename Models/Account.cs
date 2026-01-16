using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace AccountManager.Models
{
    public class Account
    {
        public int Id { get; set; }   // PK

        [Required]
        public int UserId { get; set; }   // FK (unique)


        [ValidateNever]
        public User User { get; set; } = null!;
        
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [MaxLength(100)]
        public string Address { get; set; }
    }

}
