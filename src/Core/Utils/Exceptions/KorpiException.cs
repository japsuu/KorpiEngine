namespace KorpiEngine.Utils;

internal class KorpiException : Exception
{
    public KorpiException(string? message) : base(message)
    {
    }


    public KorpiException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}