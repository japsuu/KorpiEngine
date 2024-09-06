namespace KorpiEngine.Exceptions;

/// <summary>
/// The exception that is thrown when a program link error occurs.
/// </summary>
internal class ShaderProgramLinkException : ShaderProgramException
{
    internal ShaderProgramLinkException(string message, string infoLog)
        : base(message, infoLog)
    {
    }
}