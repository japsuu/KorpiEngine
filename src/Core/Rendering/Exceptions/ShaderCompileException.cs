namespace KorpiEngine.Rendering;

/// <summary>
/// The exception that is thrown when a shader compile error occurs.
/// </summary>
internal class ShaderCompileException : ShaderProgramException
{
    internal ShaderCompileException(string message, string infoLog)
        : base(message, infoLog)
    {
    }
}