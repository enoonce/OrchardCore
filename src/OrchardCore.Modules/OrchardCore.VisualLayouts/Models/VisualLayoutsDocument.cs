using OrchardCore.Data.Documents;

namespace OrchardCore.VisualLayouts.Models;

/// <summary>
/// Represents a document that stores all the visual layouts of a tenant.
/// </summary>
public class VisualLayoutsDocument : Document
{
    public Dictionary<string, VisualLayout> VisualLayouts { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
