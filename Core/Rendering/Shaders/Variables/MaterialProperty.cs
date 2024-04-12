using System.Reflection;
using KorpiEngine.Core.Rendering.Shaders.ShaderPrograms;

namespace KorpiEngine.Core.Rendering.Shaders.Variables;

/// <summary>
/// Represents a shader variable identified by its name and the corresponding shaderProgram handle.
/// </summary>
public abstract class MaterialProperty
{
    /// <summary>
    /// The handle of the shaderProgram to which this variable relates.
    /// </summary>
    protected ShaderProgram ShaderProgram { get; private set; } = null!;

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
    internal void Initialize(ShaderProgram shaderProgram, PropertyInfo property)
    {
        ShaderProgram = shaderProgram;
        Name = property.Name;
        InitializeVariable(shaderProgram, property);
    }


    protected virtual void InitializeVariable(ShaderProgram shaderProgram, PropertyInfo property)
    {
    }
    
    
    internal void Bind()
    {
        if (!Active)
            return;
        ShaderProgram.AssertActive();
        BindProperty();
    }


    protected abstract void BindProperty();


    public override string ToString()
    {
        return $"{ShaderProgram.Name}.{Name}";
    }
}