using KorpiEngine.AssetManagement;
using KorpiEngine.Mathematics;
using Debug = KorpiEngine.Tools.Debug;

namespace KorpiEngine.Rendering;

// Taken and modified from Prowl's GBuffer.cs
// https://github.com/michaelsakharov/Prowl/blob/main/Prowl.Runtime/GBuffer.cs.

public sealed class GBuffer : IDisposable
{
    private AssetReference<RenderTexture> _buffer;
    
    internal RenderTexture Buffer => _buffer.Asset!;
    internal GraphicsFrameBuffer FrameBuffer => _buffer.Asset!.FrameBuffer;

    public int Width => Buffer.Width;
    public int Height => Buffer.Height;
    public Texture2D AlbedoAO => Buffer.GetInternalTexture(0);
    public Texture2D NormalMetallic => Buffer.GetInternalTexture(1);
    public Texture2D PositionRoughness => Buffer.GetInternalTexture(2);
    public Texture2D Emission => Buffer.GetInternalTexture(3);
    public Texture2D Velocity => Buffer.GetInternalTexture(4);
    public Texture2D ObjectIDs => Buffer.GetInternalTexture(5);
    public Texture2D Unlit => Buffer.GetInternalTexture(6);
    public Texture2D? Depth => Buffer.InternalDepth;


    public GBuffer(int width, int height)
    {
        TextureImageFormat[] formats =
        [
            TextureImageFormat.RGBA_16_F,   // Albedo & AO
            TextureImageFormat.RGBA_16_F,   // Normal & Metalness
            TextureImageFormat.RGBA_16_F,   // Position & Roughness
            TextureImageFormat.RGB_16_F,    // Emission
            TextureImageFormat.RG_16_F,     // Velocity
            TextureImageFormat.R_32_F,      // ObjectIDs
            TextureImageFormat.RGBA_16_F    // Unlit objects
        ];
        _buffer = new RenderTexture(width, height, 7, true, formats).CreateReference();
    }


    public void Begin(bool clear = true)
    {
        Graphics.Device.BindFramebuffer(FrameBuffer);

        Graphics.UpdateViewport(Width, Height);

        // Start with the GBuffer Cleared
        if (clear)
            Graphics.Clear(0, 0, 0, 0);
    }


    public void End()
    {
        Graphics.Device.UnbindFramebuffer();
    }


    public int GetObjectIDAt(Vector2 uv)
    {
        int x = (int)(uv.X * Width);
        int y = (int)(uv.Y * Height);

        Graphics.Device.BindFramebuffer(FrameBuffer);
        float result = Graphics.Device.ReadPixels<float>(5, x, y, TextureImageFormat.R_32_F);
        return (int)result;
    }


    public Vector3 GetViewPositionAt(Vector2 uv)
    {
        int x = (int)(uv.X * Width);
        int y = (int)(uv.Y * Height);
        
        Graphics.Device.BindFramebuffer(FrameBuffer!);
        Vector3 result = Graphics.Device.ReadPixels<Vector3>(2, x, y, TextureImageFormat.RGB_16_F);
        return result;
    }


    public void Dispose()
    {
        _buffer.Release();
    }
}