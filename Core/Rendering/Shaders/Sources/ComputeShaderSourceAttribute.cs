using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.Core.Rendering.Shaders.Sources;

/// <summary>
/// Specifies the source of a compute shader.
/// </summary>
public class ComputeShaderSourceAttribute : ShaderSourceAttribute
{
    /// <summary>
    /// Initializes a new instance of the ComputeShaderSourceAttribute.
    /// </summary>
    /// <param name="sourceLocation">Specifies the location of this shader.<br/>Usually the path to the shader relative to the shaders folder.<br/>Example: 'core/pass.vert'</param>
    public ComputeShaderSourceAttribute(string sourceLocation) : base(ShaderType.ComputeShader, sourceLocation)
    {
    }
}