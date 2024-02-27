using System.Text;
using KorpiEngine.Core.Logging;
using KorpiEngine.Core.Rendering.Exceptions;
using KorpiEngine.Core.Rendering.Shaders.Sources;

namespace KorpiEngine.Core.Rendering.Shaders.ShaderPrograms;

/// <summary>
/// Contains methods to automatically initialize shader shaderProgram objects.
/// </summary>
public static class ShaderProgramFactory
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(ShaderProgramFactory));


    /// <summary>
    /// Initializes a shaderProgram object using the shader sources tagged to the type with <see cref="ShaderSourceAttribute"/>.
    /// </summary>
    /// <typeparam name="T">Specifies the shaderProgram type to create.</typeparam>
    /// <returns>A compiled and linked shaderProgram.</returns>
    public static T Create<T>(string shaderBasePath) where T : ShaderProgram
    {
        // retrieve shader types and filenames from attributes
        List<ShaderSourceAttribute> shaders = ShaderSourceAttribute.GetShaderSources(typeof(T));
        if (shaders.Count == 0)
            throw new OpenGLException($"ShaderSourceAttribute(s) missing for shader of type {typeof(T)}!");

        // create shader shaderProgram instance
        T program = (T)Activator.CreateInstance(typeof(T))!;
        try
        {
            // compile and attach all shaders
            foreach (ShaderSourceAttribute attribute in shaders)
            {
                // create a new shader of the appropriate type
                using Shader shader = new(attribute.Type);
                Logger?.Debug($"Compiling {attribute.Type}: {attribute.SourceLocation}");

                // load the source
                string source = GetShaderSource(shaderBasePath, attribute.SourceLocation, shader.SourceFiles);

                // compile shader source
                shader.CompileSource(source);

                // attach shader to the shaderProgram
                program.Attach(shader);
            }

            // link and return the shaderProgram
            program.Link();
        }
        catch
        {
            program.Dispose();
            throw;
        }

        return program;
    }


    /// <summary>
    /// Retrieves the shader source from the given shader name.
    /// Recursively parses #include directives.
    /// </summary>
    /// <param name="shaderBasePath">The base path of the shader source.</param>
    /// <param name="shaderName">Name of the shader. Example: 'core/pass.vert'</param>
    /// <param name="sourceIncludes">List of already included sources.</param>
    /// <returns>The parsed shader source.</returns>
    /// <exception cref="Exception"></exception>
    private static string GetShaderSource(string shaderBasePath, string shaderName, List<string> sourceIncludes)
    {
        string sourcePath = Path.Combine(shaderBasePath, shaderName);
        
        if (!File.Exists(sourcePath))
            throw new Exception($"Shader source not found: {sourcePath}");
        
        if (sourceIncludes.Contains(sourcePath))
            throw new Exception($"Circular shader include detected: {sourcePath}");
        sourceIncludes.Add(sourcePath);
        
        // Parse source
        using StringReader reader = new(File.ReadAllText(sourcePath));
        StringBuilder source = new();
        while (true)
        {
            string? line = reader.ReadLine();
            if (line == null) break;

            // Check if it is an include statement
            const string includeKeyword = "#include";
            if (line.StartsWith(includeKeyword))
            {
                // Parse the included filename
                string includedSourcePath = line.Remove(0, includeKeyword.Length).Trim();

                // Replace current line with the source of the included section
                source.Append(GetShaderSource(shaderBasePath, includedSourcePath, sourceIncludes));
            }
            else
            {
                source.AppendLine(line);
            }
        }
        
        return source.ToString();
    }
}