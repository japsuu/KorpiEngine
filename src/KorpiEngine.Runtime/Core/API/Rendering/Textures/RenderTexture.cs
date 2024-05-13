﻿using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using KorpiEngine.Core.Platform;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Rendering.Primitives;

namespace KorpiEngine.Core.API.Rendering.Textures;

public sealed class RenderTexture : EngineObject
{
    internal GraphicsFrameBuffer? FrameBuffer { get; private init; }
    public Texture2D? MainTexture => InternalTextures?[0];
    public Texture2D[]? InternalTextures { get; private set; }
    public Texture2D? InternalDepth { get; private set; }

    public int Width { get; private set; }
    public int Height { get; private set; }


    public RenderTexture() : base("RenderTexture")
    {
        Width = 0;
        Height = 0;
    }


    public RenderTexture(int width, int height, int numTextures = 1, bool hasDepthAttachment = true, TextureImageFormat[]? formats = null) : base(
        "RenderTexture")
    {
        TextureImageFormat[] textureFormats;
        if (numTextures < 0 || numTextures > SystemInfo.MaxFramebufferColorAttachments)
            throw new Exception("Invalid number of textures! [0-" + SystemInfo.MaxFramebufferColorAttachments + "]");

        Width = width;
        Height = height;

        if (formats == null)
        {
            textureFormats = new TextureImageFormat[numTextures];
            for (int i = 0; i < numTextures; i++)
                textureFormats[i] = TextureImageFormat.RGBA_8_B;
        }
        else
        {
            if (formats.Length != numTextures)
                throw new ArgumentException("Invalid number of texture formats!");
            textureFormats = formats;
        }

        GraphicsFrameBuffer.Attachment[] attachments = new GraphicsFrameBuffer.Attachment[numTextures + (hasDepthAttachment ? 1 : 0)];
        InternalTextures = new Texture2D[numTextures];
        for (int i = 0; i < numTextures; i++)
        {
            InternalTextures[i] = new Texture2D(width, height, false, textureFormats[i]);
            InternalTextures[i].SetTextureFilters(TextureMin.Linear, TextureMag.Linear);
            InternalTextures[i].SetWrapModes(TextureWrap.ClampToEdge, TextureWrap.ClampToEdge);
            attachments[i] = new GraphicsFrameBuffer.Attachment
            {
                Texture = InternalTextures[i].Handle,
                IsDepth = false
            };
        }

        if (hasDepthAttachment)
        {
            InternalDepth = new Texture2D(width, height, false, TextureImageFormat.DEPTH_24);
            attachments[numTextures] = new GraphicsFrameBuffer.Attachment
            {
                Texture = InternalDepth.Handle,
                IsDepth = true
            };
        }

        FrameBuffer = Graphics.Driver.CreateFramebuffer(attachments);
    }


    public void Begin()
    {
        Debug.Assert(FrameBuffer != null, nameof(FrameBuffer) + " != null");
        Graphics.Driver.BindFramebuffer(FrameBuffer);
        Graphics.UpdateViewport(Width, Height);
        Graphics.FrameBufferSize = new Vector2i(Width, Height);
    }


    public void End()
    {
        Graphics.Driver.UnbindFramebuffer();
        Graphics.UpdateViewport(Application.Window.FramebufferSize.X, Application.Window.FramebufferSize.Y);
        Graphics.FrameBufferSize = new Vector2i(Width, Height);
    }


    protected override void OnDispose()
    {
        if (InternalTextures != null)
            foreach (Texture2D texture in InternalTextures)
                texture.Dispose();

        FrameBuffer?.Dispose();
    }


    #region Pool

    private readonly struct RenderTextureKey(int width, int height, TextureImageFormat[] format)
    {
        public readonly int Width = width;
        public readonly int Height = height;
        public readonly TextureImageFormat[] Format = format;


        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is not RenderTextureKey key)
                return false;

            if (Width != key.Width || Height != key.Height || Format.Length != key.Format.Length)
                return false;

            for (int i = 0; i < Format.Length; i++)
                if (Format[i] != key.Format[i])
                    return false;
            return true;
        }


        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + Width.GetHashCode();
            hash = hash * 23 + Height.GetHashCode();
            foreach (TextureImageFormat format in Format)
                hash = hash * 23 + ((int)format).GetHashCode();
            return hash;
        }


        public static bool operator ==(RenderTextureKey left, RenderTextureKey right) => left.Equals(right);

        public static bool operator !=(RenderTextureKey left, RenderTextureKey right) => !(left == right);
    }

    private static readonly Dictionary<RenderTextureKey, List<(RenderTexture, long frameCreated)>> Pool = [];
    private const int MAX_UNUSED_FRAMES = 10;


    public static RenderTexture GetTemporaryRT(int width, int height, TextureImageFormat[] format)
    {
        RenderTextureKey key = new(width, height, format);

        if (!Pool.TryGetValue(key, out List<(RenderTexture, long frameCreated)>? list) || list.Count <= 0)
            return new RenderTexture(width, height, 1, false, format);

        int i = list.Count - 1;
        RenderTexture renderTexture = list[i].Item1;
        list.RemoveAt(i);
        return renderTexture;
    }


    public static void ReleaseTemporaryRT(RenderTexture renderTexture)
    {
        Debug.Assert(renderTexture.InternalTextures != null, "renderTexture.InternalTextures != null");
        RenderTextureKey key = new(renderTexture.Width, renderTexture.Height, renderTexture.InternalTextures.Select(t => t.ImageFormat).ToArray());

        if (!Pool.TryGetValue(key, out List<(RenderTexture, long frameCreated)>? list))
        {
            list = [];
            Pool[key] = list;
        }

        list.Add((renderTexture, Time.TotalFrameCount));
    }


    public static void UpdatePool()
    {
        List<RenderTexture> disposableTextures = [];
        foreach (KeyValuePair<RenderTextureKey, List<(RenderTexture, long frameCreated)>> pair in Pool)
            for (int i = pair.Value.Count - 1; i >= 0; i--)
            {
                (RenderTexture renderTexture, long frameCreated) = pair.Value[i];

                if (Time.TotalFrameCount - frameCreated <= MAX_UNUSED_FRAMES)
                    continue;

                disposableTextures.Add(renderTexture);
                pair.Value.RemoveAt(i);
            }

        foreach (RenderTexture renderTexture in disposableTextures)
            renderTexture.Destroy();
    }

    #endregion
}