using System.Reflection;
using KorpiEngine.Utils;

namespace KorpiEngine.AssetManagement;

/// <summary>
/// Attach this attribute to a class that inherits from AssetImporter to allow the AssetDatabase to import files with the specified extensions.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AssetImporterAttribute : Attribute
{
    private static readonly Dictionary<string, Type> ImportersByExtension = new(StringComparer.OrdinalIgnoreCase);
    private string[] SupportedFileExtensions { get; set; }


    public AssetImporterAttribute(params string[] supportedFileExtensions)
    {
        SupportedFileExtensions = supportedFileExtensions;
    }


    /// <param name="extension">Extension type, including the '.' so '.png'</param>
    /// <returns>The importer type for that Extension</returns>
    public static Type? GetImporter(string extension) => ImportersByExtension.GetValueOrDefault(extension);
    public static bool SupportsExtension(string extension) => ImportersByExtension.ContainsKey(extension);
    public static string GetSupportedExtensions() => string.Join(", ", ImportersByExtension.Keys);


    public static void GenerateLookUp()
    {
        Application.Logger.Info("Generating Asset Importer Lookup Table");
        ImportersByExtension.Clear();
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (Type type in assembly.GetTypes())
            {
                AssetImporterAttribute? attribute = type.GetCustomAttribute<AssetImporterAttribute>();
                if (attribute == null)
                    continue;

                RegisterImporterType(attribute, type);
            }
        }
    }


    private static void RegisterImporterType(AssetImporterAttribute attribute, Type type)
    {
        foreach (string extensionRaw in attribute.SupportedFileExtensions)
        {
            string extension = extensionRaw.ToLower();

            // Make sure the Extension is formatted correctly.
            if (extension[0] != '.')
                extension = '.' + extension;
            if (extension.Count(x => x == '.') > 1)
                throw new InvalidOperationException($"Extension {extension} is formatted incorrectly on importer: {type.Name}");

            if (ImportersByExtension.TryGetValue(extension, out Type? oldType))
                Application.Logger.Warn($"Importer extension '{extension}' already in use by: {oldType.Name}, being overwritten by: {type.Name}");
            ImportersByExtension[extension] = type;
        }
    }


    public static void ClearLookUp()
    {
        ImportersByExtension.Clear();
    }
}