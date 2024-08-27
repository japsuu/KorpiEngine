namespace KorpiEngine.Core.Exceptions;

/// <summary>
/// The exception that is thrown when a shader compile error occurs.
/// </summary>
public class ShaderCompileException : ProgramException
{
    internal ShaderCompileException(string message, string infoLog)
        : base(message, infoLog)
    {
    }
}