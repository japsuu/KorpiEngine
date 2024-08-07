﻿using KorpiEngine.Core.API.AssetManagement;
using KorpiEngine.Core.Internal.AssetManagement;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Rendering.Primitives;

namespace KorpiEngine.Core.API.Rendering.Shaders;

/// <summary>
/// The Shader class itself doesn't do much, It stores the properties of the shader and the shader code and Keywords.
/// This is used in conjunction with the Material class to create shader variants with the correct keywords and to render things
/// </summary>
public sealed class Shader : Resource
{
    /// <summary>
    /// Represents a property of a shader, used to set values in the shader.
    /// Basically a uniform.
    /// </summary>
    public class Property
    {
        public string Name = "";
        public string DisplayName = "";
        public PropertyType Type;

        public enum PropertyType
        {
            FLOAT,
            FLOAT2,
            FLOAT3,
            FLOAT4,
            COLOR,
            INT,
            INT2,
            INT3,
            INT4,
            TEXTURE_2D,
            MATRIX_4X4
        }
    }

    /// <summary>
    /// Represents a single shader pass, which is a combination of shaders that make up a shader program, and a rasterizer state.
    /// </summary>
    public class ShaderPass
    {
        public readonly RasterizerState State;
        public readonly ShaderSourceDescriptor[] ShadersSources;


        public ShaderPass(RasterizerState state, params ShaderSourceDescriptor[] shadersSources)
        {
            State = state;
            ShadersSources = shadersSources;
        }
    }

    internal readonly struct CompiledShader
    {
        public struct Pass
        {
            public readonly RasterizerState State;
            public readonly GraphicsProgram Program;


            public Pass(RasterizerState state, GraphicsProgram program)
            {
                State = state;
                Program = program;
            }
        }

        public readonly Pass[] Passes;
        public readonly Pass ShadowPass;


        public CompiledShader(Pass[] passes, Pass shadowPass)
        {
            Passes = passes;
            ShadowPass = shadowPass;
        }
    }

    internal static int GlobalKeywordsVersion;
    internal static string GlobalKeywordsString = string.Empty;
    private static readonly SortedSet<string> GlobalKeywords = [];
    private readonly List<Property> _properties;
    private readonly List<ShaderPass> _passes;
    private readonly ShaderPass? _shadowPass;


    internal Shader(string name, List<Property> properties, List<ShaderPass> passes, ShaderPass? shadowPass = null)
    {
        Name = name;
        _properties = properties;
        _passes = passes;
        _shadowPass = shadowPass;
    }


    public static ResourceRef<Shader> Find(string path)
    {
        return AssetDatabase.LoadAsset<Shader>(path) ?? throw new InvalidOperationException($"Failed to load shader: {path}");
    }
    
    
    public bool HasVariable(string name)
    {
        foreach (Property p in _properties)
        {
            if (p.Name == name)
                return true;
        }

        return false;
    }


    #region GLOBAL KEYWORDS
    
    public static IEnumerable<string> GetGlobalKeywords() => GlobalKeywords;
    

    public static void EnableKeyword(string keyword)
    {
        keyword = keyword.ToLower().Replace(" ", "").Replace(";", "");
        GlobalKeywords.Add(keyword);
        GlobalKeywordsVersion++;
        GlobalKeywordsString = string.Join("-", GlobalKeywords);
    }


    public static void DisableKeyword(string keyword)
    {
        keyword = keyword.ToUpper().Replace(" ", "").Replace(";", "");
        GlobalKeywords.Remove(keyword);
        GlobalKeywordsVersion++;
        GlobalKeywordsString = string.Join("-", GlobalKeywords);
    }


    public static bool IsKeywordEnabled(string keyword) => GlobalKeywords.Contains(keyword.ToLower().Replace(" ", "").Replace(";", ""));

    #endregion


    internal CompiledShader Compile(string[] defines)
    {
        try
        {
            // Compile the normal passes
            CompiledShader.Pass[] compiledPasses = new CompiledShader.Pass[_passes.Count];
            for (int i = 0; i < _passes.Count; i++)
            {
                try
                {
                    List<ShaderSourceDescriptor> sources = PrepareShaderPass(_passes[i], defines);
                    compiledPasses[i] = new CompiledShader.Pass(_passes[i].State, Graphics.Driver.CompileProgram(sources));
                }
                catch (Exception e)
                {
                    const string fallbackShader = "Defaults/Invalid.kshader";
                    Application.Logger.Error($"Shader compilation of '{Name}' failed, using fallback shader '{fallbackShader}'. Reason: {e.Message}");

                    ResourceRef<Shader> fallback = Find(fallbackShader);
                    List<ShaderSourceDescriptor> sources = PrepareShaderPass(fallback.Res!._passes[0], defines);
                    compiledPasses[i] = new CompiledShader.Pass(new RasterizerState(), Graphics.Driver.CompileProgram(sources));
                }
            }

            // Compile the shadow pass
            CompiledShader.Pass compiledShadowPass;
            if (_shadowPass != null)
            {
                List<ShaderSourceDescriptor> sources = [];
                foreach (ShaderSourceDescriptor d in _shadowPass.ShadersSources)
                {
                    string source = d.Source;
                    PrepareShaderSource(ref source, defines);
                    sources.Add(new ShaderSourceDescriptor(d.Type, source));
                }
                compiledShadowPass = new CompiledShader.Pass(_shadowPass.State, Graphics.Driver.CompileProgram(sources));
            }
            else
            {
                ResourceRef<Shader> depth = Find("Defaults/Depth.kshader");
                List<ShaderSourceDescriptor> sources = PrepareShaderPass(depth.Res!._passes[0], defines);
                compiledShadowPass = new CompiledShader.Pass(new RasterizerState(), Graphics.Driver.CompileProgram(sources));
            }

            // Return the compiled shader
            CompiledShader compiledShader = new(compiledPasses, compiledShadowPass);
            return compiledShader;
        }
        catch (Exception e)
        {
            Application.Logger.Error($"Failed to compile shader '{Name}'. Reason: {e.Message}");
            return new CompiledShader();
        }
    }


    private List<ShaderSourceDescriptor> PrepareShaderPass(ShaderPass pass, string[] defines)
    {
        List<ShaderSourceDescriptor> sources = [];
        foreach (ShaderSourceDescriptor d in pass.ShadersSources)
        {
            string source = d.Source;
            PrepareShaderSource(ref source, defines);
            sources.Add(new ShaderSourceDescriptor(d.Type, source));
        }

        return sources;
    }


    private void PrepareShaderSource(ref string source, IEnumerable<string> defines)
    {
        if (string.IsNullOrWhiteSpace(source))
            throw new Exception($"Failed to compile shader pass of {Name}. Shader source is null or empty.");

        // Default Defines
        source = source.Insert(0, $"#define {EngineConstants.DEFAULT_SHADER_DEFINE}\n");

        // Insert keywords
        foreach (string define in defines)
            source = source.Insert(0, $"#define {define}\n");

        // Insert the GLSL version at the start
        source = source.Insert(0, "#version 420\n");
    }
}