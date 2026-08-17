using Application.DTOs;
using Application.Security;
using Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controllers;

[ApiController]
[Authorize]
[Route("api/users/{userId:long}/permissions")]
public sealed class PermissionsController : ControllerBase
{
    private readonly IUserService _userService;

    public PermissionsController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [RequirePermission(PermissionKeys.PermissionsView)]
    public async Task<ActionResult<IReadOnlyList<string>>> Get(long userId, CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(userId, cancellationToken);
        return user is null ? NotFound() : Ok(user.Permissions);
    }

    [HttpPut]
    [RequirePermission(PermissionKeys.PermissionsEdit)]
    public async Task<ActionResult<UserResponse>> Replace(
        long userId,
        [FromBody] ReplacePermissionsRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userService.ReplacePermissionsAsync(userId, request.PermissionKeys, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }
}
