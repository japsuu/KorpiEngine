using KorpiEngine.Utils;

namespace KorpiEngine.AssetManagement;

/// <summary>
/// Represents a fully imported asset.
/// </summary>
internal class ImportedAsset
{
    public readonly UUID AssetID;
    public readonly FileInfo AssetPath;
    public readonly Asset Instance;


    public ImportedAsset(UUID assetID, FileInfo assetPath, Asset instance)
    {
        AssetID = assetID;
        AssetPath = assetPath;
        Instance = instance;
    }
    
    
    public void Destroy()
    {
        Instance.DestroyImmediate();
    }
}