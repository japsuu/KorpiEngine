using System.Diagnostics;
using KorpiEngine.Utils;

namespace KorpiEngine.Tools;

internal static class Debug
{
    [Conditional("TOOLS")]
    public static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new KorpiException($"Engine bug: {message}");
    }
    
    /// <summary>
    /// Throws an exception if the current thread is not the main thread.
    /// </summary>
    [Conditional("TOOLS")]
    public static void AssertMainThread(bool shouldBeMainThread)
    {
        string log = shouldBeMainThread ?
            "This method must be called from the main thread." :
            "This method must not be called from the main thread.";
        Assert(Application.IsMainThread == shouldBeMainThread, log);
    }
}