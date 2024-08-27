using System.Diagnostics;
using KorpiEngine.Rendering.Primitives;
using KorpiEngine.Rendering.Textures;

namespace KorpiEngine.Rendering;

// Taken and modified from Prowl's GBuffer.cs
// https://github.com/michaelsakharov/Prowl/blob/main/Prowl.Runtime/GBuffer.cs.

public class GBuffer
{
    internal readonly RenderTexture Buffer;
    internal GraphicsFrameBuffer? FrameBuffer => Buffer.FrameBuffer;

    public int Width => Buffer.Width;
    public int Height => Buffer.Height;
    public Texture2D AlbedoAO => Buffer.InternalTextures[0];
    public Texture2D NormalMetallic => Buffer.InternalTextures[1];
    public Texture2D PositionRoughness => Buffer.InternalTextures[2];
    public Texture2D Emission => Buffer.InternalTextures[3];
    public Texture2D Velocity => Buffer.InternalTextures[4];
    public Texture2D ObjectIDs => Buffer.InternalTextures[5];
    public Texture2D Unlit => Buffer.InternalTextures[6];
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
        Buffer = new RenderTexture(width, height, 7, true, formats);
    }


    public void Begin(bool clear = true)
    {
        Debug.Assert(FrameBuffer != null, nameof(FrameBuffer) + " != null");
        
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

        Debug.Assert(FrameBuffer != null, nameof(FrameBuffer) + " != null");
        
        Graphics.Device.BindFramebuffer(FrameBuffer);
        float result = Graphics.Device.ReadPixels<float>(5, x, y, TextureImageFormat.R_32_F);
        return (int)result;
    }


    public Vector3 GetViewPositionAt(Vector2 uv)
    {
        int x = (int)(uv.X * Width);
        int y = (int)(uv.Y * Height);
        
        Debug.Assert(FrameBuffer != null, nameof(FrameBuffer) + " != null");
        
        Graphics.Device.BindFramebuffer(FrameBuffer);
        Vector3 result = Graphics.Device.ReadPixels<System.Numerics.Vector3>(2, x, y, TextureImageFormat.RGB_16_F);
        return result;
    }


    public void UnloadGBuffer()
    {
        if (FrameBuffer == null)
            return;
        
        AlbedoAO.Dispose();
        NormalMetallic.Dispose();
        PositionRoughness.Dispose();
        Emission.Dispose();
        Velocity.Dispose();
        ObjectIDs.Dispose();
        Unlit.Dispose();
        Depth?.Dispose();
        FrameBuffer.Dispose();
    }
}