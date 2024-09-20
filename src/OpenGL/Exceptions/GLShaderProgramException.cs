namespace KorpiEngine.OpenGL;

/// <summary>
/// The exception that is thrown when a program related error occurs.
/// </summary>
internal class GLShaderProgramException : GLException
{
    public string InfoLog { get; private set; }

    internal GLShaderProgramException(string message, string infoLog)
        : base($"{message}:\n{infoLog}")
    {
        InfoLog = infoLog;
    }
}