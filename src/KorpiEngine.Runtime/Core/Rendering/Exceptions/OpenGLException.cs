namespace KorpiEngine.Core.Rendering.Exceptions;

/// <summary>
/// The exception that is thrown when an OpenGL related error occurs.
/// </summary>
[Serializable]
public class OpenGLException : Exception
{
    internal OpenGLException(string message) : base(message)
    {
    }
}