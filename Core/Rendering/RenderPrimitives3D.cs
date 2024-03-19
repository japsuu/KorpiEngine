namespace KorpiEngine.Core.Rendering;

internal static class RenderPrimitives3D
{
    public static readonly float[] QuadVerticesTextured =
    {
         // Position         // Texture coordinates
         0.5f,  0.5f, 0.0f, 1.0f, 1.0f,     // Top right
         0.5f, -0.5f, 0.0f, 1.0f, 0.0f,     // Bottom right
        -0.5f, -0.5f, 0.0f, 0.0f, 0.0f,     // Bottom left
        -0.5f,  0.5f, 0.0f, 0.0f, 1.0f      // Top left
    };
    
    public static readonly float[] QuadVertices =
    {
         // Position
         0.5f,  0.5f, 0.0f,     // Top right
         0.5f, -0.5f, 0.0f,     // Bottom right
        -0.5f, -0.5f, 0.0f,     // Bottom left
        -0.5f,  0.5f, 0.0f      // Top left
    };
}