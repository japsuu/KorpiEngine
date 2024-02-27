namespace KorpiEngine.Core.Rendering.Exceptions;

/// <summary>
/// The exception that is thrown when an object is used which must be bound before usage.
/// </summary>
[Serializable]
public class ObjectNotBoundException : OpenGLException
{
    internal ObjectNotBoundException(string message) : base(message)
    {
    }
}