namespace KorpiEngine.Tools;

/// <summary>
/// Used when profiling is disabled.
/// </summary>
internal struct DummyProfilerZone : IProfilerZone
{
    public void EmitName(string name) { }
    public void EmitColor(uint color) { }
    public void EmitText(string text) { }
    public void Dispose() { }
}