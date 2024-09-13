using KorpiEngine.Utils;

namespace KorpiEngine.AssetManagement;

public static partial class AssetManager
{
    /// <summary>
    /// Converts a file path to a relative path within the project.
    /// </summary>
    /// <param name="file">The file to convert to a relative path.</param>
    /// <returns>The relative path of the file within the project.</returns>
    public static string ToRelativePath(FileInfo file) => Path.GetRelativePath(Application.Directory, file.FullName);


    /// <summary>
    /// Converts a relative path within the project to a full file path.
    /// </summary>
    /// <param name="relativePath">The relative path to convert to a full file path.</param>
    /// <returns>The full file path of the relative path.</returns>
    public static FileInfo GetFileInfoFromRelativePath(string relativePath) => new(Path.Combine(Application.Directory, relativePath));


    /// <summary>
    /// Gets the AssetID of a file from its relative path.
    /// </summary>
    /// <param name="relativePath">The relative path of the file.</param>
    /// <returns>The AssetID of the file.</returns>
    public static UUID AssetIDFromRelativePath(string relativePath)
    {
        FileInfo path = GetFileInfoFromRelativePath(relativePath);
        return TryGetAssetIDFromPath(path, out UUID assetID) ? assetID : UUID.Empty;
    }


    /// <summary>
    /// Checks if the AssetDatabase contains a file with the specified AssetID.
    /// </summary>
    /// <param name="assetID">The AssetID of the file.</param>
    /// <returns>True if the file exists in the AssetDatabase, false otherwise.</returns>
    public static bool Contains(UUID assetID) => AssetIdToAsset.ContainsKey(assetID);


    /// <summary>
    /// Tries to get the AssetID of a file.
    /// </summary>
    /// <param name="file">The file to get the AssetID for.</param>
    /// <param name="assetID">The AssetID of the file.</param>
    /// <returns>True if the AssetID was found, false otherwise.</returns>
    public static bool TryGetAssetIDFromPath(FileInfo file, out UUID assetID)
    {
        assetID = UUID.Empty;
        if (!File.Exists(file.FullName))
            return false;
        
        string relativePath = ToRelativePath(file);
        return RelativePathToAssetId.TryGetValue(relativePath, out assetID);
    }
}