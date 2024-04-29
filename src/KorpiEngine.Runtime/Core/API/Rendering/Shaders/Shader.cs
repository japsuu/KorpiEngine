﻿using KorpiEngine.Core.API.AssetManagement;
using KorpiEngine.Core.Internal.AssetManagement;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Rendering.Primitives;

namespace KorpiEngine.Core.API.Rendering.Shaders;

/// <summary>
/// The Shader class itself doesn't do much, It stores the properties of the shader and the shader code and Keywords.
/// This is used in conjunction with the Material class to create shader variants with the correct keywords and to render things
/// </summary>
public sealed class Shader : EngineObject
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
            TEXTURE_2D
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
    
    internal static readonly HashSet<string> GlobalKeywords = new();

    public readonly List<Property> Properties;  //TODO: Use to detect properties in the shader and set them in the material.
    public readonly List<ShaderPass> Passes;
    public readonly ShaderPass? ShadowPass;


    internal Shader(string name, List<Property> properties, List<ShaderPass> passes, ShaderPass? shadowPass = null)
    {
        Name = name;
        Properties = properties;
        Passes = passes;
        ShadowPass = shadowPass;
    }


    public static AssetRef<Shader> Find(string path)
    {
        return AssetDatabase.LoadAsset<Shader>(path) ?? throw new InvalidOperationException($"Failed to load shader: {path}");
    }


    #region GLOBAL KEYWORDS

    public static void EnableKeyword(string keyword)
    {
        keyword = keyword.ToLower().Replace(" ", "").Replace(";", "");
        GlobalKeywords.Add(keyword);
    }


    public static void DisableKeyword(string keyword)
    {
        keyword = keyword.ToUpper().Replace(" ", "").Replace(";", "");
        GlobalKeywords.Remove(keyword);
    }


    public static bool IsKeywordEnabled(string keyword) => GlobalKeywords.Contains(keyword.ToLower().Replace(" ", "").Replace(";", ""));

    #endregion


    internal CompiledShader Compile(string[] defines)
    {
        try
        {
            // Compile the normal passes
            CompiledShader.Pass[] compiledPasses = new CompiledShader.Pass[Passes.Count];
            for (int i = 0; i < Passes.Count; i++)
            {
                try
                {
                    List<ShaderSourceDescriptor> sources = PrepareShaderPass(Passes[i], defines);
                    compiledPasses[i] = new CompiledShader.Pass(Passes[i].State, Graphics.Driver.CompileProgram(sources));
                }
                catch (Exception e)
                {
                    Application.Logger.Error($"Shader compilation failed, using fallback shader. Reason: {e.Message}");
                    
                    AssetRef<Shader> fallback = Find("Defaults/Invalid.shader");
                    List<ShaderSourceDescriptor> sources = PrepareShaderPass(fallback.Res!.Passes[0], defines);
                    compiledPasses[i] = new CompiledShader.Pass(new RasterizerState(), Graphics.Driver.CompileProgram(sources));
                }
            }

            // Compile the shadow pass
            CompiledShader.Pass compiledShadowPass;
            if (ShadowPass != null)
            {
                List<ShaderSourceDescriptor> sources = new();
                foreach (ShaderSourceDescriptor d in ShadowPass.ShadersSources)
                {
                    string source = d.Source;
                    PrepareShaderSource(ref source, defines);
                    sources.Add(new ShaderSourceDescriptor(d.Type, source));
                }
                compiledShadowPass = new CompiledShader.Pass(ShadowPass.State, Graphics.Driver.CompileProgram(sources));
            }
            else
            {
                AssetRef<Shader> depth = Find("Defaults/Depth.shader");
                List<ShaderSourceDescriptor> sources = PrepareShaderPass(depth.Res!.Passes[0], defines);
                compiledShadowPass = new CompiledShader.Pass(new RasterizerState(), Graphics.Driver.CompileProgram(sources));
            }

            // Return the compiled shader
            CompiledShader compiledShader = new(compiledPasses, compiledShadowPass);
            return compiledShader;
        }
        catch (Exception e)
        {
            Application.Logger.Error("Failed to compile shader: " + Name + " Reason: " + e.Message);
            return new CompiledShader();
        }
    }


    private List<ShaderSourceDescriptor> PrepareShaderPass(ShaderPass pass, string[] defines)
    {
        List<ShaderSourceDescriptor> sources = new();
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
        //TODO: Handle includes.
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