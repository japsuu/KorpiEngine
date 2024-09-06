namespace KorpiEngine.Rendering;

internal abstract class GraphicsFrameBuffer(int handle) : GraphicsObject(handle)
{
    public struct Attachment
    {
        public GraphicsTexture Texture;
        public bool IsDepth;
    }
}