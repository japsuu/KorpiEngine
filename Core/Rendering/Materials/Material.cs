using KorpiEngine.Core.Rendering.Shaders.Variables;
using OpenTK.Mathematics;

namespace KorpiEngine.Core.Rendering.Materials;

/// <summary>
/// A material used for rendering.
/// Objects with a similar material may be batched together for rendering.
///
/// TODO: Figure out if shader preprocessor-based permutations are worth implementing, over a uniform-based branching system.
/// https://www.reddit.com/r/GraphicsProgramming/comments/7llloo/comment/drnyosg/?utm_source=share&utm_medium=web3x&utm_name=web3xcss&utm_term=1&utm_content=share_button
/// https://github.com/michaelsakharov/Prowl/blob/main/Prowl.Runtime/Resources/Shader.cs#L70
/// https://github.com/michaelsakharov/Prowl/blob/main/Prowl.Runtime/Resources/Material.cs#L140
/// </summary>
public abstract class Material
{
    public abstract GraphicsProgram GLShader { get; }

    private List<MaterialProperty> _properties = null!;


    protected Material()
    {
        InitializeMaterialProperties();
    }
    
    
    protected abstract void RegisterMaterialProperties(List<MaterialProperty> properties);
    protected abstract void SetMaterialPropertyDefaults();


    private void InitializeMaterialProperties()
    {
        _properties = new List<MaterialProperty>();
        RegisterMaterialProperties(_properties);
        SetMaterialPropertyDefaults();
        foreach (MaterialProperty prop in _properties)
        {
            prop.Initialize(GLShader);
        }
    }


    internal void Bind()
    {
        GLShader.Use();
        foreach (MaterialProperty property in _properties)
            property.Bind();
    }
    
    
    internal abstract void SetModelMatrix(Matrix4 modelMatrix);
    internal abstract void SetViewMatrix(Matrix4 viewMatrix);   //TODO: Implement UniformBuffers to store these
    internal abstract void SetProjectionMatrix(Matrix4 projectionMatrix);   //TODO: Implement UniformBuffers to store these
}