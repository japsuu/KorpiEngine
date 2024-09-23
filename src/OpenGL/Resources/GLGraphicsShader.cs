using System.Text.RegularExpressions;
using KorpiEngine.Rendering;
using KorpiEngine.Tools.Logging;
using OpenTK.Graphics.OpenGL4;
using ShaderType = KorpiEngine.Rendering.ShaderType;

namespace KorpiEngine.OpenGL;

/// <summary>
/// Represents an OpenGL shader object.
/// </summary>
internal class GLGraphicsShader : GraphicsObject
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(GLGraphicsShader));

    /// <summary>
    /// Specifies the type of this shader.
    /// </summary>
    public readonly ShaderType Type;

    /// <summary>
    /// Specifies a list of source filenames which are used to improve readability of the the information log in case of an error.
    /// </summary>
    public readonly List<string> SourceFiles;

    /// <summary>
    /// Used to match and replace the source filenames into the information log.
    /// </summary>
    private static readonly Regex SourceRegex = new(@"^ERROR: (\d+):", RegexOptions.Multiline);


    /// <summary>
    /// Initializes a new shader object of the given type.
    /// </summary>
    /// <param name="type"></param>
    public GLGraphicsShader(ShaderType type) : base(GL.CreateShader((OpenTK.Graphics.OpenGL4.ShaderType)type))
    {
        Type = type;
        SourceFiles = [];
    }


    protected override void DisposeResources()
    {
        GL.DeleteShader(Handle);
    }


    /// <summary>
    /// Loads the given source file and compiles the shader.
    /// </summary>
    public void CompileSource(string source)
    {
        GL.ShaderSource(Handle, source);
        GL.CompileShader(Handle);
        CheckCompileStatus(source);
    }


    /// <summary>
    /// Assert that no compile error occured.
    /// </summary>
    private void CheckCompileStatus(string source)
    {
        // check compile status
        GL.GetShader(Handle, ShaderParameter.CompileStatus, out int compileStatus);
        Logger.DebugFormat("Compile status: {0}", compileStatus);

        // check shader info log
        string? info = GL.GetShaderInfoLog(Handle);
        info = SourceRegex.Replace(info, GetSource);

        // log message and throw exception on compile error
        if (!string.IsNullOrEmpty(info))
            Logger.WarnFormat("Compile log:\n{0}", info);
        
        if (compileStatus == 1)
            return;
        
        const string msg = "Error compiling shader.";
        Logger.Error($"{msg} Source:\n{InjectLineNumbers(source)}");
        throw new GLShaderCompileException(msg, info);
    }


    private static string InjectLineNumbers(string source)
    {
        string[] lines = source.Split('\n');
        System.Text.StringBuilder sb = new();
        sb.AppendLine("----------------------------------------");
        for (int i = 0; i < lines.Length; i++)
            sb.AppendLine($"{i + 1}: {lines[i]}");
        sb.AppendLine("----------------------------------------");
        return sb.ToString();
    }


    private string GetSource(Match match)
    {
        int index = int.Parse(match.Groups[1].Value);
        System.Diagnostics.Debug.Assert(SourceFiles != null, nameof(SourceFiles) + " != null");
        return index < SourceFiles.Count ? $"ERROR: {SourceFiles[index]}:" : match.ToString();
    }
}