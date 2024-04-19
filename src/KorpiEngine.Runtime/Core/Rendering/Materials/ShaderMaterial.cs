namespace KorpiEngine.Core.Rendering.Materials;

/// <summary>
/// A material defined by a custom <see cref="GraphicsProgram"/> and the values of its shader parameters.
/// </summary>
public abstract class ShaderMaterial : Material
{
    public override GraphicsProgram GLShader { get; }


    protected ShaderMaterial(GraphicsProgram glShader)
    {
        GLShader = glShader;
    }
}