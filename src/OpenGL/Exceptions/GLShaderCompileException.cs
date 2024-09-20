namespace KorpiEngine.OpenGL;

/// <summary>
/// The exception that is thrown when a shader compile error occurs.
/// </summary>
internal class GLShaderCompileException : GLShaderProgramException
{
    internal GLShaderCompileException(string message, string infoLog)
        : base(message, infoLog)
    {
    }
}