using System.Diagnostics;
using KorpiEngine.Exceptions;

namespace KorpiEngine;

internal static class Debug
{
    [Conditional("TOOLS")]
    public static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new KorpiException(message);
    }
}