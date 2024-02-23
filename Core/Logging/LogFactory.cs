using log4net;

namespace Common.Logging;

public static class LogFactory
{
    private static readonly bool IsAvailable = File.Exists(AppDomain.CurrentDomain.BaseDirectory + "log4net.dll");


    private static IKorpiLogger CreateLogger(Type type)
    {
        ILog logger = LogManager.GetLogger(type);
        if (logger != null)
            return new DefaultLogger(logger);
        
        throw new Exception($"Failed to create logger for type {type}!");
    }


    public static IKorpiLogger GetLogger(Type type)
    {
        if (IsAvailable)
            return CreateLogger(type);

        throw new Exception("Log4Net is not available!");
    }
}