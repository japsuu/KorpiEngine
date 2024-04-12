using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.Core.Rendering.Shaders;

public class ShaderSourceDescriptor
{
    /// <summary>
    /// Specifies the type of shader.
    /// </summary>
    public ShaderType Type { get; private set; }

    /// <summary>
    /// Specifies the location of this shader.<br/>
    /// Usually the path to the shader relative to the shaders' folder.<br/>
    /// Example: 'core/pass.vert'.
    /// </summary>
    public string SourceLocation { get; private set; }


    /// <summary>
    /// Initializes a new instance of the ShaderSourceDescriptor.
    /// </summary>
    /// <param name="type">Specifies the type of the shader.</param>
    /// <param name="sourceLocation">Specifies the location of this shader.<br/>Usually the path to the shader relative to the shaders folder.<br/>Example: 'core/pass.vert'</param>
    public ShaderSourceDescriptor(ShaderType type, string sourceLocation)
    {
        Type = type;
        SourceLocation = sourceLocation;
    }
}