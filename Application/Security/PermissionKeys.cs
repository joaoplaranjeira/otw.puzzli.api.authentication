namespace Application.Security;

public static class PermissionKeys
{
    public const string UsersView = "users.view";
    public const string UsersCreate = "users.create";
    public const string UsersEdit = "users.edit";
    public const string PermissionsView = "permissions.view";
    public const string PermissionsEdit = "permissions.edit";
    public const string ManagementReportsManage = "management.reports.manage";
    public const string ManagementReportsViewDriver = "management.reports.view.driver";

    public static IReadOnlyList<string> All { get; } =
    [
        UsersView,
        UsersCreate,
        UsersEdit,
        PermissionsView,
        PermissionsEdit,
        ManagementReportsManage,
        ManagementReportsViewDriver
    ];
}
