namespace OrchardCore.VisualLayouts.ViewModels;

public class VisualLayoutViewModel
{
    public string Name { get; set; }
    public string Description { get; set; }

    /// <summary>
    /// The JSON representation of the layout tree, edited by the visual designer.
    /// </summary>
    public string State { get; set; }
}
