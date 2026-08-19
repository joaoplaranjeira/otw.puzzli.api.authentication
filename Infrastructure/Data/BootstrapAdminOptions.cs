namespace Infrastructure.Data;

public sealed class BootstrapAdminOptions
{
    public const string SectionName = "BootstrapAdmin";

    // Default company for the legacy authentication database; matches the company that already owns the existing users.
    public static readonly Guid DefaultCompanyId = Guid.Parse("c12dd532-481b-4ff2-a4f1-a1b2ee417f75");

    public bool Enabled { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = "Administrador";
    public string Username { get; set; } = "admin";
    public Guid CompanyId { get; set; } = DefaultCompanyId;
}
