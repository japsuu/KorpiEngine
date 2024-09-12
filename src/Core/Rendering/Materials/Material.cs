﻿using KorpiEngine.AssetManagement;
using KorpiEngine.Mathematics;
using KorpiEngine.Utils;

namespace KorpiEngine.Rendering;

/// <summary>
/// A material used for rendering.
/// Objects with a similar material may be batched together for rendering.
/// Uses shader preprocessor-based permutations over a uniform-based branching system.
/// </summary>
// https://www.reddit.com/r/GraphicsProgramming/comments/7llloo/comment/drnyosg/?utm_source=share&utm_medium=web3x&utm_name=web3xcss&utm_term=1&utm_content=share_button
// https://github.com/michaelsakharov/Prowl/blob/main/Prowl.Runtime/Resources/Material.cs#L140
public sealed class Material : Asset
{
    private static AssetReference<Texture2D> defaultAlbedoTex;
    private static AssetReference<Texture2D> defaultNormalTex;
    private static AssetReference<Texture2D> defaultSurfaceTex;
    private static AssetReference<Texture2D> defaultEmissionTex;
    private static AssetReference<Material> invalidMaterial;
    
    public const string MAIN_TEX = "_MainTex";
    public const string NORMAL_TEX = "_NormalTex";
    public const string SURFACE_TEX = "_SurfaceTex";
    public const string EMISSION_TEX = "_EmissionTex";
    
    public static Texture2D DefaultAlbedoTex => defaultAlbedoTex.Asset!;
    public static Texture2D DefaultNormalTex => defaultNormalTex.Asset!;
    public static Texture2D DefaultSurfaceTex => defaultSurfaceTex.Asset!;
    public static Texture2D DefaultEmissionTex => defaultEmissionTex.Asset!;
    public static Material InvalidMaterial => invalidMaterial.Asset!;
    
    
    internal static void LoadDefaults()
    {
        defaultAlbedoTex = AssetManager.LoadAssetFile<Texture2D>("Assets/Defaults/default_albedo.png").CreateReference();
        defaultNormalTex = AssetManager.LoadAssetFile<Texture2D>("Assets/Defaults/default_normal.png").CreateReference();
        defaultSurfaceTex = AssetManager.LoadAssetFile<Texture2D>("Assets/Defaults/default_surface.png").CreateReference();
        defaultEmissionTex = AssetManager.LoadAssetFile<Texture2D>("Assets/Defaults/default_emission.png").CreateReference();
        invalidMaterial = new Material(AssetManager.LoadAssetFile<Shader>("Assets/Defaults/Invalid.kshader"), "invalid material", false).CreateReference();
    }
    
    
    internal static void UnloadDefaults()
    {
        defaultAlbedoTex.Release();
        defaultNormalTex.Release();
        defaultSurfaceTex.Release();
        defaultEmissionTex.Release();
        invalidMaterial.Release();
    }
    
    
    // Key is Shader.GUID + "-" + keywords + "-" + Shader.globalKeywords
    private static readonly Dictionary<string, Shader.CompiledShader> PassVariants = new();
    private readonly SortedSet<string> _materialKeywords = [];
    private readonly MaterialPropertyBlock _propertyBlock;
    private AssetReference<Shader> _shader;
    private int _lastHash = -1;
    private int _lastGlobalKeywordsVersion = -1;
    private string _allKeywordsString = "";

    public Shader Shader => _shader.Asset!;
    public int PassCount => _shader.IsAlive ? GetCompiledVariant().Passes.Length : 0;


    public Material(Shader shader, string name, bool setDefaultTextures = true) : base(name)
    {
        _shader = shader.CreateReference();
        _propertyBlock = new MaterialPropertyBlock();
        
        if (setDefaultTextures)
            SetDefaultTextures();
    }
    
    
    public void SetDefaultTextures()
    {
        SetAlbedoDefault();
        SetNormalDefault();
        SetSurfaceDefault();
        SetEmissionDefault();
    }
    public void SetAlbedoDefault() => SetTexture(MAIN_TEX, DefaultAlbedoTex);
    public void SetNormalDefault() => SetTexture(NORMAL_TEX, DefaultNormalTex);
    public void SetSurfaceDefault() => SetTexture(SURFACE_TEX, DefaultSurfaceTex);
    public void SetEmissionDefault() => SetTexture(EMISSION_TEX, DefaultEmissionTex);
    
    
    internal void ApplyPropertyBlock(GraphicsProgram shader)
    {
        _propertyBlock.Apply(shader, Name);
    }


    #region KEYWORDS
#warning TODO: Print warning if keywords are set after SetPass has been called. SetPass should be called after all keywords are set, since it compiles the shader.
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
        if (!_shader.IsAlive)
            return;
        
        Shader.CompiledShader shader = GetCompiledVariant();

        // Make sure we have a valid pass
        if (pass < 0 || pass >= shader.Passes.Length)
            return;

        InternalSetPass(shader.Passes[pass], applyProperties);
    }


    public void SetShadowPass(bool applyProperties = false)
    {
        if (!_shader.IsAlive)
            return;
        
        Shader.CompiledShader shader = GetCompiledVariant();
        InternalSetPass(shader.ShadowPass, applyProperties);
    }


    private void InternalSetPass(Shader.CompiledShader.Pass pass, bool apply = false)
    {
        // Set the shader
        Graphics.Device.SetState(pass.State);
        Graphics.Device.BindProgram(pass.Program);

        if (apply)
            ApplyPropertyBlock(Graphics.Device.CurrentProgram!);
    }

    #endregion


    #region VARIANT COMPILATION
#warning TODO: Fix shader complied variant naming
    private Shader.CompiledShader GetCompiledVariant()
    {
        if (!_shader.IsAlive)
            throw new InvalidOperationException("Cannot compile without a valid shader assigned");
        
        int currentHash = Hashing.GetAdditiveHashCode(_materialKeywords);

        bool globalKeywordsChanged = _lastGlobalKeywordsVersion != Shader.GlobalKeywordsVersion;
        bool materialKeywordsChanged = currentHash != _lastHash;
        if (globalKeywordsChanged || materialKeywordsChanged)
        {
            string materialKeywordsString = string.Join("-", _materialKeywords);
            _allKeywordsString = $"{Shader.InstanceID}-{materialKeywordsString}-{Shader.GlobalKeywordsString}";
            _lastGlobalKeywordsVersion = Shader.GlobalKeywordsVersion;
            _lastHash = currentHash;
        }

        if (PassVariants.TryGetValue(_allKeywordsString, out Shader.CompiledShader s))
            return s;

        // Create a collection of all keywords
        List<string> allKeywords = [];
        
        // Add all global keywords
        allKeywords.AddRange(Shader.GetGlobalKeywords());
        
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
        Shader.CompiledShader compiledPasses = Shader.Compile(allKeywords.ToArray());

        PassVariants[_allKeywordsString] = compiledPasses;
        return compiledPasses;
    }

    #endregion


    #region PROPERTY SETTERS

    public void SetColor(string name, ColorHDR value, bool allowFail = false)
    {
        if (HasVariable(name))
            _propertyBlock.SetColor(name, value);
        else if (!allowFail)
            Application.Logger.Warn($"Material {Name} does not have a property named {name}");
    }


    public void SetVector(string name, Vector2 value, bool allowFail = false)
    {
        if (HasVariable(name))
            _propertyBlock.SetVector2(name, value);
        else if (!allowFail)
            Application.Logger.Warn($"Material {Name} does not have a property named {name}");
    }


    public void SetVector(string name, Vector3 value, bool allowFail = false)
    {
        if (HasVariable(name))
            _propertyBlock.SetVector3(name, value);
        else if (!allowFail)
            Application.Logger.Warn($"Material {Name} does not have a property named {name}");
    }


    public void SetVector(string name, Vector4 value, bool allowFail = false)
    {
        if (HasVariable(name))
            _propertyBlock.SetVector4(name, value);
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


    public void SetMatrices(string name, IEnumerable<Matrix4x4> value, bool allowFail = false)
    {
        if (HasVariable(name))
            _propertyBlock.SetMatrices(name, value);
        else if (!allowFail)
            Application.Logger.Warn($"Material {Name} does not have a property named {name}");
    }


    public void SetTexture(string name, Texture2D? value, bool allowFail = false)
    {
        if (HasVariable(name))
            _propertyBlock.SetTexture(name, value);
        else if (!allowFail)
            Application.Logger.Warn($"Material {Name} does not have a property named {name}");
    }

    #endregion
    
    
    private bool HasVariable(string name) => _shader.IsAlive && Shader.HasVariable(name);

    protected override void OnDispose(bool manual)
    {
        _propertyBlock.Dispose();
        _shader.Release();
    }
}