using KorpiEngine.Utils;

namespace KorpiEngine.AssetManagement;

public abstract class AssetProvider
{
    internal void Initialize()
    {
        OnInitialize();
    }


    internal void Shutdown()
    {
        OnShutdown();
    }
    
    public AssetRef<T> LoadAsset<T>(string relativeAssetPath, ushort subID = 0, AssetImporter? customImporter = null) where T : Asset => new(InternalLoadAsset<T>(relativeAssetPath, subID, customImporter));
    public AssetRef<T> LoadAsset<T>(UUID assetID, ushort subID = 0) where T : Asset => new(InternalLoadAsset<T>(assetID, subID));

    public abstract bool HasAsset(UUID assetID);
    protected internal abstract T InternalLoadAsset<T>(string relativeAssetPath, ushort subID = 0, AssetImporter? customImporter = null) where T : Asset;
    protected internal abstract T InternalLoadAsset<T>(UUID assetID, ushort subID = 0) where T : Asset;
    
    
    protected virtual void OnInitialize() { }
    protected virtual void OnShutdown() { }
}