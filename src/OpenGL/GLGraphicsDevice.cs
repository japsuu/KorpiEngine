using System.Runtime.InteropServices;
using KorpiEngine.Mathematics;
using KorpiEngine.Rendering;
using OpenTK.Graphics.OpenGL4;
using PType = OpenTK.Graphics.OpenGL4.PrimitiveType;

namespace KorpiEngine.OpenGL;

/// <summary>
/// OpenGL graphics driver.
/// </summary>
internal sealed unsafe class GLGraphicsDevice : GraphicsDevice
{
#if TOOLS
    private static readonly DebugProc DebugMessageDelegate = OnDebugMessage;
#endif

    public override GraphicsProgram? CurrentProgram => GLGraphicsProgram.CurrentProgram;

    private static readonly Dictionary<string, int> CachedUniformLocations = new();
    private static readonly Dictionary<string, int> CachedAttribLocations = new();

    // Current OpenGL State:
    private bool _depthTest = true;
    private bool _depthWrite = true;
    private DepthMode _depthMode = DepthMode.LessOrEqual;
    
    private bool _scissorTest = false;

    private bool _doBlend = true;
    private BlendType _blendSrc = BlendType.SrcAlpha;
    private BlendType _blendDst = BlendType.OneMinusSrcAlpha;
    private BlendMode _blendEquation = BlendMode.Add;

    private bool _doCull = true;
    private PolyFace _cullFace = PolyFace.Back;

    private WindingOrder _winding = WindingOrder.CCW;


    #region Initialization and Shutdown

    public override int MaxTextureSize { get; protected set; }
    public override int MaxCubeMapTextureSize { get; protected set; }
    public override int MaxArrayTextureLayers { get; protected set; }
    public override int MaxFramebufferColorAttachments { get; protected set; }


    protected override void InitializeInternal()
    {
#if TOOLS
        GL.DebugMessageCallback(DebugMessageDelegate, IntPtr.Zero);
        GL.Enable(EnableCap.DebugOutput);
        GL.Enable(EnableCap.DebugOutputSynchronous);
#endif

        // Smooth lines.
        GL.Enable(EnableCap.LineSmooth);

        MaxTextureSize = GL.GetInteger(GetPName.MaxTextureSize);
        MaxCubeMapTextureSize = GL.GetInteger(GetPName.MaxCubeMapTextureSize);
        MaxArrayTextureLayers = GL.GetInteger(GetPName.MaxArrayTextureLayers);
        MaxFramebufferColorAttachments = GL.GetInteger(GetPName.MaxColorAttachments);
    }


    protected override void ShutdownInternal()
    {
    }

    #endregion


    #region State

    protected override void SetStateInternal(RasterizerState state, bool force = false)
    {
        SetEnableDepthTest(state.EnableDepthTest, force);

        SetEnableDepthWrite(state.EnableDepthWrite, force);

        if (_depthMode != state.DepthMode || force)
        {
            GL.DepthFunc((DepthFunction)state.DepthMode);
            _depthMode = state.DepthMode;
        }
        
        SetEnableBlending(state.EnableBlend, force);

        if (_blendSrc != state.BlendSrc || _blendDst != state.BlendDst || force)
        {
            GL.BlendFunc((BlendingFactor)state.BlendSrc, (BlendingFactor)state.BlendDst);
            _blendSrc = state.BlendSrc;
            _blendDst = state.BlendDst;
        }

        if (_blendEquation != state.BlendMode || force)
        {
            GL.BlendEquation((BlendEquationMode)state.BlendMode);
            _blendEquation = state.BlendMode;
        }

        SetEnableCulling(state.EnableCulling, force);

        if (_cullFace != state.FaceCulling || force)
        {
            GL.CullFace((CullFaceMode)state.FaceCulling);
            _cullFace = state.FaceCulling;
        }

        if (_winding != state.WindingOrder || force)
        {
            GL.FrontFace((FrontFaceDirection)state.WindingOrder);
            _winding = state.WindingOrder;
        }
    }


    protected override void SetEnableDepthTestInternal(bool enable, bool force = false)
    {
        if (_depthTest == enable && !force)
            return;
        
        if (enable)
            GL.Enable(EnableCap.DepthTest);
        else
            GL.Disable(EnableCap.DepthTest);
        
        _depthTest = enable;
    }


    protected override void SetEnableDepthWriteInternal(bool enable, bool force = false)
    {
        if (_depthWrite == enable && !force)
            return;
        
        GL.DepthMask(enable);
        
        _depthWrite = enable;
    }


    protected override void SetEnableBlendingInternal(bool enable, bool force = false)
    {
        if (_doBlend == enable && !force)
            return;
        
        if (enable)
            GL.Enable(EnableCap.Blend);
        else
            GL.Disable(EnableCap.Blend);
        
        _doBlend = enable;
    }


    protected override void SetEnableCullingInternal(bool enable, bool force = false)
    {
        if (_doCull == enable && !force)
            return;
        
        if (enable)
            GL.Enable(EnableCap.CullFace);
        else
            GL.Disable(EnableCap.CullFace);
        
        _doCull = enable;
    }


    protected override void SetEnableScissorTestInternal(bool enable, bool force = false)
    {
        if (_scissorTest == enable && !force)
            return;
        
        if (enable)
            GL.Enable(EnableCap.ScissorTest);
        else
            GL.Disable(EnableCap.ScissorTest);
        
        _scissorTest = enable;
    }


    protected override void SetScissorRectInternal(int index, int left, int bottom, int width, int height)
    {
        GL.ScissorIndexed(index, left, bottom, width, height);
    }


    protected override RasterizerState GetStateInternal() =>
        new()
        {
            EnableDepthTest = _depthTest,
            EnableDepthWrite = _depthWrite,
            DepthMode = _depthMode,
            EnableBlend = _doBlend,
            BlendSrc = _blendSrc,
            BlendDst = _blendDst,
            BlendMode = _blendEquation,
            EnableCulling = _doCull,
            FaceCulling = _cullFace
        };


    protected override void UpdateViewportInternal(int x, int y, int width, int height)
    {
        GL.Viewport(x, y, width, height);
    }


    protected override void ClearInternal(float r, float g, float b, float a, ClearFlags flags)
    {
        GL.ClearColor(r, g, b, a);

        ClearBufferMask clearBufferMask = 0;
        if (flags.HasFlag(ClearFlags.Color))
            clearBufferMask |= ClearBufferMask.ColorBufferBit;
        if (flags.HasFlag(ClearFlags.Depth))
            clearBufferMask |= ClearBufferMask.DepthBufferBit;
        if (flags.HasFlag(ClearFlags.Stencil))
            clearBufferMask |= ClearBufferMask.StencilBufferBit;
        GL.Clear(clearBufferMask);
    }


    protected override void SetWireframeModeInternal(bool enabled)
    {
        GL.PolygonMode(MaterialFace.FrontAndBack, enabled ? PolygonMode.Line : PolygonMode.Fill);
    }

    #endregion


    #region Buffers

    protected override GraphicsBuffer CreateBufferInternal<T>(BufferType bufferType, T[] data, bool dynamic = false)
    {
        fixed (void* dat = data)
        {
            return new GLBuffer(bufferType, data.Length * sizeof(T), (nint)dat, dynamic);
        }
    }


    protected override void SetBufferInternal<T>(GraphicsBuffer buffer, T[] data, bool dynamic = false)
    {
        fixed (void* dat = data)
        {
            (buffer as GLBuffer)!.Set(data.Length * sizeof(T), (nint)dat, dynamic);
        }
    }


    protected override void UpdateBufferInternal<T>(GraphicsBuffer buffer, int offsetInBytes, T[] data)
    {
        fixed (void* ptr = data)
        {
            int sizeInBytes = data.Length * sizeof(T);
            UpdateBuffer(buffer, offsetInBytes, sizeInBytes, (nint)ptr);
        }
    }


    protected override void UpdateBufferInternal(GraphicsBuffer buffer, int offsetInBytes, int sizeInBytes, nint data)
    {
        (buffer as GLBuffer)!.Update(offsetInBytes, sizeInBytes, data);
    }


    protected override void BindBufferInternal(GraphicsBuffer buffer)
    {
        if (buffer is GLBuffer glBuffer)
            GL.BindBuffer(glBuffer.Target, glBuffer.Handle);
    }

    #endregion


    #region Vertex Arrays

    protected override GraphicsVertexArrayObject CreateVertexArrayInternal(MeshVertexLayout layout, GraphicsBuffer vertices, GraphicsBuffer? indices) =>
        new GLVertexArrayObject(layout, vertices, indices);


    protected override void BindVertexArrayInternal(GraphicsVertexArrayObject? vertexArrayObject)
    {
        GL.BindVertexArray((vertexArrayObject as GLVertexArrayObject)?.Handle ?? 0);
    }

    #endregion


    #region Frame Buffers

    protected override GraphicsFrameBuffer CreateFramebufferInternal(GraphicsFrameBuffer.Attachment[] attachments) => new GLFrameBuffer(attachments);

    protected override void UnbindFramebufferInternal() => GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);


    protected override void BindFramebufferInternal(GraphicsFrameBuffer frameBuffer, FBOTarget target = FBOTarget.Framebuffer)
    {
        GL.BindFramebuffer((FramebufferTarget)target, (frameBuffer as GLFrameBuffer)!.Handle);
    }


    protected override void BlitFramebufferInternal(int srcX0, int srcY0, int srcX1, int srcY1, int dstX0, int dstY0, int dstX1, int dstY1, ClearFlags clearFlags, BlitFilter filter)
    {
        ClearBufferMask clearBufferMask = 0;
        if (clearFlags.HasFlag(ClearFlags.Color))
            clearBufferMask |= ClearBufferMask.ColorBufferBit;
        if (clearFlags.HasFlag(ClearFlags.Depth))
            clearBufferMask |= ClearBufferMask.DepthBufferBit;
        if (clearFlags.HasFlag(ClearFlags.Stencil))
            clearBufferMask |= ClearBufferMask.StencilBufferBit;

        BlitFramebufferFilter nearest = filter switch
        {
            BlitFilter.Nearest => BlitFramebufferFilter.Nearest,
            BlitFilter.Linear => BlitFramebufferFilter.Linear,
            _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, null)
        };

        GL.BlitFramebuffer(srcX0, srcY0, srcX1, srcY1, dstX0, dstY0, dstX1, dstY1, clearBufferMask, nearest);
    }


    protected override void ReadPixelsInternal<T>(int attachment, int x, int y, TextureImageFormat format, IntPtr output)
    {
        GL.ReadBuffer((ReadBufferMode)((int)ReadBufferMode.ColorAttachment0 + attachment));
        GLTexture.GetTextureFormatEnums(format, out PixelInternalFormat _, out PixelType pixelType, out PixelFormat pixelFormat);
        GL.ReadPixels(x, y, 1, 1, pixelFormat, pixelType, output);
    }


    protected override T ReadPixelsInternal<T>(int attachment, int x, int y, TextureImageFormat format)
    {
        GL.ReadBuffer((ReadBufferMode)((int)ReadBufferMode.ColorAttachment0 + attachment));
        GLTexture.GetTextureFormatEnums(format, out PixelInternalFormat _, out PixelType pixelType, out PixelFormat pixelFormat);
        T result = default;
        GL.ReadPixels(x, y, 1, 1, pixelFormat, pixelType, ref result);
        return result;
    }

    #endregion


    #region Shaders

    protected override GraphicsProgram CompileProgramInternal(List<ShaderSourceDescriptor> shaders) => GLGraphicsProgramFactory.Create(shaders);

    protected override void BindProgramInternal(GraphicsProgram program) => (program as GLGraphicsProgram)!.Use();


    protected override int GetUniformLocationInternal(GraphicsProgram program, string name)
    {
        string key = $"{program}:{name}";
        if (CachedUniformLocations.TryGetValue(key, out int loc))
            return loc;

        BindProgram(program);
        int newLoc = GL.GetUniformLocation((program as GLGraphicsProgram)!.Handle, name);
        CachedUniformLocations[name] = newLoc;

        return newLoc;
    }


    protected override int GetAttribLocationInternal(GraphicsProgram program, string name)
    {
        string key = $"{program}:{name}";
        if (CachedAttribLocations.TryGetValue(key, out int loc))
            return loc;

        BindProgram(program);
        int newLoc = GL.GetAttribLocation((program as GLGraphicsProgram)!.Handle, name);
        CachedAttribLocations[name] = newLoc;

        return newLoc;
    }


    protected override void SetUniformFInternal(GraphicsProgram program, string name, float value)
    {
        int loc = GetUniformLocation(program, name);
        SetUniformF(program, loc, value);
    }


    protected override void SetUniformFInternal(GraphicsProgram program, int location, float value)
    {
        if (location == -1)
            return;

        BindProgram(program);
        GL.Uniform1(location, value);
    }


    protected override void SetUniformIInternal(GraphicsProgram program, string name, int value)
    {
        int loc = GetUniformLocation(program, name);
        SetUniformI(program, loc, value);
    }


    protected override void SetUniformIInternal(GraphicsProgram program, int location, int value)
    {
        if (location == -1)
            return;

        BindProgram(program);
        GL.Uniform1(location, value);
    }


    protected override void SetUniformV2Internal(GraphicsProgram program, string name, Vector2 value)
    {
        int loc = GetUniformLocation(program, name);
        SetUniformV2(program, loc, value);
    }


    protected override void SetUniformV2Internal(GraphicsProgram program, int location, Vector2 value)
    {
        if (location == -1)
            return;

        BindProgram(program);
        GL.Uniform2(location, new OpenTK.Mathematics.Vector2((float)value.X, (float)value.Y));
    }


    protected override void SetUniformV3Internal(GraphicsProgram program, string name, Vector3 value)
    {
        int loc = GetUniformLocation(program, name);
        SetUniformV3(program, loc, value);
    }


    protected override void SetUniformV3Internal(GraphicsProgram program, int location, Vector3 value)
    {
        if (location == -1)
            return;

        BindProgram(program);
        GL.Uniform3(location, new OpenTK.Mathematics.Vector3((float)value.X, (float)value.Y, (float)value.Z));
    }


    protected override void SetUniformV4Internal(GraphicsProgram program, string name, Vector4 value)
    {
        int loc = GetUniformLocation(program, name);
        SetUniformV4(program, loc, value);
    }


    protected override void SetUniformV4Internal(GraphicsProgram program, int location, Vector4 value)
    {
        if (location == -1)
            return;

        BindProgram(program);
        GL.Uniform4(location, new OpenTK.Mathematics.Vector4((float)value.X, (float)value.Y, (float)value.Z, (float)value.W));
    }


    protected override void SetUniformMatrixInternal(GraphicsProgram program, string name, int length, bool transpose, in float m11)
    {
        int loc = GetUniformLocation(program, name);
        SetUniformMatrix(program, loc, length, transpose, m11);
    }


    protected override void SetUniformMatrixInternal(GraphicsProgram program, int location, int length, bool transpose, in float m11)
    {
        if (location == -1)
            return;

        BindProgram(program);
        fixed (float* ptr = &m11)
        {
            GL.UniformMatrix4(location, length, transpose, ptr);
        }
    }


    protected override void SetUniformTextureInternal(GraphicsProgram program, string name, int slot, GraphicsTexture texture)
    {
        int loc = GetUniformLocation(program, name);
        SetUniformTexture(program, loc, slot, texture);
    }


    protected override void SetUniformTextureInternal(GraphicsProgram program, int location, int slot, GraphicsTexture texture)
    {
        if (location == -1)
            return;

        GLTexture glTexture = (texture as GLTexture)!;

        BindProgram(program);
        GL.ActiveTexture((TextureUnit)((uint)TextureUnit.Texture0 + slot));
        glTexture.Bind();
        GL.Uniform1(location, slot);
    }


    protected override void ClearUniformTextureInternal(GraphicsProgram program, string name, int slot)
    {
        int loc = GetUniformLocation(program, name);
        ClearUniformTexture(program, loc, slot);
    }


    protected override void ClearUniformTextureInternal(GraphicsProgram program, int location, int slot)
    {
        if (location == -1)
            return;

        BindProgram(program);
        GL.ActiveTexture((TextureUnit)((uint)TextureUnit.Texture0 + slot));
        GL.BindTexture(TextureTarget.Texture2D, 0);
        GL.Uniform1(location, 0);
#if TOOLS
        TextureSwaps++;
#endif
    }

    #endregion


    #region Textures

    protected override GraphicsTexture CreateTextureInternal(TextureType type, TextureImageFormat format) => new GLTexture(type, format);

    protected override void SetWrapSInternal(GraphicsTexture texture, TextureWrap wrap) => (texture as GLTexture)!.SetWrapS(wrap);

    protected override void SetWrapTInternal(GraphicsTexture texture, TextureWrap wrap) => (texture as GLTexture)!.SetWrapT(wrap);

    protected override void SetWrapRInternal(GraphicsTexture texture, TextureWrap wrap) => (texture as GLTexture)!.SetWrapR(wrap);

    protected override void SetTextureFiltersInternal(GraphicsTexture texture, TextureMin min, TextureMag mag) => (texture as GLTexture)!.SetTextureFilters(min, mag);

    protected override void GenerateMipmapInternal(GraphicsTexture texture) => (texture as GLTexture)!.GenerateMipmap();

    protected override void GetTexImageInternal(GraphicsTexture texture, int mipLevel, IntPtr data) => (texture as GLTexture)!.GetTexImage(mipLevel, data);


    protected override void TexImage2DInternal(GraphicsTexture texture, int mipLevel, int width, int height, int border, IntPtr data) =>
        (texture as GLTexture)!.TexImage2D((texture as GLTexture)!.Target, mipLevel, width, height, border, data);


    protected override void TexImage2DInternal(GraphicsTexture texture, CubemapFace face, int mipLevel, int width, int height, int border, IntPtr data) =>
        (texture as GLTexture)!.TexImage2D((TextureTarget)face, mipLevel, width, height, border, data);


    protected override void TexImage3DInternal(GraphicsTexture texture, int mipLevel, int width, int height, int depth, int border, IntPtr data) =>
        (texture as GLTexture)!.TexImage3D((texture as GLTexture)!.Target, mipLevel, width, height, depth, border, data);


    protected override void TexSubImage2DInternal(GraphicsTexture texture, int mipLevel, int xOffset, int yOffset, int width, int height, IntPtr data) =>
        (texture as GLTexture)!.TexSubImage2D((texture as GLTexture)!.Target, mipLevel, xOffset, yOffset, width, height, data);


    protected override void TexSubImage2DInternal(GraphicsTexture texture, CubemapFace face, int mipLevel, int xOffset, int yOffset, int width, int height,
        IntPtr data) =>
        (texture as GLTexture)!.TexSubImage2D((TextureTarget)face, mipLevel, xOffset, yOffset, width, height, data);


    protected override void TexSubImage3DInternal(GraphicsTexture texture, int mipLevel, int xOffset, int yOffset, int zOffset, int width, int height, int depth,
        IntPtr data) =>
        (texture as GLTexture)!.TexSubImage3D((texture as GLTexture)!.Target, mipLevel, xOffset, yOffset, zOffset, width, height, depth, data);

    #endregion


    #region Drawing

    protected override void DrawArraysInternal(Topology topology, int indexOffset, int count)
    {
        PType mode = topology switch
        {
            Topology.Points => PType.Points,
            Topology.Lines => PType.Lines,
            Topology.LineLoop => PType.LineLoop,
            Topology.LineStrip => PType.LineStrip,
            Topology.Triangles => PType.Triangles,
            Topology.TriangleStrip => PType.TriangleStrip,
            Topology.TriangleFan => PType.TriangleFan,
            Topology.Quads => PType.Quads,
            _ => throw new ArgumentOutOfRangeException(nameof(topology), topology, null)
        };
        GL.DrawArrays(mode, indexOffset, count);
    }


    protected override void DrawElementsInternal(Topology topology, int indexOffset, int count, bool isIndex32Bit)
    {
        PType mode = topology switch
        {
            Topology.Points => PType.Points,
            Topology.Lines => PType.Lines,
            Topology.LineLoop => PType.LineLoop,
            Topology.LineStrip => PType.LineStrip,
            Topology.Triangles => PType.Triangles,
            Topology.TriangleStrip => PType.TriangleStrip,
            Topology.TriangleFan => PType.TriangleFan,
            Topology.Quads => PType.Quads,
            _ => throw new ArgumentOutOfRangeException(nameof(topology), topology, null)
        };
        GL.DrawElements(mode, count, isIndex32Bit ? DrawElementsType.UnsignedInt : DrawElementsType.UnsignedShort, indexOffset);
    }


    protected override void DrawElementsInternal(Topology topology, int indexOffset, int count, bool isIndex32Bit, int vertexOffset)
    {
        PType mode = topology switch
        {
            Topology.Points => PType.Points,
            Topology.Lines => PType.Lines,
            Topology.LineLoop => PType.LineLoop,
            Topology.LineStrip => PType.LineStrip,
            Topology.Triangles => PType.Triangles,
            Topology.TriangleStrip => PType.TriangleStrip,
            Topology.TriangleFan => PType.TriangleFan,
            Topology.Quads => PType.Quads,
            _ => throw new ArgumentOutOfRangeException(nameof(topology), topology, null)
        };
        GL.DrawElementsBaseVertex(mode, count, isIndex32Bit ? DrawElementsType.UnsignedInt : DrawElementsType.UnsignedShort, indexOffset, vertexOffset);
    }

    #endregion


    #region Debugging

#if TOOLS
    private static void OnDebugMessage(
        DebugSource source, // Source of the debugging message.
        DebugType type, // Type of the debugging message.
        int id, // ID associated with the message.
        DebugSeverity severity, // Severity of the message.
        int length, // Length of the string in pMessage.
        IntPtr pMessage, // Pointer to message string.
        IntPtr pUserParam)
    {
        if (severity == DebugSeverity.DebugSeverityNotification)
            return;

        // To access the string pointed to by pMessage, you can use Marshal
        // class to copy its contents to a C# string without unsafe code. You can
        // also use the new function Marshal.PtrToStringUTF8 since .NET Core 1.1.
        string message = Marshal.PtrToStringAnsi(pMessage, length);

        Application.Logger.Debug($"[OpenGL-{severity} source={source} type={type} id={id}] {message}");

        if (type == DebugType.DebugTypeError)
            throw new GLException(message);
    }
    
    
    public static void Assert(string errorMessage)
    {
        Assert(GL.GetError(), ErrorCode.NoError, errorMessage);
    }


    public static void Assert(ErrorCode desiredErrorCode, string errorMessage)
    {
        Assert(GL.GetError(), desiredErrorCode, errorMessage);
    }


    public static void Assert(ErrorCode value, ErrorCode desiredValue, string errorMessage)
    {
        if (value == desiredValue)
            return;
        
        throw new GLException($"Assertion failed. ErrorCode: {value}\n{errorMessage}");
    }
#endif

    #endregion
}