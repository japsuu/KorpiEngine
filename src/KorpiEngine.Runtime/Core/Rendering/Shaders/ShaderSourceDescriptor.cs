using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.Core.Rendering.Shaders;

public class ShaderSourceDescriptor
{
    /// <summary>
    /// Specifies the type of shader.
    /// </summary>
    public ShaderType Type { get; private set; }

    /// <summary>
    /// Specifies the raw source code for this shader.
    /// </summary>
    public string SourceRaw { get; private set; }  //NOTE: This could be a TextAsset?


    /// <summary>
    /// Initializes a new instance of the ShaderSourceDescriptor.
    /// </summary>
    /// <param name="type">Specifies the type of the shader.</param>
    /// <param name="sourceRaw">The raw source code for this shader.</param>
    public ShaderSourceDescriptor(ShaderType type, string sourceRaw)
    {
        Type = type;
        SourceRaw = sourceRaw;
    }
}