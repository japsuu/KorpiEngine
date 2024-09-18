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
    
    //TODO: Add LoadAsync variants.
    public AssetRef<T> LoadAsset<T>(string relativeAssetPath, ushort subID = 0, AssetImporter? customImporter = null) where T : Asset => new(InternalLoadAsset<T>(relativeAssetPath, subID, customImporter));
    public AssetRef<T> LoadAsset<T>(UUID assetID, ushort subID = 0) where T : Asset => new(InternalLoadAsset<T>(assetID, subID));


    /// <summary>
    /// Gets the importer for the specified file extension.
    /// </summary>
    /// <param name="fileExtension">The file extension to get an importer for, including the leading dot.</param>
    /// <returns>The importer for the specified file extension.</returns>
    public abstract AssetImporter GetImporter(string fileExtension);
    public abstract bool HasAsset(UUID assetID);
    protected internal abstract T InternalLoadAsset<T>(string relativeAssetPath, ushort subID = 0, AssetImporter? customImporter = null) where T : Asset;
    protected internal abstract T InternalLoadAsset<T>(UUID assetID, ushort subID = 0) where T : Asset;
    
    
    protected virtual void OnInitialize() { }
    protected virtual void OnShutdown() { }
}