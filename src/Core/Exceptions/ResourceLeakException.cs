namespace KorpiEngine.Exceptions;

public class ResourceLeakException : KorpiException
{
    public ResourceLeakException(string message) : base(message)
    {
    }
    
    
    public ResourceLeakException(string message, Exception innerException) : base(message, innerException)
    {
    }
}