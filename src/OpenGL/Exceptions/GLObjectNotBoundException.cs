namespace KorpiEngine.OpenGL;

/// <summary>
/// The exception that is thrown when an object is used which must be bound before usage.
/// </summary>
internal class GLObjectNotBoundException : GLException
{
    internal GLObjectNotBoundException(string message) : base(message)
    {
    }
}