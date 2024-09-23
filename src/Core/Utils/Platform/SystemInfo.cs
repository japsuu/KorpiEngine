using System.Runtime.InteropServices;

namespace KorpiEngine.Utils;

/// <summary>
/// Contains information about the current system.
/// </summary>
public static class SystemInfo
{
    public static bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public static bool IsLinux() => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    public static bool IsFreeBSD() => RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD);
    public static bool IsMac() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);


    /// <summary>
    /// Gets the number of processors available to the current process.
    /// </summary>
    public static int ProcessorCount { get; private set; }

    /// <summary>
    /// ID of the thread updating the main window.
    /// </summary>
    public static int MainThreadId { get; private set; }
    
    
    public static void Initialize()
    {
        ProcessorCount = Environment.ProcessorCount;
        MainThreadId = Environment.CurrentManagedThreadId;
    }
}