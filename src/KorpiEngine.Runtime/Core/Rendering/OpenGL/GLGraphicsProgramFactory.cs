using KorpiEngine.Core.API.Rendering.Shaders;
using KorpiEngine.Core.Rendering.Exceptions;
using ShaderType = OpenTK.Graphics.OpenGL4.ShaderType;

namespace KorpiEngine.Core.Rendering.OpenGL;

/// <summary>
/// Contains methods to automatically initialize shader shaderProgram objects.
/// </summary>
internal static class GLGraphicsProgramFactory
{
    /// <summary>
    /// Initializes a shaderProgram object using the shader sources provided.
    /// </summary>
    /// <returns>A compiled and linked shaderProgram.</returns>
    public static GLGraphicsProgram Create(List<ShaderSourceDescriptor> shaders)
    {
        if (shaders.Count == 0)
            throw new OpenGLException("No shaders provided for shaderProgram creation.");

        // Create a shader shaderProgram instance
        GLGraphicsProgram program = new();
        try
        {
            // compile and attach all shaders
            foreach (ShaderSourceDescriptor sourceInfo in shaders)
            {
                // create a new shader of the appropriate type
                using GLGraphicsShader glShader = new((ShaderType)sourceInfo.Type);

                // compile shader source
                glShader.CompileSource(sourceInfo.Source);

                // attach shader to the shaderProgram
                program.AttachShader(glShader);
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
}