namespace KorpiEngine.Rendering;

[Flags]
internal enum ClearFlags
{
    Color = 1 << 1,
    Depth = 1 << 2,
    Stencil = 1 << 3
}