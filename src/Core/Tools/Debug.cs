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
}