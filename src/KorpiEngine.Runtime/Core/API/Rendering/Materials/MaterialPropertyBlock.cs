using KorpiEngine.Core.API.Rendering.Textures;
using KorpiEngine.Core.Internal.Assets;
using KorpiEngine.Core.Rendering;
using OpenTK.Mathematics;

namespace KorpiEngine.Core.API.Rendering.Materials;

public class MaterialPropertyBlock
{
    private readonly Dictionary<string, Color> _colors = new();
    private readonly Dictionary<string, Vector2> _vectors2 = new();
    private readonly Dictionary<string, Vector3> _vectors3 = new();
    private readonly Dictionary<string, Vector4> _vectors4 = new();
    private readonly Dictionary<string, float> _floats = new();
    private readonly Dictionary<string, int> _integers = new();
    private readonly Dictionary<string, Matrix4> _matrices = new();
    private readonly Dictionary<string, Matrix4[]> _matrixArrays = new();
    private readonly Dictionary<string, AssetRef<Texture2D>> _textures = new();


    public MaterialPropertyBlock()
    {
    }


    public MaterialPropertyBlock(MaterialPropertyBlock clone)
    {
        _colors = new Dictionary<string, Color>(clone._colors);
        _vectors2 = new Dictionary<string, Vector2>(clone._vectors2);
        _vectors3 = new Dictionary<string, Vector3>(clone._vectors3);
        _vectors4 = new Dictionary<string, Vector4>(clone._vectors4);
        _floats = new Dictionary<string, float>(clone._floats);
        _integers = new Dictionary<string, int>(clone._integers);
        _matrices = new Dictionary<string, Matrix4>(clone._matrices);
        _textures = new Dictionary<string, AssetRef<Texture2D>>(clone._textures);
    }


    public bool IsEmpty => _colors.Count == 0 && _vectors4.Count == 0 && _vectors3.Count == 0 && _vectors2.Count == 0 && _floats.Count == 0 && _integers.Count == 0 &&
                           _matrices.Count == 0 && _textures.Count == 0;

    public void SetColor(string name, Color value) => _colors[name] = value;

    public Color GetColor(string name) => _colors.ContainsKey(name) ? _colors[name] : Color.White;

    public void SetVector(string name, Vector2 value) => _vectors2[name] = value;

    public Vector2 GetVector2(string name) => _vectors2.ContainsKey(name) ? _vectors2[name] : Vector2.Zero;

    public void SetVector(string name, Vector3 value) => _vectors3[name] = value;

    public Vector3 GetVector3(string name) => _vectors3.ContainsKey(name) ? _vectors3[name] : Vector3.Zero;

    public void SetVector(string name, Vector4 value) => _vectors4[name] = value;

    public Vector4 GetVector4(string name) => _vectors4.ContainsKey(name) ? _vectors4[name] : Vector4.Zero;

    public void SetFloat(string name, float value) => _floats[name] = value;

    public float GetFloat(string name) => _floats.ContainsKey(name) ? _floats[name] : 0;

    public void SetInt(string name, int value) => _integers[name] = value;

    public int GetInt(string name) => _integers.ContainsKey(name) ? _integers[name] : 0;

    public void SetMatrix(string name, Matrix4 value) => _matrices[name] = value;

    public void SetMatrices(string name, IEnumerable<Matrix4> value) => _matrixArrays[name] = value.ToArray();

    public Matrix4 GetMatrix(string name) => _matrices.ContainsKey(name) ? _matrices[name] : Matrix4.Identity;

    public void SetTexture(string name, Texture2D value) => _textures[name] = value;

    public void SetTexture(string name, AssetRef<Texture2D> value) => _textures[name] = value;

    public AssetRef<Texture2D>? GetTexture(string name)
    {
        if (_textures.TryGetValue(name, out AssetRef<Texture2D> tex))
            return tex;
        return null;
    }


    public void Clear()
    {
        _textures.Clear();
        _matrices.Clear();
        _integers.Clear();
        _floats.Clear();
        _vectors2.Clear();
        _vectors3.Clear();
        _vectors4.Clear();
        _colors.Clear();
    }
    
    
    internal void Apply(GraphicsProgram shader)
    {
        Apply(this, shader);
    }


    private static void Apply(MaterialPropertyBlock propertyBlock, GraphicsProgram shader)
    {
        foreach (KeyValuePair<string, float> item in propertyBlock._floats)
            Graphics.Driver.SetUniformF(shader, item.Key, item.Value);

        foreach (KeyValuePair<string, int> item in propertyBlock._integers)
            Graphics.Driver.SetUniformI(shader, item.Key, item.Value);

        foreach (KeyValuePair<string, Vector2> item in propertyBlock._vectors2)
            Graphics.Driver.SetUniformV2(shader, item.Key, item.Value);
        foreach (KeyValuePair<string, Vector3> item in propertyBlock._vectors3)
            Graphics.Driver.SetUniformV3(shader, item.Key, item.Value);
        foreach (KeyValuePair<string, Vector4> item in propertyBlock._vectors4)
            Graphics.Driver.SetUniformV4(shader, item.Key, item.Value);
        foreach (KeyValuePair<string, Color> item in propertyBlock._colors)
            Graphics.Driver.SetUniformV4(shader, item.Key, new Vector4(item.Value.R, item.Value.G, item.Value.B, item.Value.A));

        foreach ((string? key, Matrix4 mat) in propertyBlock._matrices)
        {
            Graphics.Driver.SetUniformMatrix(shader, key, 1, false, in mat.Row0.X);
        }

        foreach ((string? key, Matrix4[]? mats) in propertyBlock._matrixArrays)
        {
            Graphics.Driver.SetUniformMatrix(shader, key, mats.Length, false, in mats[0].Row0.X);
        }

        uint texSlot = 0;
        List<(string, AssetRef<Texture2D>)> keysToUpdate = new();
        foreach (KeyValuePair<string, AssetRef<Texture2D>> item in propertyBlock._textures)
        {
            AssetRef<Texture2D> tex = item.Value;
            if (!tex.IsAvailable)
                continue;
            
            texSlot++;
            Graphics.Driver.SetUniformTexture(shader, item.Key, (int)texSlot, tex.Res!.Handle);

            keysToUpdate.Add((item.Key, tex));
        }

        foreach ((string, AssetRef<Texture2D>) item in keysToUpdate)
            propertyBlock._textures[item.Item1] = item.Item2;
    }
}