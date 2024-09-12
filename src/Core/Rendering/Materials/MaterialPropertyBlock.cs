using KorpiEngine.AssetManagement;
using KorpiEngine.Mathematics;

namespace KorpiEngine.Rendering;

public sealed class MaterialPropertyBlock : IDisposable
{
    private readonly Dictionary<string, ColorHDR> _colors = new();
    private readonly Dictionary<string, Vector2> _vectors2 = new();
    private readonly Dictionary<string, Vector3> _vectors3 = new();
    private readonly Dictionary<string, Vector4> _vectors4 = new();
    private readonly Dictionary<string, float> _floats = new();
    private readonly Dictionary<string, int> _integers = new();
    private readonly Dictionary<string, Matrix4x4> _matrices = new();
    private readonly Dictionary<string, Matrix4x4[]> _matrixArrays = new();
    private readonly Dictionary<string, AssetReference<Texture2D>> _textures = new();

    public bool IsEmpty =>
        _colors.Count   == 0 &&
        _vectors4.Count == 0 &&
        _vectors3.Count == 0 &&
        _vectors2.Count == 0 &&
        _floats.Count   == 0 &&
        _integers.Count == 0 &&
        _matrices.Count == 0 &&
        _textures.Count == 0;

    public void SetColor(string name, ColorHDR value) => _colors[name] = value;
    public ColorHDR GetColor(string name) => _colors.TryGetValue(name, out ColorHDR value) ? value : ColorHDR.White;
    public bool HasColor(string name) => _colors.ContainsKey(name);

    public void SetVector2(string name, Vector2 value) => _vectors2[name] = value;
    public Vector2 GetVector2(string name) => _vectors2.TryGetValue(name, out Vector2 value) ? value : Vector2.Zero;
    public bool HasVector2(string name) => _vectors2.ContainsKey(name);

    public void SetVector3(string name, Vector3 value) => _vectors3[name] = value;
    public Vector3 GetVector3(string name) => _vectors3.TryGetValue(name, out Vector3 value) ? value : Vector3.Zero;
    public bool HasVector3(string name) => _vectors3.ContainsKey(name);

    public void SetVector4(string name, Vector4 value) => _vectors4[name] = value;
    public Vector4 GetVector4(string name) => _vectors4.TryGetValue(name, out Vector4 value) ? value : Vector4.Zero;
    public bool HasVector4(string name) => _vectors4.ContainsKey(name);

    public void SetFloat(string name, float value) => _floats[name] = value;
    public float GetFloat(string name) => _floats.GetValueOrDefault(name, 0);
    public bool HasFloat(string name) => _floats.ContainsKey(name);

    public void SetInt(string name, int value) => _integers[name] = value;
    public int GetInt(string name) => _integers.GetValueOrDefault(name, 0);
    public bool HasInt(string name) => _integers.ContainsKey(name);

    public void SetMatrix(string name, Matrix4x4 value) => _matrices[name] = value;
    public void SetMatrices(string name, IEnumerable<Matrix4x4> value) => _matrixArrays[name] = value.ToArray();
    public Matrix4x4 GetMatrix(string name) => _matrices.TryGetValue(name, out Matrix4x4 value) ? value : Matrix4x4.Identity;
    public bool HasMatrix(string name) => _matrices.ContainsKey(name);
    
    public void SetTexture(string name, Texture2D? value)
    {
        if (value == null)
        {
            if (_textures.Remove(name, out AssetReference<Texture2D>? tex))
                tex.Release();
        }
        else
            _textures[name] = value.CreateReference();
    }


    public Texture2D? GetTexture(string name)
    {
        if (_textures.TryGetValue(name, out AssetReference<Texture2D>? tex))
            return tex.Asset;
        return null;
    }
    
    public bool HasTexture(string name) => _textures.ContainsKey(name);


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
    
    
    internal void Apply(GraphicsProgram shader, string materialName)
    {
        Apply(this, shader, materialName);
    }


    private static void Apply(MaterialPropertyBlock propertyBlock, GraphicsProgram shader, string materialName)
    {
        foreach (KeyValuePair<string, float> item in propertyBlock._floats)
            Graphics.Device.SetUniformF(shader, item.Key, item.Value);
        foreach (KeyValuePair<string, int> item in propertyBlock._integers)
            Graphics.Device.SetUniformI(shader, item.Key, item.Value);
        foreach (KeyValuePair<string, Vector2> item in propertyBlock._vectors2)
            Graphics.Device.SetUniformV2(shader, item.Key, item.Value);
        foreach (KeyValuePair<string, Vector3> item in propertyBlock._vectors3)
            Graphics.Device.SetUniformV3(shader, item.Key, item.Value);
        foreach (KeyValuePair<string, Vector4> item in propertyBlock._vectors4)
            Graphics.Device.SetUniformV4(shader, item.Key, item.Value);
        foreach (KeyValuePair<string, ColorHDR> item in propertyBlock._colors)
            Graphics.Device.SetUniformV4(shader, item.Key, new Vector4(item.Value.R, item.Value.G, item.Value.B, item.Value.A));
        foreach ((string? key, Matrix4x4 mat) in propertyBlock._matrices)
            Graphics.Device.SetUniformMatrix(shader, key, 1, false, in mat.M11);
        foreach ((string? key, Matrix4x4[]? mats) in propertyBlock._matrixArrays)
            Graphics.Device.SetUniformMatrix(shader, key, mats.Length, false, in mats[0].M11);

        uint texSlot = 0;
        List<(string, AssetReference<Texture2D>)> keysToUpdate = [];
        foreach ((string? key, AssetReference<Texture2D>? tex) in propertyBlock._textures)
        {
            if (!tex.IsAlive)
            {
                Application.Logger.Warn($"Texture '{key}' on material '{materialName}' is not available (has it been destroyed?)");
                
                // Clear the texture slot
                Graphics.Device.ClearUniformTexture(shader, key, (int)texSlot);
                
                // Remove from the property block
                propertyBlock.SetTexture(key, null);
                
                continue;
            }
            
            texSlot++;
            Graphics.Device.SetUniformTexture(shader, key, (int)texSlot, tex.Asset!.Handle);

            keysToUpdate.Add((key, tex));
        }

        foreach ((string, AssetReference<Texture2D>) item in keysToUpdate)
            propertyBlock._textures[item.Item1] = item.Item2;
    }


    public void Dispose()
    {
        _colors.Clear();
        _vectors2.Clear();
        _vectors3.Clear();
        _vectors4.Clear();
        _floats.Clear();
        _integers.Clear();
        _matrices.Clear();
        _matrixArrays.Clear();
        
        Console.WriteLine("Dispose texes");
        
        foreach (AssetReference<Texture2D> tex in _textures.Values)
            tex.Release();
        _textures.Clear();
    }
}