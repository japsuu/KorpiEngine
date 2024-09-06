namespace KorpiEngine.Exceptions;

internal class AssetLoadException<T> : KorpiException
{
    public AssetLoadException(string assetPath, string info) : base($"Failed to load asset of type {typeof(T).Name} at path '{assetPath}': {info}") { }
    
    
    public AssetLoadException(string assetPath, Exception inner) : base($"Failed to load asset of type {typeof(T).Name} at path '{assetPath}'", inner) { }
}