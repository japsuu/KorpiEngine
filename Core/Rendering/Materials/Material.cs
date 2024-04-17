using KorpiEngine.Core.Rendering.Shaders.ShaderPrograms;
using KorpiEngine.Core.Rendering.Shaders.Variables;
using OpenTK.Mathematics;

namespace KorpiEngine.Core.Rendering.Materials;

/// <summary>
/// A material used for rendering.
/// Objects with a similar material may be batched together for rendering.
/// </summary>
public abstract class Material
{
    public abstract ShaderProgram GLShader { get; }

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