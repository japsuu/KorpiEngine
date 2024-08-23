namespace KorpiEngine.Core.Rendering.Exceptions;

/// <summary>
/// The exception that is thrown when a program link error occurs.
/// </summary>
[Serializable]
public class ProgramLinkException : ProgramException
{
    internal ProgramLinkException(string message, string infoLog)
        : base(message, infoLog)
    {
    }
}