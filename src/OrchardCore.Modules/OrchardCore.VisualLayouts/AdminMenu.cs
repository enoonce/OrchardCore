using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.VisualLayouts;

public sealed class AdminMenu : AdminNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Design"], design => design
                .Add(S["Visual Layouts"], S["Visual Layouts"].PrefixPosition(), import => import
                    .Action("Index", "VisualLayout", "OrchardCore.VisualLayouts")
                    .Permission(Permissions.ManageVisualLayouts)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
