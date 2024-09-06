namespace KorpiEngine.Rendering;

[Flags]
public enum CameraClearFlags
{
    None = 0,
    Color = 1,
    Depth = 2,
    Stencil = 4
}

public static class CameraClearFlagsExtensions
{
    public static bool HasFlagFast(this CameraClearFlags value, CameraClearFlags flag)
    {
        return (value & flag) != 0;
    }
}