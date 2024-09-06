using KorpiEngine.Utils;

namespace KorpiEngine.Rendering;

/// <summary>
/// The exception that is thrown when an OpenGL-related error occurs.
/// </summary>
internal class OpenGLException : KorpiException
{
    internal OpenGLException(string message) : base(message)
    {
    }
    
    internal OpenGLException(string message, Exception innerException) : base(message, innerException)
    {
    }
}