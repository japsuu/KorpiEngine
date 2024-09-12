using KorpiEngine.Utils;

namespace KorpiEngine.AssetManagement;

/// <summary>
/// Manages the loading and unloading of external assets.
/// </summary>
public static partial class AssetManager
{
    /// <summary>
    /// Relative path of an asset to its GUID.
    /// The path is relative to the project root.
    /// </summary>
    private static readonly Dictionary<string, UUID> RelativePathToGuid = new();
    private static readonly Dictionary<UUID, ExternalAsset> GuidToAsset = new();


    #region ASSET LOADING

    /// <summary>
    /// Loads an asset of the specified type from the specified file path.
    /// Use this when the asset GUID is not known.
    /// </summary>
    /// <typeparam name="T">The type of the asset to load.</typeparam>
    /// <param name="relativeAssetPath">The project-relative file path of the asset to load.</param>
    /// <param name="customImporter">An optional custom importer to use for importing the asset.
    /// If the asset has previously been imported with a different importer,
    /// the EXISTING asset instance will be returned.</param>
    /// <returns>The loaded asset, or null if the asset could not be loaded.</returns>
    public static T LoadAssetFile<T>(string relativeAssetPath, AssetImporter? customImporter = null) where T : Asset
    {
        FileInfo fileInfo = GetFileInfoFromRelativePath(relativeAssetPath);
        
        // Check if the asset at the specified path has been loaded before
        if (TryGetGuidFromPath(fileInfo, out UUID guid))
            return LoadAsset<T>(guid);

        // The path hasn't been accessed before, so we need to import the asset
        if (ImportFile(fileInfo, customImporter).Instance is not T asset)
            throw new AssetLoadException<T>(relativeAssetPath, $"The asset is not of expected type {typeof(T).Name}");
        
        return asset;
    }


    /// <summary>
    /// Loads an asset of the specified type with the specified known GUID.
    /// Use this when the asset GUID is known.
    /// </summary>
    /// <typeparam name="T">The type of the asset to load.</typeparam>
    /// <param name="assetGuid">The GUID of the asset to load.</param>
    /// <returns>The loaded asset, or null if the asset could not be loaded.</returns>
    public static T LoadAsset<T>(UUID assetGuid) where T : Asset
    {
        if (assetGuid == UUID.Empty)
            throw new ArgumentException("Asset UUID cannot be empty", nameof(assetGuid));

        ExternalAsset? externalAsset = GetAssetWithId(assetGuid);
        if (externalAsset?.Instance is not T asset)
            throw new AssetLoadException<T>(assetGuid.ToString(), "Asset not found");
        
        return asset;
    }


    /// <summary>
    /// Gets an asset of the specified type from the specified file path.
    /// Use this when the asset GUID is not known, and if you temporarily need the asset without incrementing its reference count.
    /// </summary>
    /// <typeparam name="T">The type of the asset to get.</typeparam>
    /// <param name="relativeAssetPath">The project-relative file path of the asset to get.</param>
    /// <returns>The loaded asset, or null if the asset is not loaded.</returns>
    public static T? GetAsset<T>(string relativeAssetPath) where T : Asset
    {
        FileInfo fileInfo = GetFileInfoFromRelativePath(relativeAssetPath);
        
        if (!TryGetGuidFromPath(fileInfo, out UUID guid))
            return null;

        return GetAsset<T>(guid);
    }


    /// <summary>
    /// Gets an asset of the specified type with the specified known GUID.
    /// Use this when the asset GUID is known, and if you temporarily need the asset without incrementing its reference count.
    /// </summary>
    /// <typeparam name="T">The type of the asset to get.</typeparam>
    /// <param name="assetGuid">The GUID of the asset to get.</param>
    /// <returns>The loaded asset, or null if the asset is not loaded.</returns>
    public static T? GetAsset<T>(UUID assetGuid) where T : Asset
    {
        if (assetGuid == UUID.Empty)
            throw new ArgumentException("Asset UUID cannot be empty", nameof(assetGuid));

        return GetAssetWithId(assetGuid)?.Instance as T;
    }

    #endregion


    #region ASSET UNLOADING

    internal static void NotifyUnloadAsset(Asset asset)
    {
        ArgumentNullException.ThrowIfNull(asset);
        
        if (!asset.IsExternal)
            throw new InvalidOperationException("Cannot unload an asset that is not external. Did you mean to call AssetInstance.Release()?");
        
        NotifyUnloadAsset(asset.ExternalAssetID);
    }

    internal static void NotifyUnloadAsset(UUID guid)
    {
        if (!GuidToAsset.TryGetValue(guid, out ExternalAsset? asset))
            return;

        string relativePath = ToRelativePath(asset.AssetPath);

        GuidToAsset.Remove(guid);
        RelativePathToGuid.Remove(relativePath);
        
        Application.Logger.Info($"Unloaded {asset.Instance} ({relativePath})");
    }

    #endregion


    private static ExternalAsset ImportFile(FileInfo assetFile, AssetImporter? importer = null)
    {
        ArgumentNullException.ThrowIfNull(assetFile);
        
        string relativePath = ToRelativePath(assetFile);
        
        // Make sure the path exists
        if (!File.Exists(assetFile.FullName))
            throw new AssetImportException(relativePath, "File does not exist.");
        
        importer ??= GetImporter(assetFile.Extension);
        Asset? instance;

        try
        {
            instance = importer.Import(assetFile);
        }
        catch (Exception e)
        {
            throw new AssetImportException(relativePath, "The importer failed.", e);
        }
            
        if (instance == null)
            throw new AssetImportException(relativePath, "The importer failed.");
            
        // Generate a new GUID for the asset
        UUID assetID = new();
        instance.ExternalAssetID = assetID;
        instance.IsExternal = true;
        ExternalAsset externalAsset = new(assetID, assetFile, instance);

        RelativePathToGuid[relativePath] = assetID;
        GuidToAsset[assetID] = externalAsset;
            
        Application.Logger.Info($"Successfully imported {relativePath} as {instance}");
        return externalAsset;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileExtension">The file extension to get an importer for, including the leading dot.</param>
    /// <returns></returns>
    /// <exception cref="AssetImportException"></exception>
    public static AssetImporter GetImporter(string fileExtension)
    {
        // Ensure the file extension is valid
        if (string.IsNullOrEmpty(fileExtension) || fileExtension[0] != '.')
            throw new ArgumentException("File extension must not be null or empty, and must start with a dot.", nameof(fileExtension));
        
        // Make sure the file extension of the asset file is supported
        if (!AssetImporterAttribute.SupportsExtension(fileExtension))
        {
            string supportedExtensions = AssetImporterAttribute.GetSupportedExtensions();
            throw new InvalidOperationException($"Unsupported file extension. Supported extensions are: '{supportedExtensions}'.");
        }

        // Get the importer for the asset
        Type? importerType = AssetImporterAttribute.GetImporter(fileExtension);
        if (importerType is null)
            throw new InvalidOperationException($"No importer found for asset with extension {fileExtension}");
        
        AssetImporter? importer = (AssetImporter?)Activator.CreateInstance(importerType);
        if (importer is null)
            throw new InvalidOperationException($"Could not create importer for asset with extension {fileExtension}");
        
        return importer;
    }


    /// <summary>
    /// Gets the asset with the specified GUID.
    /// </summary>
    private static ExternalAsset? GetAssetWithId(UUID assetGuid) => GuidToAsset.GetValueOrDefault(assetGuid);
}