using KorpiEngine.Utils;

namespace KorpiEngine.AssetManagement;

internal class AssetLoadException<T> : KorpiException
{
    public AssetLoadException(string assetPath, string info) : base($"Failed to load asset of type {typeof(T).Name} at path '{assetPath}': {info}") { }
    
    
    public AssetLoadException(UUID assetID, ushort subID, string info) : base($"Failed to load sub-asset '{subID}' of type {typeof(T).Name} from assetID '{assetID}': {info}") { }
    
    
    public AssetLoadException(string assetPath, Exception inner) : base($"Failed to load asset of type {typeof(T).Name} at path '{assetPath}'", inner) { }
}