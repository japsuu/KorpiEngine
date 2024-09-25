namespace KorpiEngine.Tools;

public interface IProfilerZone : IDisposable
{
    public void EmitName(string name);
    public void EmitColor(uint color);
    public void EmitText(string text);
}