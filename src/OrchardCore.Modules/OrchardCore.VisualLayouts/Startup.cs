using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;
using OrchardCore.VisualLayouts.Deployment;
using OrchardCore.VisualLayouts.Recipes;
using OrchardCore.VisualLayouts.Services;

namespace OrchardCore.VisualLayouts;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddResourceConfiguration<ResourceManagementOptionsConfiguration>();

        services.AddScoped<IShapeBindingResolver, VisualLayoutShapeBindingResolver>();
        services.AddScoped<PreviewVisualLayoutsProvider>();
        services.AddScoped<VisualLayoutsManager>();
        services.AddScoped<VisualLayoutBlockRenderer>();
        services.AddPermissionProvider<Permissions>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddRecipeExecutionStep<VisualLayoutStep>();
        services.AddDeployment<AllVisualLayoutsDeploymentSource, AllVisualLayoutsDeploymentStep, AllVisualLayoutsDeploymentStepDriver>();
    }
}
