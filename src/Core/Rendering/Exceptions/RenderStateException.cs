using KorpiEngine.Utils;

namespace KorpiEngine.Rendering;

/// <summary>
/// The exception that is thrown when a render state is invalid.
/// </summary>
internal class RenderStateException : KorpiException
{
    internal RenderStateException(string message) : base(message)
    {
    }
}