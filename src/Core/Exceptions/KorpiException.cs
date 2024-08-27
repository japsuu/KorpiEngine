namespace KorpiEngine.Core.Exceptions;

public class KorpiException : Exception
{
    public KorpiException(string? message) : base(message)
    {
    }


    public KorpiException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}