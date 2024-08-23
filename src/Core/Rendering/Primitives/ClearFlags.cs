namespace KorpiEngine.Core.Rendering.Primitives;

[Flags]
internal enum ClearFlags
{
    Color = 1 << 1,
    Depth = 1 << 2,
    Stencil = 1 << 3
}