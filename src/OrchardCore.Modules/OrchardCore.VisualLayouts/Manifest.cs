using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Visual Layouts",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.VisualLayouts",
    Name = "Visual Layouts",
    Description = "The Visual Layouts module provides a way to visually design layout overrides for shapes from the Admin UI, without writing Liquid or code.",
    Category = "Development"
)]
