namespace KorpiEngine.Core.Rendering.Exceptions;

/// <summary>
/// The exception that is thrown when a shader compile error occurs.
/// </summary>
[Serializable]
public class ShaderCompileException : ProgramException
{
    internal ShaderCompileException(string message, string infoLog)
        : base(message, infoLog)
    {
    }
}