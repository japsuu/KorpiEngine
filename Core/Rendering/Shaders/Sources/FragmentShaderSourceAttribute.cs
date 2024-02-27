using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.Core.Rendering.Shaders.Sources;

/// <summary>
/// Specifies the source of a fragment shader.
/// </summary>
public class FragmentShaderSourceAttribute : ShaderSourceAttribute
{
    /// <summary>
    /// Initializes a new instance of the FragmentShaderSourceAttribute.
    /// </summary>
    /// <param name="sourceLocation">Specifies the location of this shader.<br/>Usually the path to the shader relative to the shaders folder.<br/>Example: 'core/pass.vert'</param>
    public FragmentShaderSourceAttribute(string sourceLocation) : base(ShaderType.FragmentShader, sourceLocation)
    {
    }
}