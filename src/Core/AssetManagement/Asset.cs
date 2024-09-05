namespace KorpiEngine.AssetManagement;

/// <summary>
/// Represents a fully imported asset.
/// </summary>
internal class Asset
{
    public readonly Guid AssetID;
    public readonly FileInfo AssetPath;
    public readonly Resource Instance;


    public Asset(Guid assetID, FileInfo assetPath, Resource instance)
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