namespace KorpiEngine.OpenGL;

/// <summary>
/// The exception that is thrown when a program link error occurs.
/// </summary>
internal class GLShaderProgramLinkException : GLShaderProgramException
{
    internal GLShaderProgramLinkException(string message, string infoLog)
        : base(message, infoLog)
    {
    }
}