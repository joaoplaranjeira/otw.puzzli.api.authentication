using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Controllers;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly HashSet<string> _requiredPermissions;

    public RequirePermissionAttribute(params string[] requiredPermissions)
    {
        _requiredPermissions = new HashSet<string>(requiredPermissions, StringComparer.Ordinal);
    }

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return Task.CompletedTask;
        }

        var hasPermission = context.HttpContext.User
            .FindAll("permissions")
            .Any(claim => _requiredPermissions.Contains(claim.Value));
        if (!hasPermission)
        {
            context.Result = new ForbidResult();
        }

        return Task.CompletedTask;
    }
}
