using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public sealed class CreateUserRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Role { get; set; }

    [MaxLength(1000)]
    public string? PhotoUrl { get; set; }

    [MaxLength(100)]
    public string? Profile { get; set; }

    public bool IsActive { get; set; } = true;
}

public sealed class UpdateUserRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Role { get; set; }

    [MaxLength(1000)]
    public string? PhotoUrl { get; set; }

    [MaxLength(100)]
    public string? Profile { get; set; }

    public bool IsActive { get; set; }
}

public sealed class ReplacePermissionsRequest
{
    public IReadOnlyList<string> PermissionKeys { get; set; } = [];
}

public sealed class UserResponse
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Profile { get; set; }
    public bool IsActive { get; set; }
    public DateTime InsertedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public bool IsDefaultPassword { get; set; }
    public IReadOnlyList<string> Permissions { get; set; } = [];
}
