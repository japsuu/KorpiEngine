namespace KorpiEngine.Core.API.Rendering.Shaders;

/// <summary>
/// Describes a shader source file.
/// </summary>
public class ShaderSourceDescriptor
{
    /// <summary>
    /// Specifies the type of shader.
    /// </summary>
    public ShaderType Type { get; }

    /// <summary>
    /// Specifies the source code for this shader.
    /// </summary>
    public string Source { get; }


    /// <summary>
    /// Initializes a new instance of the ShaderSourceDescriptor.
    /// </summary>
    /// <param name="type">Specifies the type of the shader.</param>
    /// <param name="source">The source code for this shader.</param>
    public ShaderSourceDescriptor(ShaderType type, string source)
    {
        Type = type;
        Source = source;
    }
}