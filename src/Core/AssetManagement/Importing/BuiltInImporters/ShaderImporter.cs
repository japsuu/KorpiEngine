using System.Text;
using System.Text.RegularExpressions;
using KorpiEngine.Rendering;
using KorpiEngine.Utils;

namespace KorpiEngine.AssetManagement;

[AssetImporter(".kshader")]
internal partial class ShaderImporter : AssetImporter
{
    private sealed class ShaderParseException : KorpiException
    {
        public ShaderParseException(string? message) : base(message) { }
        public ShaderParseException(string? message, Exception? innerException) : base(message, innerException) { }
    }
    
    
    private static readonly Regex PreprocessorIncludeRegex = GenerateRegex();
    private static readonly List<string> ImportErrors = [];
    private static FileInfo? currentAssetPath { get; set; }

    [GeneratedRegex("""^\s*#include\s*["<](.+?)[">]\s*$""", RegexOptions.Multiline)]
    private static partial Regex GenerateRegex();
    
    
    public override AssetInstance? Import(FileInfo assetPath)
    {
        currentAssetPath = assetPath;
        ImportErrors.Clear();
        string shaderScript = File.ReadAllText(assetPath.FullName);

        // Strip out comments and Multi-like Comments
        shaderScript = ClearAllComments(shaderScript);

        // Parse the shader
        ParsedShader parsedShader = ParseShader(shaderScript);
        
        if (ImportErrors.Count > 0)
        {
            StringBuilder sb = new();
            sb.AppendLine("Shader importing failed because of the following errors:");
            foreach (string error in ImportErrors)
                sb.AppendLine(error);
            Application.Logger.Error(sb.ToString());
            return null;
        }

        // Sort passes to be in order
        parsedShader.Passes = parsedShader.Passes.OrderBy(p => p.Order).ToArray();

        // Create normal shader passes
        List<Shader.ShaderPass> passes = [];
        foreach (ParsedShaderPass parsedPass in parsedShader.Passes)
        {
            ShaderSourceDescriptor vertexDescriptor = new(ShaderType.Vertex, parsedPass.Vertex);
            ShaderSourceDescriptor fragDescriptor = new(ShaderType.Fragment, parsedPass.Fragment);
            passes.Add(new Shader.ShaderPass(parsedPass.State, vertexDescriptor, fragDescriptor));
        }

        // Create shadow pass
        Shader.ShaderPass? shadowPass = null;
        if (parsedShader.ShadowPass != null)
        {
            ShaderSourceDescriptor shadowVertexDescriptor = new(ShaderType.Vertex, parsedShader.ShadowPass.Vertex);
            ShaderSourceDescriptor shadowFragDescriptor = new(ShaderType.Fragment, parsedShader.ShadowPass.Fragment);
            shadowPass = new Shader.ShaderPass(parsedShader.ShadowPass.State, shadowVertexDescriptor, shadowFragDescriptor);
        }

        Shader shader = new(parsedShader.Name, parsedShader.Properties, passes, shadowPass);
        return shader;
    }


    public static ParsedShader ParseShader(string input)
    {
        ParsedShader shader = new(
            ParseShaderName(input),
            ParseProperties(input),
            ParsePasses(input),
            ParseShadowPass(input));

        return shader;
    }


    private static string ParseShaderName(string input)
    {
        Match match = Regex.Match(input, @"Shader\s+""([^""]+)""");
        if (!match.Success)
            throw new ShaderParseException("Malformed input: Missing Shader declaration");
        return match.Groups[1].Value;
    }


    private static List<Shader.Property> ParseProperties(string input)
    {
        List<Shader.Property> propertiesList = [];

        Match propertiesBlockMatch = Regex.Match(input, @"Properties\s*{([^{}]*?)}", RegexOptions.Singleline);
        
        if (!propertiesBlockMatch.Success)
            return propertiesList;
        
        string propertiesBlock = propertiesBlockMatch.Groups[1].Value;

        MatchCollection propertyMatches = Regex.Matches(propertiesBlock, @"(\w+)\s*\(\""([^\""]+)\"".*?,\s*(\w+)");
        foreach (Match match in propertyMatches)
        {
            Shader.Property property = new(match.Groups[1].Value, match.Groups[2].Value, ParsePropertyType(match.Groups[3].Value));
            propertiesList.Add(property);
            Application.Logger.Debug($"Discovered property: {property.Name} ({property.DisplayName}) of type {property.Type}");
        }

        return propertiesList;
    }


    private static Shader.Property.PropertyType ParsePropertyType(string typeStr)
    {
        try
        {
            return (Shader.Property.PropertyType)Enum.Parse(typeof(Shader.Property.PropertyType), typeStr, true);
        }
        catch (ArgumentException)
        {
            throw new ArgumentException($"Unknown shader property type: {typeStr}");
        }
    }


    private static ParsedShaderPass[] ParsePasses(string input)
    {
        List<ParsedShaderPass> passesList = [];

        MatchCollection passMatches = Regex.Matches(input, @"\bPass (\d+)\s+({(?:[^{}]|(?<o>{)|(?<-o>}))+(?(o)(?!))})");
        foreach (Match passMatch in passMatches)
        {
            string passContent = passMatch.Groups[2].Value;

            ParsedShaderPass shaderPass = new(
                ParseRasterState(passContent),
                int.Parse(passMatch.Groups[1].Value),
                ParseBlockContent(passContent, "Vertex"),
                ParseBlockContent(passContent, "Fragment"));

            shaderPass.Vertex = PreprocessorIncludeRegex.Replace(shaderPass.Vertex, ImportReplacer);
            shaderPass.Fragment = PreprocessorIncludeRegex.Replace(shaderPass.Fragment, ImportReplacer);

            passesList.Add(shaderPass);
        }

        return passesList.ToArray();
    }


    private static ParsedShaderShadowPass? ParseShadowPass(string input)
    {
        MatchCollection passMatches = Regex.Matches(input, @"ShadowPass (\d+)\s+({(?:[^{}]|(?<o>{)|(?<-o>}))+(?(o)(?!))})");
        if (passMatches.Count == 0)
            return null;    // No shadow pass found
        
        if (passMatches.Count > 1)
            Application.Logger.Warn("Multiple shadow passes found, only the first one will be used");
        
        Match passMatch = passMatches[0];
        
        string passContent = passMatch.Groups[2].Value;
        ParsedShaderShadowPass shaderPass = new(
            ParseRasterState(passContent),
            ParseBlockContent(passContent, "Vertex"),
            ParseBlockContent(passContent, "Fragment"));
            
        return shaderPass; // Return the first one, any other ones are ignored
    }


    private static string ParseBlockContent(string input, string blockName)
    {
        Match blockMatch = Regex.Match(input, $@"{blockName}\s*({{(?:[^{{}}]|(?<o>{{)|(?<-o>}}))+(?(o)(?!))}})");

        if (!blockMatch.Success)
            return "";
        
        string content = blockMatch.Groups[1].Value;

        // Strip off the enclosing braces and return
        return content.Substring(1, content.Length - 2).Trim();

    }


    private static RasterizerState ParseRasterState(string passContent)
    {
        RasterizerState rasterState = new();

        if (GetStateValue(passContent, "DepthTest", out string depthTest))
            rasterState.EnableDepthTest = ConvertToBoolean(depthTest);

        if (GetStateValue(passContent, "DepthWrite", out string depthWrite))
            rasterState.EnableDepthWrite = ConvertToBoolean(depthWrite);

        if (GetStateValue(passContent, "DepthMode", out string depthMode))
            rasterState.DepthMode = (DepthMode)Enum.Parse(typeof(DepthMode), depthMode, true);

        if (GetStateValue(passContent, "Blend", out string blend))
            rasterState.EnableBlend = ConvertToBoolean(blend);

        if (GetStateValue(passContent, "BlendSrc", out string blendSrc))
            rasterState.BlendSrc = (BlendType)Enum.Parse(typeof(BlendType), blendSrc, true);

        if (GetStateValue(passContent, "BlendDst", out string blendDst))
            rasterState.BlendDst = (BlendType)Enum.Parse(typeof(BlendType), blendDst, true);

        if (GetStateValue(passContent, "BlendMode", out string blendEquation))
            rasterState.BlendMode = (BlendMode)Enum.Parse(typeof(BlendMode), blendEquation, true);

        if (GetStateValue(passContent, "Cull", out string cull))
            rasterState.EnableCulling = ConvertToBoolean(cull);

        if (GetStateValue(passContent, "CullFace", out string cullFace))
            rasterState.FaceCulling = (PolyFace)Enum.Parse(typeof(PolyFace), cullFace, true);

        if (GetStateValue(passContent, "Winding", out string winding))
            rasterState.WindingOrder = (WindingOrder)Enum.Parse(typeof(WindingOrder), winding, true);

        return rasterState;
    }


    private static bool GetStateValue(string passContent, string name, out string value)
    {
        Match windingMatch = Regex.Match(passContent, name + @"\s+(\w+)");
        value = "";
        
        if (!windingMatch.Success)
            return false;
        
        value = windingMatch.Groups[1].Value;
        return true;
    }


    // Convert string ("false", "0", "off", "no") or ("true", "1", "on", "yes") to boolean
    private static bool ConvertToBoolean(string input)
    {
        input = input.Trim();
        input = input.ToLower();
        return input.Equals("true", StringComparison.OrdinalIgnoreCase) ||
               input.Equals("1", StringComparison.OrdinalIgnoreCase) ||
               input.Equals("on", StringComparison.OrdinalIgnoreCase) ||
               input.Equals("yes", StringComparison.OrdinalIgnoreCase);
    }


    public static string ClearAllComments(string input)
    {
        // Remove single-line comments
        string noSingleLineComments = Regex.Replace(input, "//.*", "");

        // Remove multi-line comments
        string noComments = Regex.Replace(noSingleLineComments, @"/\*.*?\*/", "", RegexOptions.Singleline);

        return noComments;
    }


    private static string ImportReplacer(Match match)
    {
        string relativePath = match.Groups[1].Value + ".glsl";

        // First check the Defaults path
        FileInfo file = new(Path.Combine(Application.DefaultsDirectory, relativePath));
        if (!file.Exists)
            file = new FileInfo(Path.Combine(currentAssetPath!.Directory!.FullName, relativePath));

        if (!file.Exists)
        {
            ImportErrors.Add($"Include not found: {file.FullName}");
            return "";
        }

        // Recursively handle Imports
        string includeScript = PreprocessorIncludeRegex.Replace(File.ReadAllText(file.FullName), ImportReplacer);

        // Strip out comments and Multi-like Comments
        includeScript = ClearAllComments(includeScript);
        return includeScript;
    }


    public class ParsedShader
    {
        public readonly string Name;
        public readonly List<Shader.Property> Properties;
        public ParsedShaderPass[] Passes;
        public readonly ParsedShaderShadowPass? ShadowPass;


        public ParsedShader(string name, List<Shader.Property> properties, ParsedShaderPass[] passes, ParsedShaderShadowPass? shadowPass)
        {
            Name = name;
            Properties = properties;
            Passes = passes;
            ShadowPass = shadowPass;
        }
    }

    public class ParsedShaderPass
    {
        public readonly RasterizerState State;
        public readonly int Order;
        public string Vertex;
        public string Fragment;


        public ParsedShaderPass(RasterizerState state, int order, string vertex, string fragment)
        {
            State = state;
            Order = order;
            Vertex = vertex;
            Fragment = fragment;
        }
    }

    public class ParsedShaderShadowPass
    {
        public readonly RasterizerState State;
        public readonly string Vertex;
        public readonly string Fragment;


        public ParsedShaderShadowPass(RasterizerState state, string vertex, string fragment)
        {
            State = state;
            Vertex = vertex;
            Fragment = fragment;
        }
    }
}