using KorpiEngine.Core.Rendering.Textures;
using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.Core.Rendering.Shaders.Variables;

/// <summary>
/// Represents a typed texture uniform. Allows only textures of the given type to be bound.
/// </summary>
public class TextureUniform<T> : Uniform<int> where T : Texture
{
    public T? Texture;
    public TextureUnit Unit;
    
    
    internal TextureUniform(string shaderPropertyName) : base(GL.Uniform1, shaderPropertyName)
    {
    }


    /// <summary>
    /// Binds a texture to the given texture unit and sets the corresponding uniform to the respective number to access it.
    /// </summary>
    /// <param name="unit">The texture unit to bind to.</param>
    /// <param name="texture">The texture to bind.</param>
    public void Set(TextureUnit unit, T? texture)
    {
        Unit = unit;
        Value = (int)Unit - (int)TextureUnit.Texture0;
        
        if (Texture != null && texture == null)
            Texture.Unbind();
        
        Texture = texture;
    }


    protected override void BindProperty()
    {
        Texture?.Bind(Unit);
        base.BindProperty();
    }
}

/// <summary>
/// Represents a texture uniform. Allows any texture type to be bound.
/// </summary>
public sealed class TextureUniform : TextureUniform<Texture>
{
    internal TextureUniform(string shaderPropertyName) : base(shaderPropertyName)
    {
    }
}