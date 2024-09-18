using KorpiEngine.Utils;

namespace KorpiEngine.AssetManagement;

public interface IAssetProvider
{
    public bool HasAsset(UUID assetID);
    public AssetRef<T> LoadAsset<T>(string relativeAssetPath, ushort subID = 0, AssetImporter? customImporter = null) where T : Asset;
    public AssetRef<T> LoadAsset<T>(UUID assetID, ushort subID = 0) where T : Asset;
}