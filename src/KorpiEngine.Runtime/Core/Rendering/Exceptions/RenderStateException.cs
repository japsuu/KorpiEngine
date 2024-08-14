namespace KorpiEngine.Core.Rendering.Exceptions;

/// <summary>
/// The exception that is thrown when a render state is invalid.
/// </summary>
public class RenderStateException : Exception
{
    internal RenderStateException(string message) : base(message)
    {
    }
}