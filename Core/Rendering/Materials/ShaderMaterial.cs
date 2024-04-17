using KorpiEngine.Core.Rendering.Shaders.ShaderPrograms;

namespace KorpiEngine.Core.Rendering.Materials;

/// <summary>
/// A material defined by a custom <see cref="ShaderProgram"/> and the values of its shader parameters.
/// </summary>
public abstract class ShaderMaterial : Material
{
    public override ShaderProgram GLShader { get; }


    protected ShaderMaterial(ShaderProgram glShader)
    {
        GLShader = glShader;
    }
}