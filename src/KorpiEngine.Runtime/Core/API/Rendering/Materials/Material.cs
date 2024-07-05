using KorpiEngine.Core.API.Rendering.Shaders;
using KorpiEngine.Core.API.Rendering.Textures;
using KorpiEngine.Core.Internal.AssetManagement;
using KorpiEngine.Core.Internal.Utils;
using KorpiEngine.Core.Rendering;

namespace KorpiEngine.Core.API.Rendering.Materials;

/// <summary>
/// A material used for rendering.
/// Objects with a similar material may be batched together for rendering.
/// Uses shader preprocessor-based permutations over a uniform-based branching system.
/// </summary>
// https://www.reddit.com/r/GraphicsProgramming/comments/7llloo/comment/drnyosg/?utm_source=share&utm_medium=web3x&utm_name=web3xcss&utm_term=1&utm_content=share_button
// https://github.com/michaelsakharov/Prowl/blob/main/Prowl.Runtime/Resources/Material.cs#L140
public sealed class Material : Resource
{
    public readonly ResourceRef<Shader> Shader;

    // Key is Shader.GUID + "-" + keywords + "-" + Shader.globalKeywords
    private static readonly Dictionary<string, Shader.CompiledShader> PassVariants = new();
    private readonly SortedSet<string> _materialKeywords = [];
    private readonly MaterialPropertyBlock _propertyBlock;
    private int _lastHash = -1;
    private int _lastGlobalKeywordsVersion = -1;
    private string _materialKeywordsString = "";
    private string _allKeywordsString = "";

    public int PassCount => Shader.IsAvailable ? GetCompiledVariant().Passes.Length : 0;


    public Material(ResourceRef<Shader> shader, string name) : base(name)
    {
        if (shader.AssetID == Guid.Empty)
            throw new ArgumentNullException(nameof(shader));
        Shader = shader;
        _propertyBlock = new MaterialPropertyBlock();
    }
    
    
    internal void ApplyPropertyBlock(GraphicsProgram shader)
    {
        _propertyBlock.Apply(shader, Name);
    }


    #region KEYWORDS

    public void SetKeyword(string keyword, bool state)
    {
        if (state)
            EnableKeyword(keyword);
        else
            DisableKeyword(keyword);
    }


    public void EnableKeyword(string? keyword)
    {
        string? key = keyword?.ToUpper().Replace(" ", "").Replace(";", "");
        if (string.IsNullOrWhiteSpace(key))
            return;
        
        _materialKeywords.Add(key);
    }


    public void DisableKeyword(string? keyword)
    {
        string? key = keyword?.ToUpper().Replace(" ", "").Replace(";", "");
        if (string.IsNullOrWhiteSpace(key))
            return;
        
        _materialKeywords.Remove(key);
    }


    public bool IsKeywordEnabled(string keyword) => _materialKeywords.Contains(keyword.ToUpper().Replace(" ", "").Replace(";", ""));

    #endregion


    #region PASSES

    public void SetPass(int pass, bool applyProperties = false)
    {
        if (!Shader.IsAvailable)
            return;
        
        Shader.CompiledShader shader = GetCompiledVariant();

        // Make sure we have a valid pass
        if (pass < 0 || pass >= shader.Passes.Length)
            return;

        InternalSetPass(shader.Passes[pass], applyProperties);
    }


    public void SetShadowPass(bool applyProperties = false)
    {
        if (!Shader.IsAvailable)
            return;
        
        Shader.CompiledShader shader = GetCompiledVariant();
        InternalSetPass(shader.ShadowPass, applyProperties);
    }


    private void InternalSetPass(Shader.CompiledShader.Pass pass, bool apply = false)
    {
        // Set the shader
        Graphics.Driver.SetState(pass.State);
        Graphics.Driver.BindProgram(pass.Program);

        if (apply)
            ApplyPropertyBlock(Graphics.Driver.CurrentProgram!);
    }

    #endregion


    #region VARIANT COMPILATION

    private Shader.CompiledShader GetCompiledVariant()
    {
        //return GetVariantExperimental(_materialKeywords.ToArray());
        if (Shader.IsAvailable == false)
            throw new Exception("Cannot compile without a valid shader assigned");
        
        int currentHash = Hashing.GetAdditiveHashCode(_materialKeywords);

        bool globalKeywordsChanged = _lastGlobalKeywordsVersion != Shaders.Shader.GlobalKeywordsVersion;
        bool materialKeywordsChanged = currentHash != _lastHash;
        if (globalKeywordsChanged || materialKeywordsChanged)
        {
            _materialKeywordsString = string.Join("-", _materialKeywords);
            _allKeywordsString = $"{Shader.Res!.InstanceID}-{_materialKeywordsString}-{Shaders.Shader.GlobalKeywordsString}";
            _lastGlobalKeywordsVersion = Shaders.Shader.GlobalKeywordsVersion;
            _lastHash = currentHash;
        }

        if (PassVariants.TryGetValue(_allKeywordsString, out Shader.CompiledShader s))
            return s;

        // Create a collection of all keywords
        List<string> allKeywords = [];
        
        // Add all global keywords
        allKeywords.AddRange(Shaders.Shader.GetGlobalKeywords());
        
        // Add all material keywords
        foreach (string keyword in _materialKeywords)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                continue;
            
            if (allKeywords.Contains(keyword))
                continue;
            
            allKeywords.Add(keyword);
        }

        // Compile Each Pass
        Application.Logger.Debug($"Compiling Shader Variant: {_allKeywordsString}");
        Shader.CompiledShader compiledPasses = Shader.Res!.Compile(allKeywords.ToArray());

        PassVariants[_allKeywordsString] = compiledPasses;
        return compiledPasses;
    }
    
    /*Shader.CompiledShader GetVariantExperimental(string[] allKeywords)
    {
        if (Shader.IsAvailable == false) throw new Exception("Cannot compile without a valid shader assigned");

        string keywords = string.Join("-", allKeywords);
        string key = Shader.Res.InstanceID + "-" + keywords + "-" + Shaders.Shader.GetGlobalKeywords();
        if (PassVariants.TryGetValue(key, out var s)) return s;

        // Add each global togather making sure to not add duplicates
        string[] globals = Shaders.Shader.GetGlobalKeywords().ToArray();
        for (int i = 0; i < globals.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(globals[i])) continue;
            if (allKeywords.Contains(globals[i], StringComparer.OrdinalIgnoreCase)) continue;
            allKeywords = allKeywords.Append(globals[i]).ToArray();
        }
        // Remove empty keywords
        allKeywords = allKeywords.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

        // Compile Each Pass
        Shader.CompiledShader compiledPasses = Shader.Res!.Compile(allKeywords);

        PassVariants[key] = compiledPasses;
        return compiledPasses;
    }*/

    #endregion


    #region PROPERTY SETTERS

    public void SetColor(string name, Color value, bool allowFail = false)
    {
        if (HasVariable(name))
            _propertyBlock.SetColor(name, value);
        else if (!allowFail)
            Application.Logger.Warn($"Material {Name} does not have a property named {name}");
    }


    public void SetVector(string name, Vector2 value, bool allowFail = false)
    {
        if (HasVariable(name))
            _propertyBlock.SetVector(name, value);
        else if (!allowFail)
            Application.Logger.Warn($"Material {Name} does not have a property named {name}");
    }


    public void SetVector(string name, Vector3 value, bool allowFail = false)
    {
        if (HasVariable(name))
            _propertyBlock.SetVector(name, value);
        else if (!allowFail)
            Application.Logger.Warn($"Material {Name} does not have a property named {name}");
    }


    public void SetVector(string name, Vector4 value, bool allowFail = false)
    {
        if (HasVariable(name))
            _propertyBlock.SetVector(name, value);
        else if (!allowFail)
            Application.Logger.Warn($"Material {Name} does not have a property named {name}");
    }


    public void SetFloat(string name, float value, bool allowFail = false)
    {
        if (HasVariable(name))
            _propertyBlock.SetFloat(name, value);
        else if (!allowFail)
            Application.Logger.Warn($"Material {Name} does not have a property named {name}");
    }


    public void SetInt(string name, int value, bool allowFail = false)
    {
        if (HasVariable(name))
            _propertyBlock.SetInt(name, value);
        else if (!allowFail)
            Application.Logger.Warn($"Material {Name} does not have a property named {name}");
    }


    public void SetMatrix(string name, Matrix4x4 value, bool allowFail = false)
    {
        if (HasVariable(name))
            _propertyBlock.SetMatrix(name, value);
        else if (!allowFail)
            Application.Logger.Warn($"Material {Name} does not have a property named {name}");
    }


    public void SetMatrices(string name, IEnumerable<System.Numerics.Matrix4x4> value, bool allowFail = false)
    {
        if (HasVariable(name))
            _propertyBlock.SetMatrices(name, value);
        else if (!allowFail)
            Application.Logger.Warn($"Material {Name} does not have a property named {name}");
    }


    public void SetTexture(string name, Texture2D value, bool allowFail = false)
    {
        if (HasVariable(name))
        {
            _propertyBlock.SetTexture(name, value);
            //Console.WriteLine($"SetTex {Name}: {name} - {value.Name}");
        }
        else if (!allowFail)
            Application.Logger.Warn($"Material {Name} does not have a property named {name}");
    }


    public void SetTexture(string name, ResourceRef<Texture2D> value, bool allowFail = false)
    {
        if (HasVariable(name))
            _propertyBlock.SetTexture(name, value);
        else if (!allowFail)
            Application.Logger.Warn($"Material {Name} does not have a property named {name}");
    }

    #endregion
    
    
    private bool HasVariable(string name) => Shader.IsAvailable && Shader.Res!.HasVariable(name);
}