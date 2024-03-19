using KorpiEngine.Core.Rendering.Shaders.ShaderPrograms;
using KorpiEngine.Core.Rendering.Textures;

namespace KorpiEngine.Core.Rendering.Materials;

/// <summary>
/// A material used for rendering.
/// Objects with a similar material can be batched together for rendering.
/// </summary>
public abstract class Material
{
    public abstract void SetShaderProperties();
}

/// <summary>
/// A material defined by a custom <see cref="ShaderProgram"/> and the values of its shader parameters.
/// </summary>
public abstract class ShaderMaterial : Material
{
    internal readonly ShaderProgram GLShader;


    protected ShaderMaterial(ShaderProgram glShader)
    {
        GLShader = glShader;
    }
}

//TODO: Implement MaterialInstance
/// <summary>
/// An instance of a material.
/// Can be used to change the properties of a material for a specific object.
/// </summary>
public class MaterialInstance
{
    private struct MaterialPropertyBlock
    {
        public string Name { get; }
        public object Value { get; }


        public MaterialPropertyBlock(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }

    private readonly Material _material;
    private readonly Dictionary<string, MaterialPropertyBlock> _properties;


    public MaterialInstance(Material material)
    {
        _material = material;
        _properties = new Dictionary<string, MaterialPropertyBlock>();
    }


    public void SetProperty(string name, object value)
    {
        _properties[name] = new MaterialPropertyBlock(name, value);
    }


    public object? GetProperty(string name) => _properties.TryGetValue(name, out MaterialPropertyBlock value) ? value : null;


    public void Bind()
    {
        foreach (KeyValuePair<string, MaterialPropertyBlock> property in _properties)

            // Set the property on the shader
            // This will depend on your Shader class implementation
            _material.GLShader.SetProperty(property.Key, property.Value);

        _material.SetShaderProperties();
    }
}

/// <summary>
/// Abstract base class for 3D materials.
/// Defines the rendering properties of meshes.
/// </summary>
public abstract class BaseMaterial3D : Material
{
    private Color _color = new(1f, 1f, 1f, 1f);
    private Texture2D? _mainTexture = null;
}

public class StandardMaterial3D : BaseMaterial3D
{
    
}