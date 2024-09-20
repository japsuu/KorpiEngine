using KorpiEngine.Mathematics;
using KorpiEngine.Tools.Logging;

namespace KorpiEngine.Rendering;

public abstract class GraphicsDevice
{
    public abstract GraphicsProgram? CurrentProgram { get; }

#if TOOLS
    public ulong RenderedTriangles { get; private set; }
    public ulong RenderedVertices { get; private set; }
    public ulong DrawCalls { get; private set; }
    public ulong RasterizerStateReads { get; private set; }
    public ulong RasterizerStateWrites { get; private set; }
    public ulong RasterizerStateOverrides { get; private set; }
    public ulong ShaderUniformWrites { get; private set; }
    public ulong TextureSwaps { get; internal set; }
    public ulong Clears { get; private set; }
#endif
    
    public abstract int MaxTextureSize { get; protected set; }
    public abstract int MaxCubeMapTextureSize { get; protected set; }
    public abstract int MaxArrayTextureLayers { get; protected set; }
    public abstract int MaxFramebufferColorAttachments { get; protected set; }


    #region Initialization and Shutdown

    public void Initialize()
    {
        Application.Logger.Info($"Initializing {nameof(GraphicsDevice)}...");
        InitializeInternal();
    }


    protected abstract void InitializeInternal();


    public void Shutdown()
    {
        Application.Logger.Info($"Shutting down {nameof(GraphicsDevice)}...");
        ShutdownInternal();
    }


    protected abstract void ShutdownInternal();

    #endregion


    #region State
    
    public void SetState(RasterizerState state, bool force = false)
    {
        SetStateInternal(state, force);
#if TOOLS
        RasterizerStateWrites++;
#endif
    }
    
    
    public void SetEnableDepthTest(bool enable, bool force = false)
    {
        SetEnableDepthTestInternal(enable, force);
#if TOOLS
        RasterizerStateOverrides++;
#endif
    }
    
    
    public void SetEnableDepthWrite(bool enable, bool force = false)
    {
        SetEnableDepthWriteInternal(enable, force);
#if TOOLS
        RasterizerStateOverrides++;
#endif
    }
    
    
    public void SetEnableBlending(bool enable, bool force = false)
    {
        SetEnableBlendingInternal(enable, force);
#if TOOLS
        RasterizerStateOverrides++;
#endif
    }
    
    
    public void SetEnableCulling(bool enable, bool force = false)
    {
        SetEnableCullingInternal(enable, force);
#if TOOLS
        RasterizerStateOverrides++;
#endif
    }
    
    
    public void SetEnableScissorTest(bool enable, bool force = false)
    {
        SetEnableScissorTestInternal(enable, force);
#if TOOLS
        RasterizerStateOverrides++;
#endif
    }
    
    
    public void SetScissorRect(int index, int left, int bottom, int width, int height)
    {
        SetScissorRectInternal(index, left, bottom, width, height);
#if TOOLS
        RasterizerStateOverrides++;
#endif
    }
    
    
    public RasterizerState GetState()
    {
#if TOOLS
        RasterizerStateReads++;
#endif
        return GetStateInternal();
    }
    
    
    public void UpdateViewport(int x, int y, int width, int height)
    {
        UpdateViewportInternal(x, y, width, height);
    }
    
    
    public void Clear(float r, float g, float b, float a, ClearFlags flags)
    {
        ClearInternal(r, g, b, a, flags);
#if TOOLS
        Clears++;
#endif
    }
    
    
    public void SetWireframeMode(bool enabled)
    {
        SetWireframeModeInternal(enabled);
    }


    protected abstract void SetStateInternal(RasterizerState state, bool force = false);
    protected abstract void SetEnableDepthTestInternal(bool enable, bool force = false);
    protected abstract void SetEnableDepthWriteInternal(bool enable, bool force = false);
    protected abstract void SetEnableBlendingInternal(bool enable, bool force = false);
    protected abstract void SetEnableCullingInternal(bool enable, bool force = false);
    protected abstract void SetEnableScissorTestInternal(bool enable, bool force = false);
    protected abstract void SetScissorRectInternal(int index, int left, int bottom, int width, int height);
    protected abstract RasterizerState GetStateInternal();
    protected abstract void UpdateViewportInternal(int x, int y, int width, int height);
    protected abstract void ClearInternal(float r, float g, float b, float a, ClearFlags flags);
    protected abstract void SetWireframeModeInternal(bool enabled);

    #endregion
    
    
    #region Buffers

    /// <summary> Create a graphics buffer with the given type and data. </summary>
    public GraphicsBuffer CreateBuffer<T>(BufferType bufferType, T[] data, bool dynamic = false) where T : unmanaged
    {
        return CreateBufferInternal(bufferType, data, dynamic);
    }
    

    /// <summary> Set the data of the given buffer with the given data. </summary>
    public void SetBuffer<T>(GraphicsBuffer buffer, T[] data, bool dynamic = false) where T : unmanaged
    {
        SetBufferInternal(buffer, data, dynamic);
    }
    

    /// <summary> Update the given buffer with the given data at the given offset in bytes. </summary>
    public void UpdateBuffer<T>(GraphicsBuffer buffer, int offsetInBytes, T[] data) where T : unmanaged
    {
        UpdateBufferInternal(buffer, offsetInBytes, data);
    }
    
    
    public void UpdateBuffer(GraphicsBuffer buffer, int offsetInBytes, int sizeInBytes, nint data)
    {
        UpdateBufferInternal(buffer, offsetInBytes, sizeInBytes, data);
    }
    

    public void BindBuffer(GraphicsBuffer buffer)
    {
        BindBufferInternal(buffer);
    }
    

    /// <summary> Create a graphics buffer with the given type and data. </summary>
    protected abstract GraphicsBuffer CreateBufferInternal<T>(BufferType bufferType, T[] data, bool dynamic = false) where T : unmanaged;
    /// <summary> Set the data of the given buffer with the given data. </summary>
    protected abstract void SetBufferInternal<T>(GraphicsBuffer buffer, T[] data, bool dynamic = false) where T : unmanaged;
    /// <summary> Update the given buffer with the given data at the given offset in bytes. </summary>
    protected abstract void UpdateBufferInternal<T>(GraphicsBuffer buffer, int offsetInBytes, T[] data) where T : unmanaged;
    protected abstract void UpdateBufferInternal(GraphicsBuffer buffer, int offsetInBytes, int sizeInBytes, nint data);
    protected abstract void BindBufferInternal(GraphicsBuffer buffer);

    #endregion

    
    #region Vertex Arrays

    public GraphicsVertexArrayObject CreateVertexArray(MeshVertexLayout layout, GraphicsBuffer vertices, GraphicsBuffer? indices)
    {
        return CreateVertexArrayInternal(layout, vertices, indices);
    }

    
    public void BindVertexArray(GraphicsVertexArrayObject? vertexArrayObject)
    {
        BindVertexArrayInternal(vertexArrayObject);
    }


    protected abstract GraphicsVertexArrayObject CreateVertexArrayInternal(MeshVertexLayout layout, GraphicsBuffer vertices, GraphicsBuffer? indices);
    protected abstract void BindVertexArrayInternal(GraphicsVertexArrayObject? vertexArrayObject);

    #endregion
    
    
    #region Frame Buffers
    
    public GraphicsFrameBuffer CreateFramebuffer(GraphicsFrameBuffer.Attachment[] attachments)
    {
        return CreateFramebufferInternal(attachments);
    }

    
    public void UnbindFramebuffer()
    {
        UnbindFramebufferInternal();
    }

    
    public void BindFramebuffer(GraphicsFrameBuffer frameBuffer, FBOTarget target = FBOTarget.Framebuffer)
    {
        BindFramebufferInternal(frameBuffer, target);
    }
    

    public void BlitFramebuffer(int srcX0, int srcY0, int srcX1, int srcY1, int dstX0, int dstY0, int dstX1, int dstY1, ClearFlags clearFlags, BlitFilter filter)
    {
        BlitFramebufferInternal(srcX0, srcY0, srcX1, srcY1, dstX0, dstY0, dstX1, dstY1, clearFlags, filter);
    }


    public void ReadPixels<T>(int attachment, int x, int y, TextureImageFormat format, IntPtr output) where T : unmanaged
    {
        ReadPixelsInternal<T>(attachment, x, y, format, output);
    }
    
    
    public T ReadPixels<T>(int attachment, int x, int y, TextureImageFormat format) where T : unmanaged
    {
        return ReadPixelsInternal<T>(attachment, x, y, format);
    }


    protected abstract GraphicsFrameBuffer CreateFramebufferInternal(GraphicsFrameBuffer.Attachment[] attachments);
    protected abstract void UnbindFramebufferInternal();
    protected abstract void BindFramebufferInternal(GraphicsFrameBuffer frameBuffer, FBOTarget target = FBOTarget.Framebuffer);
    protected abstract void BlitFramebufferInternal(int srcX0, int srcY0, int srcX1, int srcY1, int dstX0, int dstY0, int dstX1, int dstY1, ClearFlags clearFlags, BlitFilter filter);
    protected abstract void ReadPixelsInternal<T>(int attachment, int x, int y, TextureImageFormat format, IntPtr output) where T : unmanaged;
    protected abstract T ReadPixelsInternal<T>(int attachment, int x, int y, TextureImageFormat format) where T : unmanaged;

    #endregion

    
    #region Shaders
    
    public GraphicsProgram CompileProgram(List<ShaderSourceDescriptor> shaders)
    {
        return CompileProgramInternal(shaders);
    }
    

    public void BindProgram(GraphicsProgram program)
    {
        BindProgramInternal(program);
    }
    

    public int GetUniformLocation(GraphicsProgram program, string name)
    {
        return GetUniformLocationInternal(program, name);
    }

    
    public int GetAttribLocation(GraphicsProgram program, string name)
    {
        return GetAttribLocationInternal(program, name);
    }
    

    public void SetUniformF(GraphicsProgram program, string name, float value)
    {
        SetUniformFInternal(program, name, value);
#if TOOLS
        ShaderUniformWrites++;
#endif
    }
    

    public void SetUniformF(GraphicsProgram program, int location, float value)
    {
        SetUniformFInternal(program, location, value);
#if TOOLS
        ShaderUniformWrites++;
#endif
    }

    
    public void SetUniformI(GraphicsProgram program, string name, int value)
    {
        SetUniformIInternal(program, name, value);
#if TOOLS
        ShaderUniformWrites++;
#endif
    }

    
    public void SetUniformI(GraphicsProgram program, int location, int value)
    {
        SetUniformIInternal(program, location, value);
#if TOOLS
        ShaderUniformWrites++;
#endif
    }

    
    public void SetUniformV2(GraphicsProgram program, string name, Vector2 value)
    {
        SetUniformV2Internal(program, name, value);
#if TOOLS
        ShaderUniformWrites++;
#endif
    }

    
    public void SetUniformV2(GraphicsProgram program, int location, Vector2 value)
    {
        SetUniformV2Internal(program, location, value);
#if TOOLS
        ShaderUniformWrites++;
#endif
    }


    public void SetUniformV3(GraphicsProgram program, string name, Vector3 value)
    {
        SetUniformV3Internal(program, name, value);
#if TOOLS
        ShaderUniformWrites++;
#endif
    }


    public void SetUniformV3(GraphicsProgram program, int location, Vector3 value)
    {
        SetUniformV3Internal(program, location, value);
#if TOOLS
        ShaderUniformWrites++;
#endif
    }


    public void SetUniformV4(GraphicsProgram program, string name, Vector4 value)
    {
        SetUniformV4Internal(program, name, value);
#if TOOLS
        ShaderUniformWrites++;
#endif
    }


    public void SetUniformV4(GraphicsProgram program, int location, Vector4 value)
    {
        SetUniformV4Internal(program, location, value);
#if TOOLS
        ShaderUniformWrites++;
#endif
    }


    public void SetUniformMatrix(GraphicsProgram program, string name, int length, bool transpose, in float m11)
    {
        SetUniformMatrixInternal(program, name, length, transpose, in m11);
#if TOOLS
        ShaderUniformWrites++;
#endif
    }


    public void SetUniformMatrix(GraphicsProgram program, int location, int length, bool transpose, in float m11)
    {
        SetUniformMatrixInternal(program, location, length, transpose, in m11);
#if TOOLS
        ShaderUniformWrites++;
#endif
    }


    public void SetUniformTexture(GraphicsProgram program, string name, int slot, GraphicsTexture texture)
    {
        SetUniformTextureInternal(program, name, slot, texture);
#if TOOLS
        ShaderUniformWrites++;
#endif
    }


    public void SetUniformTexture(GraphicsProgram program, int location, int slot, GraphicsTexture texture)
    {
        SetUniformTextureInternal(program, location, slot, texture);
#if TOOLS
        ShaderUniformWrites++;
#endif
    }


    // Explicitly clear a texture slot.
    // This is to reduce user error, since SetUniformTexture should throw if the texture is not available.
    public void ClearUniformTexture(GraphicsProgram program, string name, int slot)
    {
        ClearUniformTextureInternal(program, name, slot);
#if TOOLS
        ShaderUniformWrites++;
#endif
    }


    public void ClearUniformTexture(GraphicsProgram program, int location, int slot)
    {
        ClearUniformTextureInternal(program, location, slot);
#if TOOLS
        ShaderUniformWrites++;
#endif
    }


    protected abstract GraphicsProgram CompileProgramInternal(List<ShaderSourceDescriptor> shaders);
    protected abstract void BindProgramInternal(GraphicsProgram program);
    protected abstract int GetUniformLocationInternal(GraphicsProgram program, string name);
    protected abstract int GetAttribLocationInternal(GraphicsProgram program, string name);
    protected abstract void SetUniformFInternal(GraphicsProgram program, string name, float value);
    protected abstract void SetUniformFInternal(GraphicsProgram program, int location, float value);
    protected abstract void SetUniformIInternal(GraphicsProgram program, string name, int value);
    protected abstract void SetUniformIInternal(GraphicsProgram program, int location, int value);
    protected abstract void SetUniformV2Internal(GraphicsProgram program, string name, Vector2 value);
    protected abstract void SetUniformV2Internal(GraphicsProgram program, int location, Vector2 value);
    protected abstract void SetUniformV3Internal(GraphicsProgram program, string name, Vector3 value);
    protected abstract void SetUniformV3Internal(GraphicsProgram program, int location, Vector3 value);
    protected abstract void SetUniformV4Internal(GraphicsProgram program, string name, Vector4 value);
    protected abstract void SetUniformV4Internal(GraphicsProgram program, int location, Vector4 value);
    protected abstract void SetUniformMatrixInternal(GraphicsProgram program, string name, int length, bool transpose, in float m11);
    protected abstract void SetUniformMatrixInternal(GraphicsProgram program, int location, int length, bool transpose, in float m11);
    protected abstract void SetUniformTextureInternal(GraphicsProgram program, string name, int slot, GraphicsTexture texture);
    protected abstract void SetUniformTextureInternal(GraphicsProgram program, int location, int slot, GraphicsTexture texture);
    protected abstract void ClearUniformTextureInternal(GraphicsProgram program, string name, int slot);
    protected abstract void ClearUniformTextureInternal(GraphicsProgram program, int location, int slot);
    
    #endregion

    
    #region Textures

    public GraphicsTexture CreateTexture(TextureType type, TextureImageFormat format)
    {
        return CreateTextureInternal(type, format);
    }

    
    public void SetWrapS(GraphicsTexture texture, TextureWrap wrap)
    {
        SetWrapSInternal(texture, wrap);
    }

    
    public void SetWrapT(GraphicsTexture texture, TextureWrap wrap)
    {
        SetWrapTInternal(texture, wrap);
    }

    
    public void SetWrapR(GraphicsTexture texture, TextureWrap wrap)
    {
        SetWrapRInternal(texture, wrap);
    }

    
    public void SetTextureFilters(GraphicsTexture texture, TextureMin min, TextureMag mag)
    {
        SetTextureFiltersInternal(texture, min, mag);
    }

    
    public void GenerateMipmap(GraphicsTexture texture)
    {
        GenerateMipmapInternal(texture);
    }

    
    public void GetTexImage(GraphicsTexture texture, int mipLevel, nint data)
    {
        GetTexImageInternal(texture, mipLevel, data);
    }

    
    public void TexImage2D(GraphicsTexture texture, int mipLevel, int width, int height, int border, nint data)
    {
        TexImage2DInternal(texture, mipLevel, width, height, border, data);
    }

    
    public void TexImage2D(GraphicsTexture texture, CubemapFace face, int mipLevel, int width, int height, int border, nint data)
    {
        TexImage2DInternal(texture, face, mipLevel, width, height, border, data);
    }

    
    public void TexSubImage2D(GraphicsTexture texture, int mipLevel, int xOffset, int yOffset, int width, int height, nint data)
    {
        TexSubImage2DInternal(texture, mipLevel, xOffset, yOffset, width, height, data);
    }

    
    public void TexSubImage2D(GraphicsTexture texture, CubemapFace face, int mipLevel, int xOffset, int yOffset, int width, int height, nint data)
    {
        TexSubImage2DInternal(texture, face, mipLevel, xOffset, yOffset, width, height, data);
    }

    
    public void TexSubImage3D(GraphicsTexture texture, int mipLevel, int xOffset, int yOffset, int zOffset, int width, int height, int depth, nint data)
    {
        TexSubImage3DInternal(texture, mipLevel, xOffset, yOffset, zOffset, width, height, depth, data);
    }

    
    public void TexImage3D(GraphicsTexture texture, int mipLevel, int width, int height, int depth, int border, nint data)
    {
        TexImage3DInternal(texture, mipLevel, width, height, depth, border, data);
    }


    protected abstract GraphicsTexture CreateTextureInternal(TextureType type, TextureImageFormat format);
    protected abstract void SetWrapSInternal(GraphicsTexture texture, TextureWrap wrap);
    protected abstract void SetWrapTInternal(GraphicsTexture texture, TextureWrap wrap);
    protected abstract void SetWrapRInternal(GraphicsTexture texture, TextureWrap wrap);
    protected abstract void SetTextureFiltersInternal(GraphicsTexture texture, TextureMin min, TextureMag mag);
    protected abstract void GenerateMipmapInternal(GraphicsTexture texture);
    protected abstract void GetTexImageInternal(GraphicsTexture texture, int mipLevel, IntPtr data);
    protected abstract void TexImage2DInternal(GraphicsTexture texture, int mipLevel, int width, int height, int border, IntPtr data);
    protected abstract void TexImage2DInternal(GraphicsTexture texture, CubemapFace face, int mipLevel, int width, int height, int border, IntPtr data);
    protected abstract void TexSubImage2DInternal(GraphicsTexture texture, int mipLevel, int xOffset, int yOffset, int width, int height, IntPtr data);
    protected abstract void TexSubImage2DInternal(GraphicsTexture texture, CubemapFace face, int mipLevel, int xOffset, int yOffset, int width, int height, IntPtr data);
    protected abstract void TexSubImage3DInternal(GraphicsTexture texture, int mipLevel, int xOffset, int yOffset, int zOffset, int width, int height, int depth, IntPtr data);
    protected abstract void TexImage3DInternal(GraphicsTexture texture, int mipLevel, int width, int height, int depth, int border, IntPtr data);

    #endregion


    #region Drawing

    /// <summary>
    /// Draws the given primitive type with the given vertex count using the currently bound vertex array.
    /// </summary>
    /// <param name="topology">The type of primitive to render.</param>
    /// <param name="indexOffset">The starting index (offset) in the enabled vertex arrays from which to begin rendering.</param>
    /// <param name="count">The number of indices to render.</param>
    public void DrawArrays(Topology topology, int indexOffset, int count)
    {
        DrawArraysInternal(topology, indexOffset, count);

#if TOOLS
        RenderedTriangles += (ulong)GetTriangleCount(topology, count);
        RenderedVertices += (ulong)count;
        DrawCalls++;
#endif
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="topology">The type of primitive to render.</param>
    /// <param name="indexOffset">The starting index (offset) in the enabled vertex arrays from which to begin rendering.</param>
    /// <param name="count">The number of indices to render.</param>
    /// <param name="isIndex32Bit">Whether the indices are 32-bit or 16-bit.</param>
    public void DrawElements(Topology topology, int indexOffset, int count, bool isIndex32Bit)
    {
        DrawElementsInternal(topology, indexOffset, count, isIndex32Bit);

#if TOOLS
        RenderedTriangles += (ulong)GetTriangleCount(topology, count);
        RenderedVertices += (ulong)count;
        DrawCalls++;
#endif
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="topology">The type of primitive to render.</param>
    /// <param name="indexOffset">The starting index (offset) in the enabled vertex arrays from which to begin rendering.</param>
    /// <param name="count">The number of indices to render.</param>
    /// <param name="isIndex32Bit">Whether the indices are 32-bit or 16-bit.</param>
    /// <param name="vertexOffset">The offset added to each index in the index array before fetching the vertex data.</param>
    public void DrawElements(Topology topology, int indexOffset, int count, bool isIndex32Bit, int vertexOffset)
    {
        DrawElementsInternal(topology, indexOffset, count, isIndex32Bit, vertexOffset);

#if TOOLS
        RenderedTriangles += (ulong)GetTriangleCount(topology, count);
        RenderedVertices += (ulong)count;
        DrawCalls++;
#endif
    }


    protected abstract void DrawArraysInternal(Topology topology, int indexOffset, int count);
    protected abstract void DrawElementsInternal(Topology topology, int indexOffset, int count, bool isIndex32Bit);
    protected abstract void DrawElementsInternal(Topology topology, int indexOffset, int count, bool isIndex32Bit, int vertexOffset);


    private static int GetTriangleCount(Topology topology, int count)
    {
        switch (topology)
        {
            case Topology.Triangles:
                return count / 3;
            case Topology.TriangleStrip:
            case Topology.TriangleFan:
                return Math.Max(count - 2, 0);
            case Topology.Points:
            case Topology.Lines:
            case Topology.LineLoop:
            case Topology.LineStrip:
            case Topology.Quads:
            default:
                return 0;
        }
    }

    #endregion


#if TOOLS
    internal void ResetStatistics()
    {
        RenderedTriangles = 0;
        RenderedVertices = 0;
        DrawCalls = 0;
        RasterizerStateReads = 0;
        RasterizerStateWrites = 0;
        RasterizerStateOverrides = 0;
        ShaderUniformWrites = 0;
        TextureSwaps = 0;
        Clears = 0;
    }
#endif
}