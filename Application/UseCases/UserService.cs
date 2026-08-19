using Application.DTOs;
using Domain.Entities;
using Domain.Repositories;

namespace Application.UseCases;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        var responses = new List<UserResponse>(users.Count);
        foreach (var user in users)
        {
            responses.Add(await MapAsync(user, cancellationToken));
        }

        return responses;
    }

    public async Task<UserResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        return user is null ? null : await MapAsync(user, cancellationToken);
    }

    public async Task<UserResponse> CreateAsync(
        CreateUserRequest request,
        string actor,
        CancellationToken cancellationToken = default)
    {
        await EnsureIdentityIsAvailableAsync(request.Email, request.Username, null, cancellationToken);
        var now = DateTime.UtcNow;
        var user = new User
        {
            CompanyId = request.CompanyId,
            Name = request.Name.Trim(),
            Username = request.Username.Trim(),
            Email = NormalizeEmail(request.Email),
            Role = request.Role?.Trim(),
            PhotoUrl = request.PhotoUrl?.Trim(),
            Profile = request.Profile?.Trim(),
            IsActive = request.IsActive,
            InsertedUser = actor,
            InsertedDate = now,
            UpdatedDate = now,
            IsDefaultPassword = false
        };

        await _userRepository.CreateAsync(user, cancellationToken);
        return await MapAsync(user, cancellationToken);
    }

    public async Task<UserResponse?> UpdateAsync(
        long id,
        UpdateUserRequest request,
        string actor,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return null;
        }

        await EnsureIdentityIsAvailableAsync(request.Email, request.Username, id, cancellationToken);
        user.CompanyId = request.CompanyId;
        user.Name = request.Name.Trim();
        user.Username = request.Username.Trim();
        user.Email = NormalizeEmail(request.Email);
        user.Role = request.Role?.Trim();
        user.PhotoUrl = request.PhotoUrl?.Trim();
        user.Profile = request.Profile?.Trim();
        user.IsActive = request.IsActive;
        user.UpdatedUser = actor;
        user.UpdatedDate = DateTime.UtcNow;
        await _userRepository.SaveChangesAsync(cancellationToken);

        return await MapAsync(user, cancellationToken);
    }

    public async Task<UserResponse?> ReplacePermissionsAsync(
        long id,
        IReadOnlyList<string> permissionKeys,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return null;
        }

        await _userRepository.ReplacePermissionsAsync(id, permissionKeys, cancellationToken);
        return await MapAsync(user, cancellationToken);
    }

    private async Task EnsureIdentityIsAvailableAsync(
        string email,
        string username,
        long? currentUserId,
        CancellationToken cancellationToken)
    {
        var emailOwner = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (emailOwner is not null && emailOwner.Id != currentUserId)
        {
            throw new InvalidOperationException("Já existe um utilizador com este email.");
        }

        var usernameOwner = await _userRepository.GetByUsernameAsync(username, cancellationToken);
        if (usernameOwner is not null && usernameOwner.Id != currentUserId)
        {
            throw new InvalidOperationException("Já existe um utilizador com este username.");
        }
    }

    private async Task<UserResponse> MapAsync(User user, CancellationToken cancellationToken) => new()
    {
        Id = user.Id,
        CompanyId = user.CompanyId,
        Name = user.Name,
        Username = user.Username,
        Email = user.Email,
        Role = user.Role,
        PhotoUrl = user.PhotoUrl,
        Profile = user.Profile,
        IsActive = user.IsActive,
        InsertedDate = user.InsertedDate,
        UpdatedDate = user.UpdatedDate,
        IsDefaultPassword = user.IsDefaultPassword,
        IsGlobalAdministrator = user.Profile == "Administrador Sistema",
        Permissions = await _userRepository.GetPermissionsAsync(user.Id, cancellationToken)
    };

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
