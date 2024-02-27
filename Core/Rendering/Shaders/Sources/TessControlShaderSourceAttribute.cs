using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.Core.Rendering.Shaders.Sources;

/// <summary>
/// Specifies the source of a vertex shader.
/// </summary>
public class TessControlShaderSourceAttribute : ShaderSourceAttribute
{
    /// <summary>
    /// Initializes a new instance of the TessControlShaderSourceAttribute.
    /// </summary>
    /// <param name="sourcePath">Specifies the location of this shader.<br/>Usually the path to the shader relative to the shaders folder.<br/>Example: 'core/pass.vert'</param>
    public TessControlShaderSourceAttribute(string sourcePath) : base(ShaderType.TessControlShader, sourcePath)
    {
    }
}