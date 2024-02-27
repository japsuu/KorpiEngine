namespace KorpiEngine.Core.Debugging.Profiling;

/// <summary>
/// <see cref="ProfileScope"/> but will get discarded if the duration is less than the specified value.
/// Wrap the code you want to profile in a using statement.
/// </summary>
public sealed class ConditionalProfileScope : IDisposable
{
    public ConditionalProfileScope(string name, float discardIfDurationLessThanMillis)
    {
        KorpiProfiler.BeginConditional(name, discardIfDurationLessThanMillis);
    }

    public void Dispose()
    {
        KorpiProfiler.End();
    }
}