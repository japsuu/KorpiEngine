using KorpiEngine.Utils;

namespace KorpiEngine.AssetManagement;

/// <summary>
/// Represents a fully imported asset.
/// </summary>
internal class ExternalAsset
{
    public readonly UUID AssetID;
    public readonly FileInfo AssetPath;
    public readonly AssetInstance Instance;

    public int ReferenceCount { get; private set; }


    public ExternalAsset(UUID assetID, FileInfo assetPath, AssetInstance instance)
    {
        AssetID = assetID;
        AssetPath = assetPath;
        Instance = instance;
        
        ReferenceOnce();
        
        Application.Logger.Info($"Imported {assetPath.Name} as {instance.Name}");
    }
    
    
    public void ReferenceOnce()
    {
        ReferenceCount++;
    }


    public bool TryRelease()
    {
        ReferenceCount--;

        return ReferenceCount == 0;
    }
}