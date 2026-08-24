using OrchardCore.Documents;
using OrchardCore.VisualLayouts.Models;

namespace OrchardCore.VisualLayouts.Services;

public class VisualLayoutsManager
{
    private readonly IDocumentManager<VisualLayoutsDocument> _documentManager;

    public VisualLayoutsManager(IDocumentManager<VisualLayoutsDocument> documentManager) => _documentManager = documentManager;

    /// <summary>
    /// Loads the visual layouts document from the store for updating and that should not be cached.
    /// </summary>
    public Task<VisualLayoutsDocument> LoadVisualLayoutsDocumentAsync() => _documentManager.GetOrCreateMutableAsync();

    /// <summary>
    /// Gets the visual layouts document from the cache for sharing and that should not be updated.
    /// </summary>
    public Task<VisualLayoutsDocument> GetVisualLayoutsDocumentAsync() => _documentManager.GetOrCreateImmutableAsync();

    public async Task RemoveVisualLayoutAsync(string name)
    {
        var document = await LoadVisualLayoutsDocumentAsync();
        document.VisualLayouts.Remove(name);
        await _documentManager.UpdateAsync(document);
    }

    public async Task UpdateVisualLayoutAsync(string name, VisualLayout visualLayout)
    {
        var document = await LoadVisualLayoutsDocumentAsync();
        document.VisualLayouts[name] = visualLayout;
        await _documentManager.UpdateAsync(document);
    }
}
