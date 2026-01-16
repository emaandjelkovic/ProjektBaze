namespace AccountManager.Models.Views;

public class VwAdminAccount
{
    public int AccountId { get; set; }
    public int UserId { get; set; }

    public string UserEmail { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }
    public string Address { get; set; } = string.Empty;
}
