using OpenTK.Mathematics;

namespace KorpiEngine.Core.Rendering;

internal class RenderCamera
{
    public Matrix4 ProjectionMatrix { get; set; }
    public Matrix4 ViewMatrix { get; set; }
    public Matrix4 ViewProjectionMatrix { get; set; }
}