namespace Infrastructure.Data;

public sealed class BootstrapAdminOptions
{
    public const string SectionName = "BootstrapAdmin";

    public bool Enabled { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = "Administrador";
    public string Username { get; set; } = "admin";
}
