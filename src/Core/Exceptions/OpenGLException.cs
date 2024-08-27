namespace KorpiEngine.Core.Exceptions;

/// <summary>
/// The exception that is thrown when an OpenGL-related error occurs.
/// </summary>
public class OpenGLException : KorpiException
{
    internal OpenGLException(string message) : base(message)
    {
    }
    
    internal OpenGLException(string message, Exception innerException) : base(message, innerException)
    {
    }
}