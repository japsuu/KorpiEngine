using System.Numerics;
using KorpiEngine.Core.Rendering.Shaders.Variables;
using OpenTK.Mathematics;
using Vector2 = OpenTK.Mathematics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;
using Vector4 = OpenTK.Mathematics.Vector4;

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
public sealed class Material
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
            prop.Initialize(GLShader);
    }


    internal void Bind()
    {
        GLShader.Use();
        foreach (MaterialProperty property in _properties)
            property.Bind();
    }


    internal abstract void SetModelMatrix(Matrix4 modelMatrix);

    internal abstract void SetViewMatrix(Matrix4 viewMatrix); //TODO: Implement UniformBuffers to store these

    internal abstract void SetProjectionMatrix(Matrix4 projectionMatrix); //TODO: Implement UniformBuffers to store these
}

public class MaterialPropertyBlock
{
    private Dictionary<string, Color> colors = new();
    private Dictionary<string, Vector2> vectors2 = new();
    private Dictionary<string, Vector3> vectors3 = new();
    private Dictionary<string, Vector4> vectors4 = new();
    private Dictionary<string, float> floats = new();
    private Dictionary<string, int> ints = new();
    private Dictionary<string, Matrix4x4> matrices = new();
    private Dictionary<string, System.Numerics.Matrix4x4[]> matrixArr = new();
    private Dictionary<string, AssetRef<Texture2D>> textures = new();

    //private Dictionary<string, int> textureSlots = new();


    public MaterialPropertyBlock()
    {
    }


    public MaterialPropertyBlock(MaterialPropertyBlock clone)
    {
        colors = new Dictionary<string, Color>(clone.colors);
        vectors2 = new Dictionary<string, Vector2>(clone.vectors2);
        vectors3 = new Dictionary<string, Vector3>(clone.vectors3);
        vectors4 = new Dictionary<string, Vector4>(clone.vectors4);
        floats = new Dictionary<string, float>(clone.floats);
        ints = new Dictionary<string, int>(clone.ints);
        matrices = new Dictionary<string, Matrix4x4>(clone.matrices);
        textures = new Dictionary<string, AssetRef<Texture2D>>(clone.textures);
    }


    public bool isEmpty => colors.Count == 0 && vectors4.Count == 0 && vectors3.Count == 0 && vectors2.Count == 0 && floats.Count == 0 && ints.Count == 0 &&
                           matrices.Count == 0 && textures.Count == 0;

    public void SetColor(string name, Color value) => colors[name] = value;

    public Color GetColor(string name) => colors.ContainsKey(name) ? colors[name] : Color.white;

    public void SetVector(string name, Vector2 value) => vectors2[name] = value;

    public Vector2 GetVector2(string name) => vectors2.ContainsKey(name) ? vectors2[name] : Vector2.zero;

    public void SetVector(string name, Vector3 value) => vectors3[name] = value;

    public Vector3 GetVector3(string name) => vectors3.ContainsKey(name) ? vectors3[name] : Vector3.zero;

    public void SetVector(string name, Vector4 value) => vectors4[name] = value;

    public Vector4 GetVector4(string name) => vectors4.ContainsKey(name) ? vectors4[name] : Vector4.zero;

    public void SetFloat(string name, float value) => floats[name] = value;

    public float GetFloat(string name) => floats.ContainsKey(name) ? floats[name] : 0;

    public void SetInt(string name, int value) => ints[name] = value;

    public int GetInt(string name) => ints.ContainsKey(name) ? ints[name] : 0;

    public void SetMatrix(string name, Matrix4x4 value) => matrices[name] = value;

    public void SetMatrices(string name, System.Numerics.Matrix4x4[] value) => matrixArr[name] = value.Cast<System.Numerics.Matrix4x4>().ToArray();

    public Matrix4x4 GetMatrix(string name) => matrices.ContainsKey(name) ? matrices[name] : Matrix4x4.Identity;

    public void SetTexture(string name, Texture2D value) => textures[name] = value;

    public void SetTexture(string name, AssetRef<Texture2D> value) => textures[name] = value;

    public AssetRef<Texture2D>? GetTexture(string name) => textures.ContainsKey(name) ? textures[name] : null;


    public void Clear()
    {
        textures.Clear();
        matrices.Clear();
        ints.Clear();
        floats.Clear();
        vectors2.Clear();
        vectors3.Clear();
        vectors4.Clear();
        colors.Clear();
    }


    public static void Apply(MaterialPropertyBlock mpb, GraphicsProgram shader)
    {
        foreach (KeyValuePair<string, float> item in mpb.floats)
            Graphics.Device.SetUniformF(shader, item.Key, item.Value);

        foreach (KeyValuePair<string, int> item in mpb.ints)
            Graphics.Device.SetUniformI(shader, item.Key, (int)item.Value);

        foreach (KeyValuePair<string, Vector2> item in mpb.vectors2)
            Graphics.Device.SetUniformV2(shader, item.Key, item.Value);
        foreach (KeyValuePair<string, Vector3> item in mpb.vectors3)
            Graphics.Device.SetUniformV3(shader, item.Key, item.Value);
        foreach (KeyValuePair<string, Vector4> item in mpb.vectors4)
            Graphics.Device.SetUniformV4(shader, item.Key, item.Value);
        foreach (KeyValuePair<string, Color> item in mpb.colors)
            Graphics.Device.SetUniformV4(shader, item.Key, new System.Numerics.Vector4(item.Value.r, item.Value.g, item.Value.b, item.Value.a));

        foreach (var item in mpb.matrices)
        {
            var m = item.Value.ToFloat();
            Graphics.Device.SetUniformMatrix(shader, item.Key, 1, false, in m.M11);
        }

        foreach (KeyValuePair<string, Matrix4x4[]> item in mpb.matrixArr)
        {
            Matrix4x4[] m = item.Value;
            Graphics.Device.SetUniformMatrix(shader, item.Key, (uint)item.Value.Length, false, in m[0].M11);
        }

        uint texSlot = 0;
        var keysToUpdate = new List<(string, AssetRef<Texture2D>)>();
        foreach (var item in mpb.textures)
        {
            var tex = item.Value;
            if (tex.IsAvailable)
            {
                texSlot++;
                Graphics.Device.SetUniformTexture(shader, item.Key, (int)texSlot, tex.Res!.Handle);

                keysToUpdate.Add((item.Key, tex));
            }
        }

        foreach (var item in keysToUpdate)
            mpb.textures[item.Item1] = item.Item2;
    }
}