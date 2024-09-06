using System.Collections;

namespace KorpiEngine.EntityModel.Coroutines;

public sealed class WaitForSecondsRealtime(float seconds) : IEnumerator
{
    private readonly double _endTime = Time.TotalTime + seconds;

    public object? Current => null;

    
    public bool MoveNext() => Time.TotalTime < _endTime;
    public void Reset() { }
}