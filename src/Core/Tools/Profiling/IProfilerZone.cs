using KorpiEngine.Mathematics;

namespace KorpiEngine.Tools;

public interface IProfilerZone : IDisposable
{
    public void EmitName(string name);
    public void EmitColor(ColorRGB color);
    public void EmitText(string text);
}