using System.Diagnostics.CodeAnalysis;
using KorpiEngine.AssetManagement;
using KorpiEngine.Utils;
using Debug = KorpiEngine.Tools.Debug;

namespace KorpiEngine.Rendering;

// Taken and modified from Prowl's RenderTexture.cs
// https://github.com/michaelsakharov/Prowl/blob/main/Prowl.Runtime/RenderTexture.cs.

public sealed class RenderTexture : Asset
{
    private const int MAX_UNUSED_FRAMES = 10;
    private static readonly Dictionary<RenderTextureKey, List<(RenderTexture, int frameCreated)>> Pool = [];
    private static readonly List<RenderTexture> DisposableTextures = [];

    private readonly AssetReference<Texture2D>[] _internalTextures;
    private readonly AssetReference<Texture2D>? _internalDepth;
    internal readonly GraphicsFrameBuffer FrameBuffer;
    
    public Texture2D MainTexture => _internalTextures[0].Asset!;
    public Texture2D? InternalDepth => _internalDepth?.Asset;

    public int Width { get; private set; }
    public int Height { get; private set; }
    
    public Texture2D GetInternalTexture(int index) => _internalTextures[index].Asset!;


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
        _internalTextures = new AssetReference<Texture2D>[numTextures];
        for (int i = 0; i < numTextures; i++)
        {
            TextureImageFormat format = textureFormats[i];
            Texture2D texture = new Texture2D(width, height, false, format, $"RenderTexture Attachment {i} ({format})");
            texture.SetTextureFilters(TextureMin.Linear, TextureMag.Linear);
            texture.SetWrapModes(TextureWrap.ClampToEdge, TextureWrap.ClampToEdge);
            attachments[i] = new GraphicsFrameBuffer.Attachment
            {
                Texture = texture.Handle,
                IsDepth = false
            };
            _internalTextures[i] = texture.CreateReference();
        }

        if (hasDepthAttachment)
        {
            Texture2D depthTexture = new Texture2D(width, height, false, TextureImageFormat.DEPTH_24, "RenderTexture Depth Attachment");
            attachments[numTextures] = new GraphicsFrameBuffer.Attachment
            {
                Texture = depthTexture.Handle,
                IsDepth = true
            };
            _internalDepth = depthTexture.CreateReference();
        }

        FrameBuffer = Graphics.Device.CreateFramebuffer(attachments);
    }


    public void Begin()
    {
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
            throw new ResourceLeakException($"RenderTexture '{Name}' was not disposed of explicitly, and is now being disposed by the GC. This is a memory leak!");
#endif

        foreach (AssetReference<Texture2D> texture in _internalTextures)
            texture.Release();
        
        _internalDepth?.Release();

        FrameBuffer.Dispose();
    }


    #region Pool

    private readonly struct RenderTextureKey(int width, int height, TextureImageFormat[] format) : IEquatable<RenderTextureKey>
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


        public bool Equals(RenderTextureKey other) => _width == other._width && _height == other._height && _format.Equals(other._format);


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
        RenderTextureKey key = new(renderTexture.Width, renderTexture.Height, renderTexture._internalTextures.Select(t => t.Asset!.ImageFormat).ToArray());

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
            renderTexture.DisposeDeferred();
    }

    #endregion
}