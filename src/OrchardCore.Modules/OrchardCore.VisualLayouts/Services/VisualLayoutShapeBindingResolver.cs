using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.VisualLayouts.Models;

namespace OrchardCore.VisualLayouts.Services;

/// <summary>
/// Resolves shape bindings from the visual layouts defined in the Admin UI, so that a
/// named visual layout overrides the rendering of the matching shape, without Liquid.
/// </summary>
public class VisualLayoutShapeBindingResolver : IShapeBindingResolver
{
    private VisualLayoutsDocument _visualLayoutsDocument;
    private readonly VisualLayoutsManager _visualLayoutsManager;
    private readonly PreviewVisualLayoutsProvider _previewVisualLayoutsProvider;
    private readonly IShapeFactory _shapeFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private bool? _isAdmin;

    public VisualLayoutShapeBindingResolver(
        VisualLayoutsManager visualLayoutsManager,
        PreviewVisualLayoutsProvider previewVisualLayoutsProvider,
        IShapeFactory shapeFactory,
        IHttpContextAccessor httpContextAccessor)
    {
        _visualLayoutsManager = visualLayoutsManager;
        _previewVisualLayoutsProvider = previewVisualLayoutsProvider;
        _shapeFactory = shapeFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ShapeBinding> GetShapeBindingAsync(string shapeType)
    {
        // Cache this value since the service is scoped and this method is invoked for every
        // alternate of every shape.
        _isAdmin ??= AdminAttribute.IsApplied(_httpContextAccessor.HttpContext);

        if (_isAdmin.Value)
        {
            return null;
        }

        var localVisualLayouts = _previewVisualLayoutsProvider.GetVisualLayouts();

        if (localVisualLayouts?.VisualLayouts?.TryGetValue(shapeType, out var localVisualLayout) == true)
        {
            return BuildShapeBinding(shapeType, localVisualLayout);
        }

        _visualLayoutsDocument ??= await _visualLayoutsManager.GetVisualLayoutsDocumentAsync();

        if (_visualLayoutsDocument.VisualLayouts.TryGetValue(shapeType, out var visualLayout))
        {
            return BuildShapeBinding(shapeType, visualLayout);
        }

        return null;
    }

    private ShapeBinding BuildShapeBinding(string shapeType, VisualLayout visualLayout)
    {
        return new ShapeBinding()
        {
            BindingName = shapeType,
            BindingSource = $"VisualLayouts/{shapeType}",
            BindingAsync = displayContext => RenderVisualLayoutAsync(displayContext, visualLayout),
        };
    }

    private async Task<IHtmlContent> RenderVisualLayoutAsync(DisplayContext displayContext, VisualLayout visualLayout)
    {
        var shape = await _shapeFactory.CreateAsync("VisualLayout");
        shape.Properties["Layout"] = visualLayout;

        return await displayContext.DisplayHelper.ShapeExecuteAsync(shape);
    }
}
