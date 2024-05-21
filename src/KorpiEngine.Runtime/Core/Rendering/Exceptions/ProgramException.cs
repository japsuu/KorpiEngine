using System.Runtime.Serialization;

namespace KorpiEngine.Core.Rendering.Exceptions;

/// <summary>
/// The exception that is thrown when a program related error occurs.
/// </summary>
[Serializable]
public class ProgramException : OpenGLException
{
    public string InfoLog { get; private set; }

    internal ProgramException(string message, string infoLog)
        : base(message)
    {
        InfoLog = infoLog;
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue("InfoLog", InfoLog);
    }
}