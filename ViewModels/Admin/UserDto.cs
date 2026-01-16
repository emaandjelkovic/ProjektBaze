namespace AccountManager.ViewModels.Admin;

public class UserDto
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool HasAccount { get; set; }
}
