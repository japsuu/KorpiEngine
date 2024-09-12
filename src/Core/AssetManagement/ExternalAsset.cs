using KorpiEngine.Utils;

namespace KorpiEngine.AssetManagement;

/// <summary>
/// Represents a fully imported asset.
/// </summary>
internal class ExternalAsset(UUID assetID, FileInfo assetPath, Asset instance)
{
    public readonly UUID AssetID = assetID;
    public readonly FileInfo AssetPath = assetPath;
    public readonly Asset Instance = instance;
}