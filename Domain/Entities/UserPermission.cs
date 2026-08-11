namespace Domain.Entities;

public class UserPermission
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public required string PermissionKey { get; set; }
    public User? User { get; set; }
}
