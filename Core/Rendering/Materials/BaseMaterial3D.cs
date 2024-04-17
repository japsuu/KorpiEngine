using KorpiEngine.Core.Rendering.Shaders.ShaderPrograms;
using KorpiEngine.Core.Rendering.Shaders.Variables;
using KorpiEngine.Core.Rendering.Textures;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace KorpiEngine.Core.Rendering.Materials;

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

    private Uniform<Color> _color = null!;
    private TextureUniform<Texture2D> _mainTexture = null!;
    private Uniform<Matrix4> _modelMatrix = null!;
    private Uniform<Matrix4> _viewMatrix = null!;
    private Uniform<Matrix4> _projectionMatrix = null!;
    
    public override ShaderProgram GLShader => ShaderManager.StandardShader3D;

    
    protected override void RegisterMaterialProperties(List<MaterialProperty> properties)
    {
        _color = new Uniform<Color>("u_Color");
        _mainTexture = new TextureUniform<Texture2D>("u_MainTexture");
        _modelMatrix = new Uniform<Matrix4>("u_ModelMatrix");
        _viewMatrix = new Uniform<Matrix4>("u_ViewMatrix");
        _projectionMatrix = new Uniform<Matrix4>("u_ProjectionMatrix");
        
        properties.Add(_color);
        properties.Add(_mainTexture);
        properties.Add(_modelMatrix);
        properties.Add(_viewMatrix);
        properties.Add(_projectionMatrix);
    }

    
    protected override void SetMaterialPropertyDefaults()
    {
        _color.Value = Color.White;
        _modelMatrix.Value = Matrix4.Identity;
        _viewMatrix.Value = Matrix4.Identity;
        _projectionMatrix.Value = Matrix4.Identity;
    }


    internal override void SetModelMatrix(Matrix4 modelMatrix) => _modelMatrix.Value = modelMatrix;
    internal override void SetViewMatrix(Matrix4 viewMatrix) => _viewMatrix.Value = viewMatrix;
    internal override void SetProjectionMatrix(Matrix4 projectionMatrix) => _projectionMatrix.Value = projectionMatrix;
}