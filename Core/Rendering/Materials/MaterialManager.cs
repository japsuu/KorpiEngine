namespace KorpiEngine.Core.Rendering.Materials;

internal static class MaterialManager
{
    public static MissingMaterial3D MissingMaterial3D { get; private set; } = new();
}