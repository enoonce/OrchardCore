using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.VisualLayouts.Services;

namespace OrchardCore.VisualLayouts.Deployment;

public sealed class AllVisualLayoutsDeploymentSource
    : DeploymentSourceBase<AllVisualLayoutsDeploymentStep>
{
    private readonly VisualLayoutsManager _visualLayoutsManager;

    public AllVisualLayoutsDeploymentSource(VisualLayoutsManager visualLayoutsManager)
    {
        _visualLayoutsManager = visualLayoutsManager;
    }

    protected override async Task ProcessAsync(AllVisualLayoutsDeploymentStep step, DeploymentPlanResult result)
    {
        var visualLayoutObjects = new JsonObject();
        var visualLayouts = await _visualLayoutsManager.GetVisualLayoutsDocumentAsync();

        foreach (var visualLayout in visualLayouts.VisualLayouts)
        {
            visualLayoutObjects[visualLayout.Key] = JObject.FromObject(visualLayout.Value);
        }

        result.Steps.Add(new JsonObject
        {
            ["name"] = "VisualLayouts",
            ["VisualLayouts"] = visualLayoutObjects,
        });
    }
}
