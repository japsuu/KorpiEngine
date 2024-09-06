using KorpiEngine.Utils;

namespace KorpiEngine.Rendering;

internal class ResourceLeakException : KorpiException
{
    public ResourceLeakException(string message) : base(message)
    {
    }
    
    
    public ResourceLeakException(string message, Exception innerException) : base(message, innerException)
    {
    }
}