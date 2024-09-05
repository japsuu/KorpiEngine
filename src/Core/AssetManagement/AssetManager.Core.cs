using KorpiEngine.Exceptions;

namespace KorpiEngine.AssetManagement;

public static partial class AssetManager
{
    /// <summary>
    /// Relative path of an asset to its GUID.
    /// The path is relative to the project root.
    /// </summary>
    private static readonly Dictionary<string, Guid> RelativePathToGuid = new();
    private static readonly Dictionary<Guid, Asset> GuidToAsset = new();


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
    public static T LoadAssetFile<T>(string relativeAssetPath, AssetImporter? customImporter = null) where T : Resource
    {
        FileInfo fileInfo = GetFileInfoFromRelativePath(relativeAssetPath);
        
        // Check if the asset at the specified path has been loaded before
        if (TryGetGuidFromPath(fileInfo, out Guid guid))
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
    public static T LoadAsset<T>(Guid assetGuid) where T : Resource
    {
        if (assetGuid == Guid.Empty)
            throw new ArgumentException("Asset Guid cannot be empty", nameof(assetGuid));

        if (GetAssetWithId(assetGuid)?.Instance is not T asset)
            throw new AssetLoadException<T>(assetGuid.ToString(), "Asset not found");
        
        return asset;
    }

    #endregion


    #region ASSET UNLOADING

    public static void UnloadAsset(Guid guid)
    {
        if (!GuidToAsset.TryGetValue(guid, out Asset? asset))
            return;

        string relativePath = ToRelativePath(asset.AssetPath);
        
        asset.Destroy();
        GuidToAsset.Remove(guid);
        RelativePathToGuid.Remove(relativePath);
    }

    #endregion


    private static Asset ImportFile(FileInfo assetFile, AssetImporter? importer = null)
    {
        ArgumentNullException.ThrowIfNull(assetFile);
        
        string relativePath = ToRelativePath(assetFile);
        Application.Logger.Info($"Attempting to Import {relativePath}...");
        
        // Make sure the path exists
        if (!File.Exists(assetFile.FullName))
            throw new AssetImportException(relativePath, "File does not exist.");
        
        importer ??= GetImporter(assetFile.Extension);
        Resource? instance = importer.Import(assetFile);
            
        if (instance == null)
            throw new AssetImportException(relativePath, "The importer failed.");
            
        // Generate a new GUID for the asset
        Guid assetID = Guid.NewGuid();
        instance.AssetID = assetID;
        Asset asset = new(assetID, assetFile, instance);

        RelativePathToGuid[relativePath] = assetID;
        GuidToAsset[assetID] = asset;
            
        Application.Logger.Info($"Successfully imported {relativePath}");
        return asset;
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
    private static Asset? GetAssetWithId(Guid assetGuid) => GuidToAsset.GetValueOrDefault(assetGuid);
}