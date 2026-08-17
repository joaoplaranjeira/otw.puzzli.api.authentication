using Application.Security;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Data;

public sealed class BootstrapAdminSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly BootstrapAdminOptions _options;
    private readonly ILogger<BootstrapAdminSeeder> _logger;

    public BootstrapAdminSeeder(
        ApplicationDbContext context,
        IOptions<BootstrapAdminOptions> options,
        ILogger<BootstrapAdminSeeder> logger)
    {
        _context = context;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.Email) ||
            string.IsNullOrWhiteSpace(_options.Username))
        {
            throw new InvalidOperationException(
                "BootstrapAdmin:Email and BootstrapAdmin:Username are required when bootstrap is enabled.");
        }

        var email = _options.Email.Trim().ToLowerInvariant();
        var username = _options.Username.Trim();
        var user = await _context.Users
            .SingleOrDefaultAsync(
                candidate => candidate.Email != null && candidate.Email.ToLower() == email,
                cancellationToken);

        if (user is null)
        {
            var usernameExists = await _context.Users.AnyAsync(
                candidate => candidate.Username != null && candidate.Username.ToLower() == username.ToLower(),
                cancellationToken);
            if (usernameExists)
            {
                throw new InvalidOperationException(
                    $"Bootstrap admin username '{username}' is already assigned to another user.");
            }

            var now = DateTime.UtcNow;
            user = new User
            {
                Name = _options.Name.Trim(),
                Username = username,
                Email = email,
                Role = "Administrator",
                Profile = "Administrador",
                IsActive = true,
                InsertedUser = "Bootstrap",
                InsertedDate = now,
                UpdatedDate = now,
                IsDefaultPassword = false
            };
            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            user.IsActive = true;
            user.Role = "Administrator";
            user.Profile ??= "Administrador";
        }

        var assignedPermissions = await _context.UserPermissions
            .Where(permission => permission.UserId == user.Id)
            .Select(permission => permission.PermissionKey)
            .ToListAsync(cancellationToken);
        var missingPermissions = PermissionKeys.All
            .Except(assignedPermissions, StringComparer.Ordinal)
            .Select(permission => new UserPermission
            {
                UserId = user.Id,
                PermissionKey = permission
            });

        await _context.UserPermissions.AddRangeAsync(missingPermissions, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Bootstrap administrator configured for {Email}", email);
    }
}
