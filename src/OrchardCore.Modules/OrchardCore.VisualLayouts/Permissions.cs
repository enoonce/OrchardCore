using OrchardCore.Security.Permissions;

namespace OrchardCore.VisualLayouts;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageVisualLayouts = new("ManageVisualLayouts", "Manage visual layouts", isSecurityCritical: true);

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ManageVisualLayouts,
    ];

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult(_allPermissions);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions = _allPermissions,
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Editor,
            Permissions = _allPermissions,
        },
    ];
}
