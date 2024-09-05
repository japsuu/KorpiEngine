namespace KorpiEngine.Exceptions;

/// <summary>
/// The exception that is thrown when a render state is invalid.
/// </summary>
public class RenderStateException : KorpiException
{
    internal RenderStateException(string message) : base(message)
    {
    }
}