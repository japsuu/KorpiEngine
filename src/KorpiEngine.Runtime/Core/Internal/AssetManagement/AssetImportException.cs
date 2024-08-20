namespace KorpiEngine.Core.Internal.AssetManagement;

public class AssetImportException<T> : Exception
{
    public AssetImportException(string message) : base($"Failed to import asset of type {typeof(T).Name}: {message}")
    {
    }
    
    
    public AssetImportException(string message, Exception ex) : base($"Failed to import asset of type {typeof(T).Name}: {message}", ex)
    {
    }
}

public class AssetImportException : Exception
{
    public AssetImportException(string path, string message) : base($"Failed to import asset at '{path}': {message}")
    {
    }
    
    
    public AssetImportException(string path, string message, Exception ex) : base($"Failed to import asset at '{path}': {message}", ex)
    {
    }
}