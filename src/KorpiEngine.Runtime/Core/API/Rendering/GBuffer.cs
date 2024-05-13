﻿using System.Diagnostics;
using KorpiEngine.Core.API.Rendering.Textures;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Rendering.Primitives;

namespace KorpiEngine.Core.API.Rendering;

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
    public Texture2D? Depth => Buffer.InternalDepth;


    public GBuffer(int width, int height)
    {
        TextureImageFormat[] formats =
        [
            TextureImageFormat.RGBA_16_S,   // Albedo & AO
            TextureImageFormat.RGBA_16_S,   // Normal & Metalness
            TextureImageFormat.RGBA_16_S,   // Position & Roughness
            TextureImageFormat.RGB_16_S,    // Emission
            TextureImageFormat.RG_16_S,     // Velocity
            TextureImageFormat.R_16_S       // ObjectIDs
        ];
        Buffer = new RenderTexture(width, height, 6, true, formats);
    }


    public void Begin(bool clear = true)
    {
        Debug.Assert(FrameBuffer != null, nameof(FrameBuffer) + " != null");
        Graphics.Driver.BindFramebuffer(FrameBuffer);

        Graphics.UpdateViewport(Width, Height);

        // Start with the GBuffer Clear
        if (clear)
            Graphics.Clear(0, 0, 0, 0);
    }


    public void End()
    {
        Graphics.Driver.UnbindFramebuffer();
    }


    public int GetObjectIDAt(Vector2 uv)
    {
        int x = (int)(uv.X * Width);
        int y = (int)(uv.Y * Height);

        Debug.Assert(FrameBuffer != null, nameof(FrameBuffer) + " != null");
        Graphics.Driver.BindFramebuffer(FrameBuffer);
        float result = Graphics.Driver.ReadPixels<float>(5, x, y, TextureImageFormat.R_16_S);
        return (int)result;
    }


    public Vector3 GetViewPositionAt(Vector2 uv)
    {
        int x = (int)(uv.X * Width);
        int y = (int)(uv.Y * Height);
        Debug.Assert(FrameBuffer != null, nameof(FrameBuffer) + " != null");
        Graphics.Driver.BindFramebuffer(FrameBuffer);
        Vector3 result = Graphics.Driver.ReadPixels<System.Numerics.Vector3>(2, x, y, TextureImageFormat.RGB_16_S);
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
        Depth?.Dispose();
        FrameBuffer.Dispose();
    }
}