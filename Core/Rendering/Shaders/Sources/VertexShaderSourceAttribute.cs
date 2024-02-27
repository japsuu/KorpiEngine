using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.Core.Rendering.Shaders.Sources;

/// <summary>
/// Specifies the source of a vertex shader.
/// </summary>
public class VertexShaderSourceAttribute : ShaderSourceAttribute
{
    /// <summary>
    /// Initializes a new instance of the VertexShaderSourceAttribute.
    /// </summary>
    /// <param name="sourcePath">Specifies the location of this shader.<br/>Usually the path to the shader relative to the shaders folder.<br/>Example: 'core/pass.vert'</param>
    public VertexShaderSourceAttribute(string sourcePath) : base(ShaderType.VertexShader, sourcePath)
    {
    }
}