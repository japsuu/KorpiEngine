using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using KorpiEngine.Exceptions;
using KorpiEngine.Platform;
using KorpiEngine.Rendering.Primitives;
using KorpiEngine.Rendering.Textures;

namespace KorpiEngine.Rendering;

// Taken and modified from Prowl's RenderTexture.cs
// https://github.com/michaelsakharov/Prowl/blob/main/Prowl.Runtime/RenderTexture.cs.

public sealed class RenderTexture : AssetInstance
{
    private const int MAX_UNUSED_FRAMES = 10;
    private static readonly Dictionary<RenderTextureKey, List<(RenderTexture, int frameCreated)>> Pool = [];
    private static readonly List<RenderTexture> DisposableTextures = [];
    
    internal GraphicsFrameBuffer? FrameBuffer { get; private init; }
    
    public Texture2D MainTexture => InternalTextures[0];
    public Texture2D[] InternalTextures { get; private set; }
    public Texture2D? InternalDepth { get; private set; }

    public int Width { get; private set; }
    public int Height { get; private set; }


    public RenderTexture(int width, int height, int numTextures = 1, bool hasDepthAttachment = true, TextureImageFormat[]? formats = null) : base("RenderTexture")
    {
        TextureImageFormat[] textureFormats;
        if (numTextures < 0 || numTextures > SystemInfo.MaxFramebufferColorAttachments)
            throw new ArgumentOutOfRangeException("Invalid number of textures! [0-" + SystemInfo.MaxFramebufferColorAttachments + "]");

        Width = width;
        Height = height;

        if (formats == null)
        {
            textureFormats = new TextureImageFormat[numTextures];
            for (int i = 0; i < numTextures; i++)
                textureFormats[i] = TextureImageFormat.RGBA_8_UF;
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

        FrameBuffer = Graphics.Device.CreateFramebuffer(attachments);
    }


    public void Begin()
    {
        Debug.Assert(FrameBuffer != null, nameof(FrameBuffer) + " != null");
        Graphics.Device.BindFramebuffer(FrameBuffer);
        Graphics.UpdateViewport(Width, Height);
    }


    public void End()
    {
        Graphics.Device.UnbindFramebuffer();
        Graphics.UpdateViewport(Graphics.Window.FramebufferSize.X, Graphics.Window.FramebufferSize.Y);
    }


    protected override void OnDispose(bool manual)
    {
        
#if TOOLS
        if (!manual)
            throw new ResourceLeakException($"Mesh '{Name}' was not disposed of explicitly, and is now being disposed by the GC. This is a memory leak!");
#endif

        foreach (Texture2D texture in InternalTextures)
            texture.Dispose();
        
        InternalDepth?.Dispose();

        FrameBuffer?.Dispose();
    }


    #region Pool

    private readonly struct RenderTextureKey(int width, int height, TextureImageFormat[] format)
    {
        private readonly int _width = width;
        private readonly int _height = height;
        private readonly TextureImageFormat[] _format = format;


        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is not RenderTextureKey key)
                return false;

            if (_width != key._width || _height != key._height || _format.Length != key._format.Length)
                return false;

            for (int i = 0; i < _format.Length; i++)
            {
                if (_format[i] != key._format[i])
                    return false;
            }
            
            return true;
        }


        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + _width.GetHashCode();
            hash = hash * 23 + _height.GetHashCode();
            foreach (TextureImageFormat texFormat in _format)
                hash = hash * 23 + ((int)texFormat).GetHashCode();
            return hash;
        }


        public static bool operator ==(RenderTextureKey left, RenderTextureKey right) => left.Equals(right);
        public static bool operator !=(RenderTextureKey left, RenderTextureKey right) => !(left == right);
    }


    public static RenderTexture GetTemporaryRT(int width, int height, TextureImageFormat[] format)
    {
        RenderTextureKey key = new(width, height, format);

        if (!Pool.TryGetValue(key, out List<(RenderTexture, int frameCreated)>? list) || list.Count <= 0)
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

        if (!Pool.TryGetValue(key, out List<(RenderTexture, int frameCreated)>? list))
        {
            list = [];
            Pool[key] = list;
        }

        list.Add((renderTexture, Time.TotalFrameCount));
    }


    public static void UpdatePool()
    {
        DisposableTextures.Clear();
        
        foreach (KeyValuePair<RenderTextureKey, List<(RenderTexture, int frameCreated)>> pair in Pool)
        {
            for (int i = pair.Value.Count - 1; i >= 0; i--)
            {
                (RenderTexture renderTexture, int frameCreated) = pair.Value[i];

                if (Time.TotalFrameCount - frameCreated <= MAX_UNUSED_FRAMES)
                    continue;

                DisposableTextures.Add(renderTexture);
                pair.Value.RemoveAt(i);
            }
        }

        foreach (RenderTexture renderTexture in DisposableTextures)
            renderTexture.Destroy();
    }

    #endregion
}