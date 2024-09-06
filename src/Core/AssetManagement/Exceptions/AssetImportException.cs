using KorpiEngine.Utils;

namespace KorpiEngine.AssetManagement;

internal class AssetImportException<T> : KorpiException
{
    public AssetImportException(string message) : base($"Failed to import asset of type {typeof(T).Name}: {message}")
    {
    }
    
    
    public AssetImportException(string message, Exception ex) : base($"Failed to import asset of type {typeof(T).Name}: {message}", ex)
    {
    }
}

internal class AssetImportException : KorpiException
{
    public AssetImportException(string path, string message) : base($"Failed to import asset at '{path}': {message}")
    {
    }
    
    
    public AssetImportException(string path, string message, Exception ex) : base($"Failed to import asset at '{path}': {message}", ex)
    {
    }
}