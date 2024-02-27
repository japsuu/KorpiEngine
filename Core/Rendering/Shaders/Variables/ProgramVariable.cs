using System.Reflection;

namespace KorpiEngine.Core.Rendering.Shaders.Variables;

/// <summary>
/// Represents a shader variable identified by its name and the corresponding shaderProgram handle.
/// </summary>
public abstract class ProgramVariable
{
    /// <summary>
    /// The handle of the shaderProgram to which this variable relates.
    /// </summary>
    protected ShaderPrograms.ShaderProgram ShaderProgram { get; private set; } = null!;

    /// <summary>
    /// The handle of the shaderProgram to which this variable relates.
    /// </summary>
    public int ProgramHandle => ShaderProgram.Handle;

    /// <summary>
    /// The name of this shader variable.
    /// </summary>
    public string Name { get; protected set; } = null!;

    /// <summary>
    /// Specifies whether this variable is active.<br/>
    /// Unused variables are generally removed by OpenGL and cause them to be inactive.
    /// </summary>
    public bool Active { get; protected set; }


    /// <summary>
    /// Initializes this instance using the given ShaderProgram and PropertyInfo.
    /// </summary>
    internal virtual void Initialize(ShaderPrograms.ShaderProgram shaderProgram, PropertyInfo property)
    {
        ShaderProgram = shaderProgram;
        Name = property.Name;
    }


    /// <summary>
    /// When overridden in a derived class, handles initialization which must occur after the shaderProgram object is linked.
    /// </summary>
    internal virtual void OnLink()
    {
    }


    public override string ToString()
    {
        return string.Format("{0}.{1}", ShaderProgram.Name, Name);
    }
}