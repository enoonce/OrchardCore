using Microsoft.AspNetCore.Http;
using System.Text.Json;
using OrchardCore.VisualLayouts.Models;
using OrchardCore.VisualLayouts.ViewModels;

namespace OrchardCore.VisualLayouts.Services;

/// <summary>
/// Provides the visual layout currently being previewed, if any.
/// </summary>
public class PreviewVisualLayoutsProvider
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly Lazy<VisualLayoutsDocument> _visualLayoutsDocument;

    public PreviewVisualLayoutsProvider(IHttpContextAccessor httpContextAccessor)
    {
        _visualLayoutsDocument = new Lazy<VisualLayoutsDocument>(() =>
        {
            var visualLayoutsDocument = new VisualLayoutsDocument();

            var httpContext = httpContextAccessor.HttpContext;

            if (httpContext.Items.TryGetValue("OrchardCore.PreviewVisualLayout", out var model))
            {
                var viewModel = model as VisualLayoutViewModel;

                if (viewModel == null || viewModel.Name == null)
                {
                    return visualLayoutsDocument;
                }

                try
                {
                    var visualLayout = JsonSerializer.Deserialize<VisualLayout>(viewModel.State ?? "{}", _jsonSerializerOptions);

                    if (visualLayout != null)
                    {
                        visualLayout.Description = viewModel.Description;
                        visualLayoutsDocument.VisualLayouts.Add(viewModel.Name, visualLayout);
                    }
                }
                catch (JsonException)
                {
                    // Ignore invalid JSON while previewing.
                }
            }

            return visualLayoutsDocument;
        });
    }

    public VisualLayoutsDocument GetVisualLayouts() => _visualLayoutsDocument.Value;
}
