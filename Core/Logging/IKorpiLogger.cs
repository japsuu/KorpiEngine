namespace Common.Logging;

public interface IKorpiLogger
{
    public bool IsFatalEnabled { get; }
    public bool IsWarnEnabled { get; }
    public bool IsInfoEnabled { get; }
    public bool IsDebugEnabled { get; }
    public bool IsErrorEnabled { get; }

    public void Verbose(object message, Exception? exception = null);
    public void VerboseFormat(IFormatProvider provider, string format, params object[] args);
    public void VerboseFormat(string format, params object[] args);

    public void Debug(object message, Exception? exception = null);
    public void DebugFormat(IFormatProvider provider, string format, params object[] args);
    public void DebugFormat(string format, params object[] args);

    public void Error(object message, Exception? exception = null);
    public void ErrorFormat(IFormatProvider provider, string format, params object[] args);
    public void ErrorFormat(string format, params object[] args);

    public void Fatal(object message, Exception? exception = null);
    public void FatalFormat(IFormatProvider provider, string format, params object[] args);
    public void FatalFormat(string format, params object[] args);

    public void Info(object message, Exception? exception = null);
    public void InfoFormat(IFormatProvider provider, string format, params object[] args);
    public void InfoFormat(string format, params object[] args);

    public void Warn(object message, Exception? exception = null);
    public void WarnFormat(IFormatProvider provider, string format, params object[] args);
    public void WarnFormat(string format, params object[] args);

    public void OpenGl(string message);
}