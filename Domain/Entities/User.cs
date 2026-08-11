namespace Domain.Entities;

public class User
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? Salt { get; set; }
    public string? Role { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Profile { get; set; }
    public bool IsActive { get; set; }
    public required string InsertedUser { get; set; }
    public string? UpdatedUser { get; set; }
    public DateTime InsertedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public bool IsDefaultPassword { get; set; }
}
