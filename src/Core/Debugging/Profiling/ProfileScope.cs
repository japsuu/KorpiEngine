namespace KorpiEngine.Core.Debugging.Profiling;

/// <summary>
/// A scope that can be used to profile code.
/// Wrap the code you want to profile in a using statement.
/// </summary>
public sealed class ProfileScope : IDisposable
{
    public ProfileScope(string name)
    {
        KorpiProfiler.Begin(name);
    }

    public void Dispose()
    {
        KorpiProfiler.End();
    }
}