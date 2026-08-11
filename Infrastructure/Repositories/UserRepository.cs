using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken = default) =>
        _context.Users.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        return _context.Users.FirstOrDefaultAsync(
            user => user.Email != null && user.Email.ToLower() == normalizedEmail,
            cancellationToken);
    }

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var normalizedUsername = username.Trim().ToLowerInvariant();
        return _context.Users.FirstOrDefaultAsync(
            user => user.Username != null && user.Username.ToLower() == normalizedUsername,
            cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Users
            .AsNoTracking()
            .OrderBy(user => user.Name)
            .ThenBy(user => user.Id)
            .ToListAsync(cancellationToken);

    public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);

    public async Task<IReadOnlyList<string>> GetPermissionsAsync(
        long userId,
        CancellationToken cancellationToken = default) =>
        await _context.UserPermissions
            .AsNoTracking()
            .Where(permission => permission.UserId == userId)
            .Select(permission => permission.PermissionKey)
            .OrderBy(permission => permission)
            .ToListAsync(cancellationToken);

    public async Task ReplacePermissionsAsync(
        long userId,
        IEnumerable<string> permissionKeys,
        CancellationToken cancellationToken = default)
    {
        var normalizedPermissions = permissionKeys
            .Select(permission => permission.Trim())
            .Where(permission => permission.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        var currentPermissions = await _context.UserPermissions
            .Where(permission => permission.UserId == userId)
            .ToListAsync(cancellationToken);

        _context.UserPermissions.RemoveRange(currentPermissions);
        await _context.UserPermissions.AddRangeAsync(
            normalizedPermissions.Select(permission => new UserPermission
            {
                UserId = userId,
                PermissionKey = permission
            }),
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
