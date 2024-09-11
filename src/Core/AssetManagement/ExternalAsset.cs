using KorpiEngine.Utils;

namespace KorpiEngine.AssetManagement;

/// <summary>
/// Represents a fully imported asset.
/// </summary>
internal class ExternalAsset
{
    public readonly UUID AssetID;
    public readonly FileInfo AssetPath;
    public readonly Asset Instance;


    public ExternalAsset(UUID assetID, FileInfo assetPath, Asset instance)
    {
        AssetID = assetID;
        AssetPath = assetPath;
        Instance = instance;
        
        Application.Logger.Info($"Imported {assetPath.Name} as {instance.Name}");
    }
}