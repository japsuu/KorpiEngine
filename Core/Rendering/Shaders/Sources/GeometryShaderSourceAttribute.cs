using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.Core.Rendering.Shaders.Sources;

/// <summary>
/// Specifies the source of a geometry shader.
/// </summary>
public class GeometryShaderSourceAttribute : ShaderSourceAttribute
{
    /// <summary>
    /// Initializes a new instance of the GeometryShaderSourceAttribute.
    /// </summary>
    /// <param name="sourceLocation">Specifies the location of this shader.<br/>Usually the path to the shader relative to the shaders folder.<br/>Example: 'core/pass.vert'</param>
    public GeometryShaderSourceAttribute(string sourceLocation) : base(ShaderType.GeometryShader, sourceLocation)
    {
    }
}