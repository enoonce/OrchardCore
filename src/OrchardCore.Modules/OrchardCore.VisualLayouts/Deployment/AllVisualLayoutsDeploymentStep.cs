using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.VisualLayouts.Deployment;

/// <summary>
/// Adds visual layouts to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllVisualLayoutsDeploymentStep : DeploymentStep
{
    public AllVisualLayoutsDeploymentStep()
    {
        Name = "AllVisualLayouts";
    }

    public AllVisualLayoutsDeploymentStep(IStringLocalizer<AllVisualLayoutsDeploymentStep> S)
        : this()
    {
        Category = S["Development"];
    }
}
