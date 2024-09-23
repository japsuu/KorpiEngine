using KorpiEngine.Utils;

namespace KorpiEngine.OpenGL;

/// <summary>
/// The exception that is thrown when an OpenGL-related error occurs.
/// </summary>
internal class GLException : KorpiException
{
    internal GLException(string message) : base(message)
    {
    }
    
    internal GLException(string message, Exception innerException) : base(message, innerException)
    {
    }
}