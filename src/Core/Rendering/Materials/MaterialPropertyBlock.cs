using KorpiEngine.AssetManagement;
using KorpiEngine.Rendering.Textures;

namespace KorpiEngine.Rendering.Materials;

public class MaterialPropertyBlock
{
    private readonly Dictionary<string, Color> _colors = new();
    private readonly Dictionary<string, Vector2> _vectors2 = new();
    private readonly Dictionary<string, Vector3> _vectors3 = new();
    private readonly Dictionary<string, Vector4> _vectors4 = new();
    private readonly Dictionary<string, float> _floats = new();
    private readonly Dictionary<string, int> _integers = new();
    private readonly Dictionary<string, Matrix4x4> _matrices = new();
    private readonly Dictionary<string, System.Numerics.Matrix4x4[]> _matrixArrays = new();
    private readonly Dictionary<string, ResourceRef<Texture2D>> _textures = new();


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
        _matrices = new Dictionary<string, Matrix4x4>(clone._matrices);
        _textures = new Dictionary<string, ResourceRef<Texture2D>>(clone._textures);
    }


    public bool IsEmpty => _colors.Count == 0 && _vectors4.Count == 0 && _vectors3.Count == 0 && _vectors2.Count == 0 && _floats.Count == 0 && _integers.Count == 0 &&
                           _matrices.Count == 0 && _textures.Count == 0;

    public void SetColor(string name, Color value) => _colors[name] = value;

    public Color GetColor(string name) => _colors.TryGetValue(name, out Color value) ? value : Color.White;
    
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

    public void SetMatrices(string name, IEnumerable<System.Numerics.Matrix4x4> value) => _matrixArrays[name] = value.ToArray();

    public Matrix4x4 GetMatrix(string name) => _matrices.TryGetValue(name, out Matrix4x4 value) ? value : Matrix4x4.Identity;
    
    public bool HasMatrix(string name) => _matrices.ContainsKey(name);
    
    public void SetTexture(string name, Texture2D? value)
    {
        if (value == null)
            _textures.Remove(name);
        else
            _textures[name] = value;
    }


    public void SetTexture(string name, ResourceRef<Texture2D> value) => _textures[name] = value;

    public ResourceRef<Texture2D>? GetTexture(string name)
    {
        if (_textures.TryGetValue(name, out ResourceRef<Texture2D> tex))
            return tex;
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
        foreach (KeyValuePair<string, Color> item in propertyBlock._colors)
            Graphics.Device.SetUniformV4(shader, item.Key, new Vector4(item.Value.R, item.Value.G, item.Value.B, item.Value.A));

        foreach ((string? key, Matrix4x4 mat) in propertyBlock._matrices)
        {
            System.Numerics.Matrix4x4 fMat = mat.ToFloat();
            Graphics.Device.SetUniformMatrix(shader, key, 1, false, in fMat.M11);
        }

        foreach ((string? key, System.Numerics.Matrix4x4[]? mats) in propertyBlock._matrixArrays)
        {
            Graphics.Device.SetUniformMatrix(shader, key, mats.Length, false, in mats[0].M11);
        }

        uint texSlot = 0;
        List<(string, ResourceRef<Texture2D>)> keysToUpdate = new();
        foreach (KeyValuePair<string, ResourceRef<Texture2D>> item in propertyBlock._textures)
        {
            ResourceRef<Texture2D> tex = item.Value;
            if (!tex.IsAvailable)
            {
                Application.Logger.Warn($"Texture '{item.Key}' on material '{materialName}' is not available (has it been destroyed?)");
                
                // Clear the texture slot
                Graphics.Device.ClearUniformTexture(shader, item.Key, (int)texSlot);
                
                // Remove from the property block
                propertyBlock.SetTexture(item.Key, null);
                
                continue;
            }
            
            texSlot++;
            Graphics.Device.SetUniformTexture(shader, item.Key, (int)texSlot, tex.Res!.Handle);

            keysToUpdate.Add((item.Key, tex));
        }

        foreach ((string, ResourceRef<Texture2D>) item in keysToUpdate)
            propertyBlock._textures[item.Item1] = item.Item2;
    }
}