using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.VisualLayouts.Deployment;

public sealed class AllVisualLayoutsDeploymentStepDriver : DisplayDriver<DeploymentStep, AllVisualLayoutsDeploymentStep>
{
    public override Task<IDisplayResult> DisplayAsync(AllVisualLayoutsDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("AllVisualLayoutsDeploymentStep_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
                View("AllVisualLayoutsDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(AllVisualLayoutsDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<AllVisualLayoutsDeploymentStep>("AllVisualLayoutsDeploymentStep_Fields_Edit", model =>
        {
        }).Location("Content");
    }
}
