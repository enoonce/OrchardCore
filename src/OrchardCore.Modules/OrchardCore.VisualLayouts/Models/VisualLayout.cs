namespace OrchardCore.VisualLayouts.Models;

/// <summary>
/// Represents a visual layout, i.e. a structured tree of rows, columns and blocks
/// that can be rendered in place of a shape without writing Liquid or code.
/// </summary>
public class VisualLayout
{
    /// <summary>
    /// Gets or sets the description of the layout.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets the rows composing the layout.
    /// </summary>
    public IList<LayoutRow> Rows { get; set; } = [];
}

/// <summary>
/// Represents a row of a visual layout.
/// </summary>
public class LayoutRow
{
    /// <summary>
    /// Gets or sets the CSS classes applied to the row container.
    /// </summary>
    public string CssClasses { get; set; }

    /// <summary>
    /// Gets the columns composing the row.
    /// </summary>
    public IList<LayoutColumn> Columns { get; set; } = [];
}

/// <summary>
/// Represents a column of a visual layout row.
/// </summary>
public class LayoutColumn
{
    /// <summary>
    /// Gets or sets the width of the column on large screens, from 1 to 12 grid units.
    /// </summary>
    public int WidthLg { get; set; } = 12;

    /// <summary>
    /// Gets or sets the CSS classes applied to the column container.
    /// </summary>
    public string CssClasses { get; set; }

    /// <summary>
    /// Gets the blocks composing the column.
    /// </summary>
    public IList<LayoutBlock> Blocks { get; set; } = [];
}

/// <summary>
/// Represents a block of content inside a visual layout column.
/// </summary>
public class LayoutBlock
{
    /// <summary>
    /// Gets or sets the type of the block, e.g. <c>Html</c> or <c>Text</c>.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Gets the properties of the block, interpreted by the block renderer matching <see cref="Type"/>.
    /// </summary>
    public Dictionary<string, string> Properties { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
