using Application.DTOs;

namespace Application.UseCases;

public interface IUserService
{
    Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserResponse?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<UserResponse> CreateAsync(CreateUserRequest request, string actor, CancellationToken cancellationToken = default);
    Task<UserResponse?> UpdateAsync(long id, UpdateUserRequest request, string actor, CancellationToken cancellationToken = default);
    Task<UserResponse?> ReplacePermissionsAsync(long id, IReadOnlyList<string> permissionKeys, CancellationToken cancellationToken = default);
}
