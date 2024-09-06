namespace KorpiEngine.Rendering;

/// <summary>
/// The exception that is thrown when a program related error occurs.
/// </summary>
internal class ShaderProgramException : OpenGLException
{
    public string InfoLog { get; private set; }

    internal ShaderProgramException(string message, string infoLog)
        : base($"{message}:\n{infoLog}")
    {
        InfoLog = infoLog;
    }
}