using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace OrchardCore.VisualLayouts;

public sealed class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
{
    private static readonly ResourceManifest _manifest;

    static ResourceManagementOptionsConfiguration()
    {
        _manifest = new ResourceManifest();

        _manifest
            .DefineScript("visuallayout-editor")
            .SetUrl("~/OrchardCore.VisualLayouts/Scripts/visuallayout.editor.min.js", "~/OrchardCore.VisualLayouts/Scripts/visuallayout.editor.js")
            .SetVersion("1.0.0");

        _manifest
            .DefineScript("visuallayout-preview-edit")
            .SetUrl("~/OrchardCore.VisualLayouts/Scripts/visuallayoutpreview.edit.min.js", "~/OrchardCore.VisualLayouts/Scripts/visuallayoutpreview.edit.js")
            .SetVersion("1.0.0");
    }

    public void Configure(ResourceManagementOptions options)
    {
        options.ResourceManifests.Add(_manifest);
    }
}
