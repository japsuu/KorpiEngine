using KorpiEngine.Utils;

namespace KorpiEngine.AssetManagement;

public class UncompressedAssetProvider : AssetProvider
{
    protected override void OnInitialize()
    {
        AssetImporterAttribute.GenerateLookUp();
    }


    protected override void OnShutdown()
    {
        AssetImporterAttribute.ClearLookUp();
    }


    public override AssetImporter GetImporter(string fileExtension) => UncompressedAssetDatabase.GetImporter(fileExtension);


    /// <summary>
    /// Checks if an asset with the specified AssetID exists in the AssetDatabase.
    /// </summary>
    /// <param name="assetID">The AssetID of the file.</param>
    /// <returns>True if the file exists in the AssetDatabase, false otherwise.</returns>
    public override bool HasAsset(UUID assetID) => UncompressedAssetDatabase.Contains(assetID);


    /// <summary>
    /// Loads an asset of the specified type from the specified file path.
    /// Use this when the AssetID is not known.
    /// </summary>
    /// <typeparam name="T">The type of the asset to load.</typeparam>
    /// <param name="relativeAssetPath">The project-relative file path of the asset to load.</param>
    /// <param name="subID">The sub-ID of the asset to load. 0 for the main asset, 1+ for sub-assets.</param>
    /// <param name="customImporter">An optional custom importer to use for importing the asset for the first time.
    /// If the asset has previously been imported with a different importer,
    /// the EXISTING asset instance will be returned.</param>
    /// <returns>The loaded asset, or null if the asset could not be loaded.</returns>
    protected internal override T InternalLoadAsset<T>(string relativeAssetPath, ushort subID = 0, AssetImporter? customImporter = null)
    {
        return UncompressedAssetDatabase.LoadAssetFile<T>(relativeAssetPath, subID, customImporter);
    }


    /// <summary>
    /// Loads an asset of the specified type with the specified known AssetID.
    /// Use this when the asset AssetID is known.
    /// </summary>
    /// <typeparam name="T">The type of the asset to load.</typeparam>
    /// <param name="assetID">The AssetID of the asset to load.</param>
    /// <param name="subID">The sub-ID of the asset to load. 0 for the main asset, 1+ for sub-assets.</param>
    /// <returns>The loaded asset, or null if the asset could not be loaded.</returns>
    protected internal override T InternalLoadAsset<T>(UUID assetID, ushort subID = 0)
    {
        return UncompressedAssetDatabase.LoadAsset<T>(assetID, subID);
    }
}