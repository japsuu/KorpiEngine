namespace KorpiEngine.Core.API.AssetManagement;

/// <summary>
/// Represents a fully imported asset.
/// </summary>
public class Asset
{
    public readonly Guid AssetID;
    public readonly FileInfo AssetPath;
    public readonly EngineObject? Instance;


    public Asset(Guid assetID, FileInfo assetPath, EngineObject? instance)
    {
        AssetID = assetID;
        AssetPath = assetPath;
        Instance = instance;
    }
    
    
    public void Destroy()
    {
        Instance?.DestroyImmediate();
    }
}