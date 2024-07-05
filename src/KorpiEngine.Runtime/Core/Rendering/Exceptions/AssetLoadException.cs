namespace KorpiEngine.Core.Rendering.Exceptions;

public class AssetLoadException<T>(string assetPath) : Exception($"Failed to load asset of type {typeof(T).Name} at path '{assetPath}'");