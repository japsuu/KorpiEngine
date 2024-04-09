using KorpiEngine.Core.Rendering.Shaders.ShaderPrograms;
using KorpiEngine.Core.Rendering.Textures;

namespace KorpiEngine.Core.Rendering.Materials;

/// <summary>
/// A material used for rendering.
/// Objects with a similar material may be batched together for rendering.
/// </summary>
public abstract class Material
{
    public abstract ShaderProgram GLShader { get; }
    internal abstract void SetShaderProperties();
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
    private Color _color = new(1f, 1f, 1f, 1f);
    private Texture2D? _mainTexture = null;
    
    public override ShaderProgram GLShader => ShaderManager.Standard3DShaderProgram;
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
    
    
    /// <summary>
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
    }


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