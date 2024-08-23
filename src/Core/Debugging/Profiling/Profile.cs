using System.Diagnostics;

namespace KorpiEngine.Core.Debugging.Profiling;

public class Profile
{
    public readonly string Name;
    public readonly Stopwatch Stopwatch;
    public readonly List<Profile> Children = new();
    public readonly float DiscardIfDurationLessThan;
    public double DurationMillis;


    public Profile(string name, Stopwatch stopwatch, float discardIfDurationLessThan)
    {
        Name = name;
        Stopwatch = stopwatch;
        DiscardIfDurationLessThan = discardIfDurationLessThan;
    }
}