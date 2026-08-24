using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Localization;
using System.Text.Encodings.Web;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.VisualLayouts.Models;

namespace OrchardCore.VisualLayouts.Services;

/// <summary>
/// Renders a <see cref="LayoutBlock"/> into HTML, based on its type.
/// </summary>
public class VisualLayoutBlockRenderer
{
    private readonly IContentManager _contentManager;
    private readonly IContentItemDisplayManager _contentItemDisplayManager;
    private readonly IShapeFactory _shapeFactory;
    private readonly IDisplayHelper _displayHelper;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly HtmlEncoder _htmlEncoder;

    internal readonly IStringLocalizer S;

    public VisualLayoutBlockRenderer(
        IContentManager contentManager,
        IContentItemDisplayManager contentItemDisplayManager,
        IShapeFactory shapeFactory,
        IDisplayHelper displayHelper,
        IUpdateModelAccessor updateModelAccessor,
        HtmlEncoder htmlEncoder,
        IStringLocalizer<VisualLayoutBlockRenderer> stringLocalizer)
    {
        _contentManager = contentManager;
        _contentItemDisplayManager = contentItemDisplayManager;
        _shapeFactory = shapeFactory;
        _displayHelper = displayHelper;
        _updateModelAccessor = updateModelAccessor;
        _htmlEncoder = htmlEncoder;
        S = stringLocalizer;
    }

    /// <summary>
    /// Renders the provided block as HTML.
    /// </summary>
    public async Task<IHtmlContent> RenderBlockAsync(LayoutBlock block)
    {
        switch (block.Type)
        {
            case "Text":
                return new HtmlString(_htmlEncoder.Encode(block.Properties.TryGetValue("Text", out var text) ? text : string.Empty));
            case "Html":
                return new HtmlString(block.Properties.TryGetValue("Html", out var html) ? html : string.Empty);
            case "Widget":
                return await RenderWidgetBlockAsync(block);
            case "Shape":
                return await RenderShapeBlockAsync(block);
            default:
                return HtmlString.Empty;
        }
    }

    private async Task<IHtmlContent> RenderWidgetBlockAsync(LayoutBlock block)
    {
        if (!block.Properties.TryGetValue("ContentItemId", out var contentItemId) || string.IsNullOrWhiteSpace(contentItemId))
        {
            return HtmlString.Empty;
        }

        var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Published);

        if (contentItem is null)
        {
            return HtmlString.Empty;
        }

        var shape = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, _updateModelAccessor.ModelUpdater, OrchardCoreConstants.DisplayType.Detail);

        return await _displayHelper.ShapeExecuteAsync(shape);
    }

    private async Task<IHtmlContent> RenderShapeBlockAsync(LayoutBlock block)
    {
        if (!block.Properties.TryGetValue("Name", out var name) || string.IsNullOrWhiteSpace(name))
        {
            return HtmlString.Empty;
        }

        var shape = await _shapeFactory.CreateAsync(name);

        return await _displayHelper.ShapeExecuteAsync(shape);
    }
}
