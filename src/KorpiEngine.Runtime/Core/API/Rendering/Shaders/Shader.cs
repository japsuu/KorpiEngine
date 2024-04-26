using KorpiEngine.Core.Internal.Assets;
using KorpiEngine.Core.Internal.Serialization;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Rendering.Primitives;
using KorpiEngine.Core.Rendering.Shaders;

namespace KorpiEngine.Core.API.Rendering.Shaders;

/// <summary>
/// The Shader class itself doesn't do much, It stores the properties of the shader and the shader code and Keywords.
/// This is used in conjunction with the Material class to create shader variants with the correct keywords and to render things
/// </summary>
public sealed class Shader : EngineObject
{
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

    public class ShaderPass
    {
        public readonly RasterizerState State;
        public readonly List<ShaderSourceDescriptor> Shaders;


        public ShaderPass(RasterizerState state, List<ShaderSourceDescriptor> shaders)
        {
            State = state;
            Shaders = shaders;
        }
    }

    public class ShaderShadowPass
    {
        public readonly RasterizerState State;
        public readonly List<ShaderSourceDescriptor> Shaders;


        public ShaderShadowPass(RasterizerState state, List<ShaderSourceDescriptor> shaders)
        {
            State = state;
            Shaders = shaders;
        }
    }

    internal struct CompiledShader
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

        public Pass[] Passes;
        public Pass ShadowPass;


        public CompiledShader(Pass[] passes, Pass shadowPass)
        {
            Passes = passes;
            ShadowPass = shadowPass;
        }
    }

    
    internal static readonly HashSet<string> GlobalKeywords = new();

    public readonly List<Property> Properties = new();
    public readonly List<ShaderPass> Passes = new();
    public ShaderShadowPass? ShadowPass;


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
            CompiledShader.Pass[] compiledPasses = new CompiledShader.Pass[Passes.Count];
            for (int i = 0; i < Passes.Count; i++)
            {
                string frag = Passes[i].Fragment;
                string vert = Passes[i].Vertex;
                try
                {
                    PrepareShaderSource(ref frag, ref vert, defines);
                    GraphicsProgram program = Graphics.Driver.CompileProgram(frag, vert, "");
                    compiledPasses[i] = new CompiledShader.Pass(Passes[i].State, program);
                }
                catch (Exception e)
                {
                    Application.Logger.Error("Shader compilation failed using fallback shader, Reason: " + e.Message);

                    // We Assume Invalid exists, if it doesn't we have a bigger problem
                    AssetRef<Shader> fallback = Find("Defaults/Invalid.shader");
                    frag = fallback.Res!.Passes[0].Fragment;
                    vert = fallback.Res!.Passes[0].Vertex;
                    PrepareShaderSource(ref frag, ref vert, defines);
                    compiledPasses[i] = new CompiledShader.Pass(new RasterizerState(), Graphics.Driver.CompileProgram(frag, vert, ""));
                }
            }

            CompiledShader compiledShader = new()
            {
                Passes = compiledPasses
            };

            if (ShadowPass != null)
            {
                string frag = ShadowPass.Fragment;
                string vert = ShadowPass.Vertex;
                PrepareShaderSource(ref frag, ref vert, defines);
                GraphicsProgram program = Graphics.Driver.CompileProgram(frag, vert, "");
                compiledShader.ShadowPass = new CompiledShader.Pass(ShadowPass.State, program);
            }
            else
            {
                // We Assume Depth exists, if it doesn't we have a bigger problem
                AssetRef<Shader> depth = Find("Defaults/Depth.shader");
                string frag = depth.Res!.Passes[0].Fragment;
                string vert = depth.Res!.Passes[0].Vertex;
                PrepareShaderSource(ref frag, ref vert, defines);
                compiledShader.ShadowPass = new CompiledShader.Pass(new RasterizerState(), Graphics.Driver.CompileProgram(frag, vert, ""));
            }

            return compiledShader;
        }
        catch (Exception e)
        {
            Application.Logger.Error("Failed to compile shader: " + Name + " Reason: " + e.Message);
            return new CompiledShader();
        }
    }


    private void PrepareShaderSource(ref string source, IEnumerable<string> defines)
    {
        if (string.IsNullOrWhiteSpace(source))
            throw new Exception($"Failed to compile shader pass of {Name}. Shader source is null or empty.");

        // Default Defines
        source = source.Insert(0, $"#define {EngineConstants.DEFAULT_SHADER_DEFINE}\n");

        // Insert keywords
        foreach (string define in defines)
        {
            source = source.Insert(0, $"#define {define}\n");
        }

        // Insert the GLSL version at the start
        source = source.Insert(0, "#version 420\n");
    }


    private Shader()
    {
        
    }


    public static AssetRef<Shader> Find(string path)
    {
        return Application.AssetProvider.LoadAsset<Shader>(path);
    }


    public SerializedProperty Serialize(Serializer.SerializationContext ctx)
    {
        SerializedProperty compoundTag = SerializedProperty.NewCompound();
        compoundTag.Add("Name", new SerializedProperty(Name));

        if (AssetID != Guid.Empty)
        {
            compoundTag.Add("AssetID", new SerializedProperty(AssetID.ToString()));
            if (FileID != 0)
                compoundTag.Add("FileID", new SerializedProperty(FileID));
        }

        SerializedProperty propertiesTag = SerializedProperty.NewList();
        foreach (Property property in Properties)
        {
            SerializedProperty propertyTag = SerializedProperty.NewCompound();
            propertyTag.Add("Name", new SerializedProperty(property.Name));
            propertyTag.Add("DisplayName", new SerializedProperty(property.DisplayName));
            propertyTag.Add("Type", new SerializedProperty((byte)property.Type));
            propertiesTag.ListAdd(propertyTag);
        }

        compoundTag.Add("Properties", propertiesTag);
        SerializedProperty passesTag = SerializedProperty.NewList();
        foreach (ShaderPass pass in Passes)
        {
            SerializedProperty passTag = SerializedProperty.NewCompound();
            passTag.Add("State", Serializer.Serialize(pass.State, ctx));
            passTag.Add("Vertex", new(pass.Vertex));
            passTag.Add("Fragment", new(pass.Fragment));
            passesTag.ListAdd(passTag);
        }

        compoundTag.Add("Passes", passesTag);
        if (ShadowPass != null)
        {
            SerializedProperty shadowPassTag = SerializedProperty.NewCompound();
            shadowPassTag.Add("State", Serializer.Serialize(ShadowPass.State, ctx));
            shadowPassTag.Add("Vertex", new(ShadowPass.Vertex));
            shadowPassTag.Add("Fragment", new(ShadowPass.Fragment));
            compoundTag.Add("ShadowPass", shadowPassTag);
        }

        return compoundTag;
    }


    public void Deserialize(SerializedProperty value, Serializer.SerializationContext ctx)
    {
        Name = value.Get("Name")?.StringValue!;

        if (value.TryGet("AssetID", out SerializedProperty? assetIDTag))
        {
            AssetID = Guid.Parse(assetIDTag!.StringValue);
            FileID = value.Get("FileID")!.UShortValue;
        }

        Properties.Clear();
        SerializedProperty? propertiesTag = value.Get("Properties");
        foreach (SerializedProperty propertyTag in propertiesTag!.List)
        {
            Property property = new()
            {
                Name = propertyTag.Get("Name")!.StringValue,
                DisplayName = propertyTag.Get("DisplayName")!.StringValue,
                Type = (Property.PropertyType)propertyTag.Get("Type")!.ByteValue
            };
            Properties.Add(property);
        }

        Passes.Clear();
        SerializedProperty? passesTag = value.Get("Passes");
        foreach (SerializedProperty passTag in passesTag!.List)
        {
            RasterizerState state = Serializer.Deserialize<RasterizerState>(passTag.Get("State")!, ctx);
            ShaderPass pass = new(state);
            pass.Vertex = passTag.Get("Vertex")!.StringValue;
            pass.Fragment = passTag.Get("Fragment")!.StringValue;
            Passes.Add(pass);
        }

        if (value.TryGet("ShadowPass", out SerializedProperty? shadowPassTag))
        {
            ShaderShadowPass shadowPass = new()
            {
                State = Serializer.Deserialize<RasterizerState>(shadowPassTag!.Get("State")!, ctx)
            };
            shadowPass.Vertex = shadowPassTag.Get("Vertex")!.StringValue;
            shadowPass.Fragment = shadowPassTag.Get("Fragment")!.StringValue;
            ShadowPass = shadowPass;
        }
    }
}