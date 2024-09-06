using System.Reflection;
using log4net;
using log4net.Config;
using log4net.Core;
using log4net.Repository;

namespace KorpiEngine.Tools.Logging;

public static class LogFactory
{
    private static readonly Level DefaultLogLevel = Level.Info;
    private static readonly bool IsAvailable = File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "log4net.dll"));
    
    
    public static Level LogLevel
    {
        get
        {
            if (!IsAvailable)
                return Level.Off;
            
            ILoggerRepository? logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            return logRepository.Threshold;
        }
        set
        {
            if (!IsAvailable)
                return;
            
            ILoggerRepository? logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            logRepository.Threshold = value;
        }
    }
    
    
    public static void Initialize(string relativeConfigFilePath)
    {
        if (!IsAvailable)
        {
            Console.WriteLine("Log4Net is not available, cannot initialize!");
            return;
        }
        string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativeConfigFilePath);
        FileInfo configFile = new FileInfo(configFilePath);
        
        if (!configFile.Exists)
        {
            Console.WriteLine($"Log4Net configuration file not found at {configFilePath}!");
            return;
        }
        
        ILoggerRepository? logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
        XmlConfigurator.Configure(logRepository, configFile);
        
        logRepository.Threshold = DefaultLogLevel;
    }


    public static IKorpiLogger GetLogger(Type type)
    {
        if (IsAvailable)
            return CreateLogger(type);

        throw new InvalidOperationException("Log4Net is not available!");
    }


    private static IKorpiLogger CreateLogger(Type type)
    {
        ILog logger = LogManager.GetLogger(type);
        if (logger != null)
            return new DefaultLogger(logger);
        
        throw new InvalidOperationException($"Failed to create logger for type {type}!");
    }
}