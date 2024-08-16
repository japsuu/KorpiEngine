namespace KorpiEngine.Core.Internal.AssetManagement;

public class AssetImportException<T> : Exception
{
    public AssetImportException(string message) : base($"Failed to load asset of type {typeof(T).Name}: {message}")
    {
    }
    
    
    public AssetImportException(string message, Exception ex) : base($"Failed to load asset of type {typeof(T).Name}: {message}", ex)
    {
    }
}