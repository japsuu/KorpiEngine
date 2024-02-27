using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.Core.Rendering.Shaders.Sources;

/// <summary>
/// Specifies a source file which contains a single shader of predefined type.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ShaderSourceAttribute : Attribute
{
    /// <summary>
    /// Specifies the type of shader.
    /// </summary>
    public ShaderType Type { get; private set; }

    /// <summary>
    /// Specifies the location of this shader.<br/>
    /// Usually the path to the shader relative to the shaders folder.<br/>
    /// Example: 'core/pass.vert'.
    /// </summary>
    public string SourceLocation { get; private set; }


    /// <summary>
    /// Initializes a new instance of the ShaderSourceAttribute.
    /// </summary>
    /// <param name="type">Specifies the type of the shader.</param>
    /// <param name="sourceLocation">Specifies the location of this shader.<br/>Usually the path to the shader relative to the shaders folder.<br/>Example: 'core/pass.vert'</param>
    protected ShaderSourceAttribute(ShaderType type, string sourceLocation)
    {
        Type = type;
        SourceLocation = sourceLocation;
    }


    /// <summary>
    /// Retrieves all shader sources from attributes tagged to the given shaderProgram type.
    /// </summary>
    /// <param name="programType">Specifies the type of the shaderProgram of which the shader sources are to be found.</param>
    /// <returns>A mapping of ShaderType and source path.</returns>
    public static List<ShaderSourceAttribute> GetShaderSources(Type programType)
    {
        return programType.GetCustomAttributes(typeof(ShaderSourceAttribute), true).Cast<ShaderSourceAttribute>().ToList();
    }
}