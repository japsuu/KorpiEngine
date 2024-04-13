using System.Reflection;
using KorpiEngine.Core.Rendering.Shaders.ShaderPrograms;
using KorpiEngine.Core.Rendering.Shaders.Variables;
using KorpiEngine.Core.Rendering.Textures;
using OpenTK.Graphics.OpenGL4;

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
    }


    internal void Bind()
    {
        GLShader.Use();
        foreach (MaterialProperty property in _properties)
            property.Bind();
    }
}

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

/// <summary>
/// Abstract base class for 3D materials.
/// Defines the rendering properties of meshes.
/// </summary>
public abstract class BaseMaterial3D : Material
{
    public Color Color
    {
        get => _color.Value;
        set => _color.Value = value;
    }
    
    public Texture2D? MainTexture
    {
        get => _mainTexture.Texture;
        set => _mainTexture.Set(TextureUnit.Texture0, value);
    }

    private Uniform<Color> _color { get; set; } = null!;
    private TextureUniform<Texture2D> _mainTexture { get; set; } = null!;
    
    public override ShaderProgram GLShader => ShaderManager.StandardShader3D;

    
    protected override void RegisterMaterialProperties(List<MaterialProperty> properties)
    {
        _color = new Uniform<Color>();
        _mainTexture = new TextureUniform<Texture2D>();
        
        properties.Add(_color);
        properties.Add(_mainTexture);
    }

    
    protected override void SetMaterialPropertyDefaults()
    {
        _color.Value = Color.White;
    }
}

public class StandardMaterial3D : BaseMaterial3D
{
    
}

/*/// <summary>
/// An instance of a material.
/// Can be used to override the properties of a material for a specific object.
/// </summary>
public class MaterialInstance   //TODO: Delayed property setting
{
    private readonly Material _material;
    // private readonly Dictionary<string, MaterialPropertyBlock> _properties;


    public MaterialInstance(Material material)
    {
        _material = material;
        // _properties = new Dictionary<string, MaterialPropertyBlock>();
    }
    
    
    /#1#// <summary>
    /// Tries to find and return a uniform by its name.
    /// Expensive operation, use sparingly.
    /// </summary>
    /// <param name="name">The name of the uniform to find.</param>
    /// <param name="uniform">The found uniform, if any.</param>
    /// <typeparam name="T">The type of the uniform.</typeparam>
    /// <returns></returns>
    public bool TryGetUniformByName<T>(string name, out Uniform<T> uniform) where T : struct
    {
        // Get the glId from the name. If valid, construct an uniform.
        //TODO: Cache the returned Uniform, to set it's value to the shader when material is bound.
    }


    public void SetUniform<T>(Uniform<T> property, T value) where T : struct    // Uniform registering, is this needed?
    {
        // Set the property on the shader
        // This will depend on your Shader class implementation
        _material.GLShader.SetProperty(property.Name, value);
    }#1#


    public void SetTexture1(Texture texture) => throw new NotImplementedException();
    public void SetTexture2(Texture texture) => throw new NotImplementedException();
    public void SetTexture3(Texture texture) => throw new NotImplementedException();
    public void SetTexture4(Texture texture) => throw new NotImplementedException();


    public void Bind()
    {
        _material.SetShaderProperties();
        
        foreach (KeyValuePair<string, MaterialPropertyBlock> property in _properties)
            // Set the property on the shader
            // This will depend on your Shader class implementation
            _material.GLShader.SetProperty(property.Key, property.Value);
    }
}*/