using KorpiEngine.Core.API.Rendering.Shaders;
using KorpiEngine.Core.API.Rendering.Textures;
using KorpiEngine.Core.Internal.AssetManagement;
using KorpiEngine.Core.Rendering;

namespace KorpiEngine.Core.API.Rendering.Materials;

/// <summary>
/// A material used for rendering.
/// Objects with a similar material may be batched together for rendering.
/// Uses shader preprocessor-based permutations, over a uniform-based branching system.
/// </summary>
// https://www.reddit.com/r/GraphicsProgramming/comments/7llloo/comment/drnyosg/?utm_source=share&utm_medium=web3x&utm_name=web3xcss&utm_term=1&utm_content=share_button
// https://github.com/michaelsakharov/Prowl/blob/main/Prowl.Runtime/Resources/Material.cs#L140
public sealed class Material : EngineObject
{
    public const string DEFAULT_COLOR_PROPERTY = "u_MainColor";
    public readonly AssetRef<Shader> Shader;
    public readonly MaterialPropertyBlock PropertyBlock;

    // Key is Shader.GUID + "-" + keywords + "-" + Shader.globalKeywords
    private static readonly Dictionary<string, Shader.CompiledShader> PassVariants = new();
    private readonly HashSet<string> _keywords = new();

    public int PassCount => Shader.IsAvailable ? GetVariant(_keywords.ToArray()).Passes.Length : 0;


    public Material(AssetRef<Shader> shader)
    {
        if (shader.AssetID == Guid.Empty)
            throw new ArgumentNullException(nameof(shader));
        Shader = shader;
        PropertyBlock = new MaterialPropertyBlock();
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
        _keywords.Add(key);
    }


    public void DisableKeyword(string? keyword)
    {
        string? key = keyword?.ToUpper().Replace(" ", "").Replace(";", "");
        if (string.IsNullOrWhiteSpace(key))
            return;
        _keywords.Remove(key);
    }


    public bool IsKeywordEnabled(string keyword) => _keywords.Contains(keyword.ToUpper().Replace(" ", "").Replace(";", ""));

    #endregion


    #region PASSES

    public void SetPass(int pass, bool applyProperties = false)
    {
        if (!Shader.IsAvailable)
            return;
        
        Shader.CompiledShader shader = GetVariant(_keywords.ToArray());

        // Make sure we have a valid pass
        if (pass < 0 || pass >= shader.Passes.Length)
            return;

        InternalSetPass(shader.Passes[pass], applyProperties);
    }


    public void SetShadowPass(bool applyProperties = false)
    {
        if (!Shader.IsAvailable)
            return;
        
        Shader.CompiledShader shader = GetVariant(_keywords.ToArray());
        InternalSetPass(shader.ShadowPass, applyProperties);
    }


    private void InternalSetPass(Shader.CompiledShader.Pass pass, bool apply = false)
    {
        // Set the shader
        Graphics.Driver.SetState(pass.State);
        Graphics.Driver.BindProgram(pass.Program);

        if (apply)
            PropertyBlock.Apply(Graphics.Driver.CurrentProgram!);
    }

    #endregion


    #region VARIANT COMPILATION

    private Shader.CompiledShader GetVariant(string[] allKeywords)
    {
        if (Shader.IsAvailable == false)
            throw new Exception("Cannot compile without a valid shader assigned");

        string keywords = string.Join("-", allKeywords);
        string key = Shader.Res!.InstanceID + "-" + keywords + "-" + Shaders.Shader.GlobalKeywords;
        if (PassVariants.TryGetValue(key, out Shader.CompiledShader s))
            return s;

        // Add each global together making sure to not add duplicates
        string[] globals = Shaders.Shader.GlobalKeywords.ToArray();
        foreach (string globalKeyword in globals)
        {
            if (string.IsNullOrWhiteSpace(globalKeyword))
                continue;
            
            if (allKeywords.Contains(globalKeyword, StringComparer.OrdinalIgnoreCase))
                continue;
            
            allKeywords = allKeywords.Append(globalKeyword).ToArray();
        }

        // Remove empty keywords
        allKeywords = allKeywords.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

        // Compile Each Pass
        Shader.CompiledShader compiledPasses = Shader.Res!.Compile(allKeywords);

        PassVariants[key] = compiledPasses;
        return compiledPasses;
    }

    #endregion


    #region PROPERTY SETTERS

    public void SetColor(string name, Color value) => PropertyBlock.SetColor(name, value);
    public void SetVector(string name, Vector2 value) => PropertyBlock.SetVector(name, value);
    public void SetVector(string name, Vector3 value) => PropertyBlock.SetVector(name, value);
    public void SetVector(string name, Vector4 value) => PropertyBlock.SetVector(name, value);
    public void SetFloat(string name, float value) => PropertyBlock.SetFloat(name, value);
    public void SetInt(string name, int value) => PropertyBlock.SetInt(name, value);
    public void SetMatrix(string name, Matrix4x4 value) => PropertyBlock.SetMatrix(name, value);
    public void SetMatrices(string name, IEnumerable<System.Numerics.Matrix4x4> value) => PropertyBlock.SetMatrices(name, value);
    public void SetTexture(string name, Texture2D value) => PropertyBlock.SetTexture(name, value);
    public void SetTexture(string name, AssetRef<Texture2D> value) => PropertyBlock.SetTexture(name, value);

    #endregion
}