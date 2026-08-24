using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.VisualLayouts.Models;

namespace OrchardCore.VisualLayouts.ViewModels;

public class VisualLayoutIndexViewModel
{
    public IList<VisualLayoutEntry> VisualLayouts { get; set; }
    public dynamic Pager { get; set; }
    public VisualLayoutIndexOptions Options { get; set; } = new VisualLayoutIndexOptions();
}

public class VisualLayoutEntry
{
    public string Name { get; set; }
    public VisualLayout VisualLayout { get; set; }
}

public class VisualLayoutIndexOptions
{
    public string Search { get; set; }
    public VisualLayoutsBulkAction BulkAction { get; set; }

    #region Lists to populate

    [BindNever]
    public List<SelectListItem> VisualLayoutsBulkAction { get; set; }

    #endregion Lists to populate
}

public enum VisualLayoutsBulkAction
{
    None,
    Remove,
}
