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
    public const string DEFAULT_COLOR_PROPERTY = "_MainColor";
    public const string DEFAULT_DIFFUSE_TEX_PROPERTY = "_MainTex";
    public const string DEFAULT_NORMAL_TEX_PROPERTY = "_NormalTex";
    public const string DEFAULT_SURFACE_TEX_PROPERTY = "_SurfaceTex";
    public const string DEFAULT_EMISSION_TEX_PROPERTY = "_EmissionTex";
    public const string DEFAULT_EMISSION_COLOR_PROPERTY = "_EmissiveColor";
    public const string DEFAULT_EMISSION_INTENSITY_PROPERTY = "_EmissionIntensity";
    
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
        _propertyBlock.Apply(shader);
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
            _propertyBlock.Apply(Graphics.Driver.CurrentProgram!);
    }

    #endregion


    #region VARIANT COMPILATION

    private Shader.CompiledShader GetCompiledVariant()
    {
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

    #endregion


    #region PROPERTY SETTERS

    public void SetColor(string name, Color value)
    {
        if (HasVariable(name))
            _propertyBlock.SetColor(name, value);
    }


    public void SetVector(string name, Vector2 value)
    {
        if (HasVariable(name))
            _propertyBlock.SetVector(name, value);
    }


    public void SetVector(string name, Vector3 value)
    {
        if (HasVariable(name))
            _propertyBlock.SetVector(name, value);
    }


    public void SetVector(string name, Vector4 value)
    {
        if (HasVariable(name))
            _propertyBlock.SetVector(name, value);
    }


    public void SetFloat(string name, float value)
    {
        if (HasVariable(name))
            _propertyBlock.SetFloat(name, value);
    }


    public void SetInt(string name, int value)
    {
        if (HasVariable(name))
            _propertyBlock.SetInt(name, value);
    }


    public void SetMatrix(string name, Matrix4x4 value)
    {
        if (HasVariable(name))
            _propertyBlock.SetMatrix(name, value);
    }


    public void SetMatrices(string name, IEnumerable<System.Numerics.Matrix4x4> value)
    {
        if (HasVariable(name))
            _propertyBlock.SetMatrices(name, value);
    }


    public void SetTexture(string name, Texture2D value)
    {
        if (HasVariable(name))
            _propertyBlock.SetTexture(name, value);
    }


    public void SetTexture(string name, ResourceRef<Texture2D> value)
    {
        if (HasVariable(name))
            _propertyBlock.SetTexture(name, value);
    }

    #endregion
    
    
    private bool HasVariable(string name) => Shader.IsAvailable && Shader.Res!.HasVariable(name);
}