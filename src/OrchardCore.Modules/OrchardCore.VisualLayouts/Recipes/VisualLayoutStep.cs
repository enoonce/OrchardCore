using System.Text.Json.Nodes;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.VisualLayouts.Models;
using OrchardCore.VisualLayouts.Services;

namespace OrchardCore.VisualLayouts.Recipes;

/// <summary>
/// This recipe step creates a set of visual layouts.
/// </summary>
public sealed class VisualLayoutStep : NamedRecipeStepHandler
{
    private readonly VisualLayoutsManager _visualLayoutsManager;

    public VisualLayoutStep(VisualLayoutsManager visualLayoutsManager)
        : base("VisualLayouts")
    {
        _visualLayoutsManager = visualLayoutsManager;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        if (context.Step.TryGetPropertyValue("VisualLayouts", out var jsonNode) && jsonNode is JsonObject visualLayouts)
        {
            foreach (var property in visualLayouts)
            {
                var name = property.Key;
                var value = property.Value.ToObject<VisualLayout>();

                await _visualLayoutsManager.UpdateVisualLayoutAsync(name, value);
            }
        }
    }
}
