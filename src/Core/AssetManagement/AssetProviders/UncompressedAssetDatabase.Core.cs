using KorpiEngine.Utils;

namespace KorpiEngine.AssetManagement;

internal static partial class UncompressedAssetDatabase
{
    /// <summary>
    /// Relative path of an asset to its AssetID.
    /// The path is relative to the project root.
    /// </summary>
    private static readonly Dictionary<string, UUID> RelativePathToAssetId = new();
    private static readonly Dictionary<UUID, ImportedAsset> AssetIdToAsset = new();


    #region ASSET LOADING

    /// <summary>
    /// Loads an asset of the specified type from the specified file path.
    /// Use this when the AssetID is not known.
    /// </summary>
    /// <typeparam name="T">The type of the asset to load.</typeparam>
    /// <param name="assetPath">The file path of the asset to load.</param>
    /// <param name="subID">The sub-ID of the asset to load. 0 for the main asset, 1+ for sub-assets.</param>
    /// <param name="customImporter">An optional custom importer to use for importing the asset.
    /// If the asset has previously been imported with a different importer,
    /// the EXISTING asset instance will be returned.</param>
    /// <returns>The loaded asset, or null if the asset could not be loaded.</returns>
    public static T LoadAssetFile<T>(FileInfo assetPath, ushort subID, AssetImporter? customImporter = null) where T : Asset
    {
        string relativeAssetPath = ToRelativePath(assetPath);
        return LoadAssetFile<T>(relativeAssetPath, subID, customImporter);
    }

    /// <summary>
    /// Loads an asset of the specified type from the specified file path.
    /// Use this when the AssetID is not known.
    /// </summary>
    /// <typeparam name="T">The type of the asset to load.</typeparam>
    /// <param name="relativeAssetPath">The project-relative file path of the asset to load.</param>
    /// <param name="subID">The sub-ID of the asset to load. 0 for the main asset, 1+ for sub-assets.</param>
    /// <param name="customImporter">An optional custom importer to use for importing the asset.
    /// If the asset has previously been imported with a different importer,
    /// the EXISTING asset instance will be returned.</param>
    /// <returns>The loaded asset, or null if the asset could not be loaded.</returns>
    public static T LoadAssetFile<T>(string relativeAssetPath, ushort subID, AssetImporter? customImporter = null) where T : Asset
    {
        FileInfo fileInfo = GetFileInfoFromRelativePath(relativeAssetPath);
        
        // Check if the asset at the specified path has been loaded before
        if (TryGetAssetIDFromPath(fileInfo, out UUID assetID))
            return LoadAsset<T>(assetID, subID);

        // The path hasn't been accessed before, so we need to import the asset
        if (ImportFile(fileInfo, customImporter).GetAsset(subID) is not T asset)
            throw new AssetLoadException<T>(relativeAssetPath, $"The asset is not of expected type {typeof(T).Name}");
        
        if (asset.IsDestroyed)
            throw new AssetLoadException<T>(relativeAssetPath, "The asset has been destroyed.");
        
        return asset;
    }


    /// <summary>
    /// Loads an asset of the specified type with the specified known AssetID.
    /// Use this when the asset AssetID is known.
    /// </summary>
    /// <typeparam name="T">The type of the asset to load.</typeparam>
    /// <param name="assetID">The AssetID of the asset to load.</param>
    /// <param name="subID">The sub-ID of the asset to load. 0 for the main asset, 1+ for sub-assets.</param>
    /// <returns>The loaded asset, or null if the asset could not be loaded.</returns>
    public static T LoadAsset<T>(UUID assetID, ushort subID) where T : Asset
    {
        if (assetID == UUID.Empty)
            throw new ArgumentException("Asset UUID cannot be empty", nameof(assetID));

        ImportedAsset? importedAsset = GetAssetWithId(assetID);
        
        if (importedAsset == null)
            throw new AssetLoadException<T>(assetID, subID, "Asset with the specified ID not found.");

        Asset asset = importedAsset.GetAsset(subID);
        if (asset is not T typedAsset)
            throw new AssetLoadException<T>(assetID, subID, $"The asset is not of expected type {typeof(T).Name}, but {asset.GetType().Name}");
        
        if (typedAsset.IsDestroyed)
            throw new AssetLoadException<T>(assetID, subID, "The asset has been destroyed.");
        
        return typedAsset;
    }


    /// <summary>
    /// Gets an asset of the specified type with the specified known AssetID,
    /// without loading it if it is not already loaded.
    /// Never returns a destroyed asset.
    /// </summary>
    /// <typeparam name="T">The type of the asset to get.</typeparam>
    /// <param name="relativeAssetPath">The project-relative file path of the asset to load.</param>
    /// <param name="subID">The sub-ID of the asset to load. 0 for the main asset, 1+ for sub-assets.</param>
    /// <param name="foundAsset">The asset that was found or null if the asset could not be found.</param>
    /// <returns>True if the asset was found, false otherwise.</returns>
    public static bool TryGet<T>(string relativeAssetPath, ushort subID, out T? foundAsset) where T : Asset
    {
        foundAsset = null;
        
        FileInfo fileInfo = GetFileInfoFromRelativePath(relativeAssetPath);
        
        return TryGet(fileInfo, subID, out foundAsset);
    }


    /// <summary>
    /// Gets an asset of the specified type with the specified known AssetID,
    /// without loading it if it is not already loaded.
    /// Never returns a destroyed asset.
    /// </summary>
    /// <typeparam name="T">The type of the asset to get.</typeparam>
    /// <param name="path">The path of the asset to load.</param>
    /// <param name="subID">The sub-ID of the asset to load. 0 for the main asset, 1+ for sub-assets.</param>
    /// <param name="foundAsset">The asset that was found or null if the asset could not be found.</param>
    /// <returns>True if the asset was found, false otherwise.</returns>
    public static bool TryGet<T>(FileInfo path, ushort subID, out T? foundAsset) where T : Asset
    {
        foundAsset = null;

        // Check if the asset at the specified path has been loaded before
        return TryGetAssetIDFromPath(path, out UUID assetID) && TryGet(assetID, subID, out foundAsset);
    }


    /// <summary>
    /// Gets an asset of the specified type with the specified known AssetID,
    /// without loading it if it is not already loaded.
    /// Never returns a destroyed asset.
    /// </summary>
    /// <typeparam name="T">The type of the asset to get.</typeparam>
    /// <param name="assetID">The AssetID of the asset to get.</param>
    /// <param name="subID">The sub-ID of the asset to load. 0 for the main asset, 1+ for sub-assets.</param>
    /// <param name="foundAsset">The asset that was found or null if the asset could not be found.</param>
    /// <returns>True if the asset was found, false otherwise.</returns>
    public static bool TryGet<T>(UUID assetID, ushort subID, out T? foundAsset) where T : Asset
    {
        foundAsset = null;
        
        if (assetID == UUID.Empty)
            throw new ArgumentException("Asset UUID cannot be empty", nameof(assetID));

        ImportedAsset? importedAsset = GetAssetWithId(assetID);
        
        if (importedAsset == null)
            return false;

        Asset asset = importedAsset.GetAsset(subID);
        if (asset is not T typedAsset)
            throw new AssetLoadException<T>(assetID, subID, $"The asset is not of expected type {typeof(T).Name}, but {asset.GetType().Name}");
        
        if (typedAsset.IsDestroyed)
            return false;
        
        foundAsset = typedAsset;
        return true;
    }

    #endregion


    #region ASSET UNLOADING

    /// <summary>
    /// Unloads the asset with the specified AssetID, along with all its sub-assets.
    /// </summary>
    /// <param name="assetID">The AssetID of the asset to unload.</param>
    public static void UnloadAsset(UUID assetID)
    {
        if (!AssetIdToAsset.TryGetValue(assetID, out ImportedAsset? asset))
            return;

        if (asset.AssetID != assetID)
            throw new InvalidOperationException("Provided AssetID does not match the AssetID of the asset instance.");
        
        UnloadAndDestroyAsset(asset);
        Application.Logger.Info($"Asset with ID {assetID} has been unloaded.");
    }

    /// <summary>
    /// Unloads the asset with the specified AssetID, along with all its sub-assets.
    /// </summary>
    /// <param name="assetID">The AssetID of the asset that was destroyed.</param>
    public static void NotifyDestroy(UUID assetID)
    {
        if (!AssetIdToAsset.TryGetValue(assetID, out ImportedAsset? asset))
            return;

        if (asset.AssetID != assetID)
            throw new InvalidOperationException("Provided AssetID does not match the AssetID of the asset instance.");
        
        UnloadAsset(asset);
        Application.Logger.Info($"Asset with ID {assetID} has been unloaded.");
    }

#endregion


    private static void UnloadAndDestroyAsset(ImportedAsset asset)
    {
        UnloadAsset(asset);

        asset.Destroy();
    }


    private static void UnloadAsset(ImportedAsset asset)
    {
        AssetIdToAsset.Remove(asset.AssetID);
        RelativePathToAssetId.Remove(asset.RelativeAssetPath);
    }


    private static ImportedAsset ImportFile(FileInfo assetFile, AssetImporter? importer = null)
    {
        ArgumentNullException.ThrowIfNull(assetFile);
        
        string relativePath = ToRelativePath(assetFile);
        Application.Logger.Info($"Attempting to Import {relativePath}...");
        
        // Make sure the path exists
        if (!File.Exists(assetFile.FullName))
            throw new AssetImportException(relativePath, "File does not exist.");
        
        importer ??= GetImporter(assetFile.Extension);
        AssetImportContext context = new(relativePath, new UUID());

        try
        {
            importer.Import(context);
        }
        catch (Exception e)
        {
            throw new AssetImportException(relativePath, "The importer failed.", e);
        }
            
        if (context.MainAsset == null)
            throw new AssetImportException(relativePath, "The importer failed.");
            
        ImportedAsset importedAsset = new(context);

        RelativePathToAssetId[relativePath] = importedAsset.AssetID;
        AssetIdToAsset[importedAsset.AssetID] = importedAsset;
            
        Application.Logger.Info($"Successfully imported {relativePath}");
        return importedAsset;
    }


    /// <summary>
    /// Gets the importer for the specified file extension.
    /// </summary>
    /// <param name="fileExtension">The file extension to get an importer for, including the leading dot.</param>
    /// <returns>The importer for the specified file extension.</returns>
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
    /// Gets the asset with the specified AssetID.
    /// </summary>
    private static ImportedAsset? GetAssetWithId(UUID assetID) => AssetIdToAsset.GetValueOrDefault(assetID);
}