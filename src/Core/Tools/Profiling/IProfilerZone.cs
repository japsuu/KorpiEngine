namespace KorpiEngine.Tools;

/// <summary>
/// Represents a zone in the profiler.
/// </summary>
public interface IProfilerZone : IDisposable
{
    /// <summary>
    /// Emits a custom name for this zone.
    /// </summary>
    /// <param name="name">The name to emit.</param>
    public void EmitName(string name);
    
    /// <summary>
    /// Emits a custom color for this zone.
    /// </summary>
    /// <param name="color">The color to emit.</param>
    public void EmitColor(uint color);
    
    /// <summary>
    /// Emits custom text for this zone.
    /// </summary>
    /// <param name="text">The text to emit.</param>
    public void EmitText(string text);
}