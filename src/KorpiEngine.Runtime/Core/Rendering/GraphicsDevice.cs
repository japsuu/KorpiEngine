using KorpiEngine.Core.API;
using KorpiEngine.Core.API.Rendering.Shaders;
using KorpiEngine.Core.Internal.Rendering;
using KorpiEngine.Core.Logging;
using KorpiEngine.Core.Rendering.Primitives;

namespace KorpiEngine.Core.Rendering;

internal abstract class GraphicsDevice
{
    protected static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(GraphicsDevice));

    public abstract GraphicsProgram? CurrentProgram { get; }


    #region Initialization and Shutdown

    public void Initialize()
    {
        Logger.Info($"Initializing {nameof(GraphicsDevice)}...");
        InitializeInternal();
    }


    protected abstract void InitializeInternal();


    public void Shutdown()
    {
        Logger.Info($"Shutting down {nameof(GraphicsDevice)}...");
        ShutdownInternal();
    }


    protected abstract void ShutdownInternal();

    #endregion


    #region State
    
    public abstract void SetState(RasterizerState state, bool force = false);
    // State overrides:
    public abstract void SetEnableDepthTest(bool enable, bool force = false);
    public abstract void SetEnableDepthWrite(bool enable, bool force = false);
    public abstract void SetEnableBlending(bool enable, bool force = false);
    public abstract void SetEnableCulling(bool enable, bool force = false);
    public abstract void SetEnableScissorTest(bool enable, bool force = false);
    public abstract void SetScissorRect(int index, int left, int bottom, int width, int height);
    
    public abstract RasterizerState GetState();

    public abstract void UpdateViewport(int x, int y, int width, int height);

    public abstract void Clear(float r, float g, float b, float a, ClearFlags flags);
    
    public abstract void SetWireframeMode(bool enabled);

    #endregion
    
    
    #region Buffers

    /// <summary> Create a graphics buffer with the given type and data. </summary>
    public abstract GraphicsBuffer CreateBuffer<T>(BufferType bufferType, T[] data, bool dynamic = false) where T : unmanaged;

    /// <summary> Set the data of the given buffer with the given data. </summary>
    public abstract void SetBuffer<T>(GraphicsBuffer buffer, T[] data, bool dynamic = false) where T : unmanaged;

    /// <summary> Update the given buffer with the given data at the given offset in bytes. </summary>
    public abstract void UpdateBuffer<T>(GraphicsBuffer buffer, int offsetInBytes, T[] data) where T : unmanaged;
    public abstract void UpdateBuffer(GraphicsBuffer buffer, int offsetInBytes, int sizeInBytes, nint data);

    public abstract void BindBuffer(GraphicsBuffer buffer);

    #endregion

    
    #region Vertex Arrays

    public abstract GraphicsVertexArrayObject CreateVertexArray(MeshVertexLayout layout, GraphicsBuffer vertices, GraphicsBuffer? indices);
    public abstract void BindVertexArray(GraphicsVertexArrayObject? vertexArrayObject);

    #endregion
    
    
    #region Frame Buffers

    public abstract GraphicsFrameBuffer CreateFramebuffer(GraphicsFrameBuffer.Attachment[] attachments);
    public abstract void UnbindFramebuffer();
    public abstract void BindFramebuffer(GraphicsFrameBuffer frameBuffer, FBOTarget target = FBOTarget.Framebuffer);
    public abstract void BlitFramebuffer(int v1, int v2, int width, int height, int v3, int v4, int v5, int v6, ClearFlags depthBufferBit, BlitFilter nearest);
    public abstract void ReadPixels<T>(int attachment, int x, int y, TextureImageFormat format, IntPtr output) where T : unmanaged;
    public abstract T ReadPixels<T>(int attachment, int x, int y, TextureImageFormat format) where T : unmanaged;

    #endregion

    
    #region Shaders

    public abstract GraphicsProgram CompileProgram(List<ShaderSourceDescriptor> shaders);
    public abstract void BindProgram(GraphicsProgram program);
    
    public abstract int GetUniformLocation(GraphicsProgram program, string name);
    public abstract int GetAttribLocation(GraphicsProgram program, string name);
    
    public abstract void SetUniformF(GraphicsProgram program, string name, float value);
    public abstract void SetUniformF(GraphicsProgram program, int location, float value);
    public abstract void SetUniformI(GraphicsProgram program, string name, int value);
    public abstract void SetUniformI(GraphicsProgram program, int location, int value);
    public abstract void SetUniformV2(GraphicsProgram program, string name, Vector2 value);
    public abstract void SetUniformV2(GraphicsProgram program, int location, Vector2 value);
    public abstract void SetUniformV3(GraphicsProgram program, string name, Vector3 value);
    public abstract void SetUniformV3(GraphicsProgram program, int location, Vector3 value);
    public abstract void SetUniformV4(GraphicsProgram program, string name, Vector4 value);
    public abstract void SetUniformV4(GraphicsProgram program, int location, Vector4 value);
    public abstract void SetUniformMatrix(GraphicsProgram program, string name, int length, bool transpose, in float m11);
    public abstract void SetUniformMatrix(GraphicsProgram program, int location, int length, bool transpose, in float m11);
    public abstract void SetUniformTexture(GraphicsProgram program, string name, int slot, GraphicsTexture texture);
    public abstract void SetUniformTexture(GraphicsProgram program, int location, int slot, GraphicsTexture texture);
    // Explicitly clear a texture slot.
    // This is to reduce user error, since SetUniformTexture should throw if the texture is not available.
    public abstract void ClearUniformTexture(GraphicsProgram program, string name, int slot);
    public abstract void ClearUniformTexture(GraphicsProgram program, int location, int slot);
    
    #endregion

    
    #region Textures

    public abstract GraphicsTexture CreateTexture(TextureType type, TextureImageFormat format);
    public abstract void SetWrapS(GraphicsTexture texture, TextureWrap wrap);
    public abstract void SetWrapT(GraphicsTexture texture, TextureWrap wrap);
    public abstract void SetWrapR(GraphicsTexture texture, TextureWrap wrap);
    public abstract void SetTextureFilters(GraphicsTexture texture, TextureMin min, TextureMag mag);
    public abstract void GenerateMipmap(GraphicsTexture texture);

    public abstract void GetTexImage(GraphicsTexture texture, int mipLevel, nint data);

    public abstract void TexImage2D(GraphicsTexture texture, int mipLevel, int width, int height, int border, nint data);
    public abstract void TexImage2D(GraphicsTexture texture, CubemapFace face, int mipLevel, int width, int height, int border, nint data);
    public abstract void TexSubImage2D(GraphicsTexture texture, int mipLevel, int xOffset, int yOffset, int width, int height, nint data);
    public abstract void TexSubImage2D(GraphicsTexture texture, CubemapFace face, int mipLevel, int xOffset, int yOffset, int width, int height, nint data);
    public abstract void TexSubImage3D(GraphicsTexture texture, int mipLevel, int xOffset, int yOffset, int zOffset, int width, int height, int depth, nint data);
    public abstract void TexImage3D(GraphicsTexture texture, int mipLevel, int width, int height, int depth, int border, nint data);

    #endregion


    #region Drawing

    public abstract void DrawArrays(Topology primitiveType, int startIndex, int count);
    public abstract void DrawElements(Topology triangles, int indexCount, bool isIndex32Bit, int indexOffset);
    public abstract void DrawElements(Topology triangles, int indexCount, bool isIndex32Bit, int indexOffset, int vertexOffset);

    #endregion
}