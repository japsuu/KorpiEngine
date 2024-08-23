namespace KorpiEngine.Core.Rendering.Exceptions;

/// <summary>
/// The exception that is thrown when a program related error occurs.
/// </summary>
public class ProgramException : OpenGLException
{
    public string InfoLog { get; private set; }

    internal ProgramException(string message, string infoLog)
        : base($"{message}:\n{infoLog}")
    {
        InfoLog = infoLog;
    }
}