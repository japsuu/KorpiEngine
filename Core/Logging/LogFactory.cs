using System.Reflection;
using log4net;
using log4net.Config;
using log4net.Repository;

namespace KorpiEngine.Core.Logging;

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
    
    
    public static void Initialize(string relativeConfigFilePath)
    {
        if (!IsAvailable)
        {
            Console.WriteLine("Log4Net is not available, cannot initialize!");
            return;
        }
        
        string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativeConfigFilePath);
        
        ILoggerRepository? logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
        XmlConfigurator.Configure(logRepository, new FileInfo(configFilePath));
    }
}