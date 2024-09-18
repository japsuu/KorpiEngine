namespace KorpiEngine.AssetManagement;

/// <summary>
/// Base class for all asset importers.
/// Implementations should be decorated with the <see cref="AssetImporterAttribute"/>.
/// </summary>
public abstract class AssetImporter
{
    /// <summary>
    /// Imports the asset with all their sub-assets into the given context.
    /// </summary>
    /// <param name="context">The context to import the asset(s) into.</param>
    public abstract void Import(AssetImportContext context);
}