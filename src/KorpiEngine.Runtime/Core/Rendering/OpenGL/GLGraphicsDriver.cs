using System.Runtime.InteropServices;
using KorpiEngine.Core.API;
using KorpiEngine.Core.API.Rendering.Shaders;
using KorpiEngine.Core.Internal.Rendering;
using KorpiEngine.Core.Platform;
using KorpiEngine.Core.Rendering.Primitives;
using OpenTK.Graphics.OpenGL4;
using PType = OpenTK.Graphics.OpenGL4.PrimitiveType;

namespace KorpiEngine.Core.Rendering.OpenGL;

/// <summary>
/// OpenGL graphics driver.
/// </summary>
internal sealed unsafe class GLGraphicsDriver : GraphicsDriver
{
#if DEBUG
    private static readonly DebugProc DebugMessageDelegate = OnDebugMessage;
#endif

    public override GraphicsProgram? CurrentProgram => GLGraphicsProgram.CurrentProgram;

    private static readonly Dictionary<string, int> CachedUniformLocations = new();
    private static readonly Dictionary<string, int> CachedAttribLocations = new();

    // Current OpenGL State:
    private bool _depthTest = true;
    private bool _depthWrite = true;
    private DepthMode _depthMode = DepthMode.LessOrEqual;

    private bool _doBlend = true;
    private BlendType _blendSrc = BlendType.SrcAlpha;
    private BlendType _blendDst = BlendType.OneMinusSrcAlpha;
    private BlendMode _blendEquation = BlendMode.Add;

    private bool _doCull = true;
    private PolyFace _cullFace = PolyFace.Back;

    private WindingOrder _winding = WindingOrder.CW;


    #region Initialization and Shutdown

    protected override void InitializeInternal()
    {
#if DEBUG
        GL.DebugMessageCallback(DebugMessageDelegate, IntPtr.Zero);
        GL.Enable(EnableCap.DebugOutput);
        GL.Enable(EnableCap.DebugOutputSynchronous);
#endif

        // Smooth lines.
        GL.Enable(EnableCap.LineSmooth);

        SystemInfo.MaxTextureSize = GL.GetInteger(GetPName.MaxTextureSize);
        SystemInfo.MaxCubeMapTextureSize = GL.GetInteger(GetPName.MaxCubeMapTextureSize);
        SystemInfo.MaxArrayTextureLayers = GL.GetInteger(GetPName.MaxArrayTextureLayers);
        SystemInfo.MaxFramebufferColorAttachments = GL.GetInteger(GetPName.MaxColorAttachments);
    }


    protected override void ShutdownInternal()
    {
    }

    #endregion


    #region State

    public override void SetState(RasterizerState state, bool force = false)
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


    public override void SetEnableDepthTest(bool enable, bool force = false)
    {
        if (_depthTest == enable && !force)
            return;
        
        if (enable)
            GL.Enable(EnableCap.DepthTest);
        else
            GL.Disable(EnableCap.DepthTest);
        
        _depthTest = enable;
    }


    public override void SetEnableDepthWrite(bool enable, bool force = false)
    {
        if (_depthWrite == enable && !force)
            return;
        
        GL.DepthMask(enable);
        
        _depthWrite = enable;
    }


    public override void SetEnableBlending(bool enable, bool force = false)
    {
        if (_doBlend == enable && !force)
            return;
        
        if (enable)
            GL.Enable(EnableCap.Blend);
        else
            GL.Disable(EnableCap.Blend);
        
        _doBlend = enable;
    }


    public override void SetEnableCulling(bool enable, bool force = false)
    {
        if (_doCull == enable && !force)
            return;
        
        if (enable)
            GL.Enable(EnableCap.CullFace);
        else
            GL.Disable(EnableCap.CullFace);
        
        _doCull = enable;
    }


    public override RasterizerState GetState() =>
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


    public override void UpdateViewport(int x, int y, int width, int height)
    {
        GL.Viewport(x, y, width, height);
    }


    public override void Clear(float r, float g, float b, float a, ClearFlags flags)
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


    public override void SetWireframeMode(bool enabled)
    {
        GL.PolygonMode(MaterialFace.FrontAndBack, enabled ? PolygonMode.Line : PolygonMode.Fill);
    }

    #endregion


    #region Buffers

    public override GraphicsBuffer CreateBuffer<T>(BufferType bufferType, T[] data, bool dynamic = false)
    {
        fixed (void* dat = data)
        {
            return new GLBuffer(bufferType, data.Length * sizeof(T), dat, dynamic);
        }
    }


    public override void SetBuffer<T>(GraphicsBuffer buffer, T[] data, bool dynamic = false)
    {
        fixed (void* dat = data)
        {
            (buffer as GLBuffer)!.Set(data.Length * sizeof(T), dat, dynamic);
        }
    }


    public override void UpdateBuffer<T>(GraphicsBuffer buffer, int offsetInBytes, T[] data)
    {
        fixed (void* dat = data)
        {
            (buffer as GLBuffer)!.Update(offsetInBytes, data.Length * sizeof(T), dat);
        }
    }


    public override void BindBuffer(GraphicsBuffer buffer)
    {
        if (buffer is GLBuffer glBuffer)
            GL.BindBuffer(glBuffer.Target, glBuffer.Handle);
    }

    #endregion


    #region Vertex Arrays

    public override GraphicsVertexArrayObject CreateVertexArray(MeshVertexLayout layout, GraphicsBuffer vertices, GraphicsBuffer? indices) =>
        new GLVertexArrayObject(layout, vertices, indices);


    public override void BindVertexArray(GraphicsVertexArrayObject? vertexArrayObject)
    {
        GL.BindVertexArray((vertexArrayObject as GLVertexArrayObject)?.Handle ?? 0);
    }

    #endregion


    #region Frame Buffers

    public override GraphicsFrameBuffer CreateFramebuffer(GraphicsFrameBuffer.Attachment[] attachments) => new GLFrameBuffer(attachments);

    public override void UnbindFramebuffer() => GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);


    public override void BindFramebuffer(GraphicsFrameBuffer frameBuffer, FBOTarget target = FBOTarget.Framebuffer)
    {
        GL.BindFramebuffer((FramebufferTarget)target, (frameBuffer as GLFrameBuffer)!.Handle);
    }


    public override void BlitFramebuffer(int v1, int v2, int width, int height, int v3, int v4, int v5, int v6, ClearFlags v, BlitFilter filter)
    {
        ClearBufferMask clearBufferMask = 0;
        if (v.HasFlag(ClearFlags.Color))
            clearBufferMask |= ClearBufferMask.ColorBufferBit;
        if (v.HasFlag(ClearFlags.Depth))
            clearBufferMask |= ClearBufferMask.DepthBufferBit;
        if (v.HasFlag(ClearFlags.Stencil))
            clearBufferMask |= ClearBufferMask.StencilBufferBit;

        BlitFramebufferFilter nearest = filter switch
        {
            BlitFilter.Nearest => BlitFramebufferFilter.Nearest,
            BlitFilter.Linear => BlitFramebufferFilter.Linear,
            _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, null)
        };

        GL.BlitFramebuffer(v1, v2, width, height, v3, v4, v5, v6, clearBufferMask, nearest);
    }


    public override void ReadPixels<T>(int attachment, int x, int y, TextureImageFormat format, IntPtr output)
    {
        GL.ReadBuffer((ReadBufferMode)((int)ReadBufferMode.ColorAttachment0 + attachment));
        GLTexture.GetTextureFormatEnums(format, out PixelInternalFormat _, out PixelType pixelType, out PixelFormat pixelFormat);
        GL.ReadPixels(x, y, 1, 1, pixelFormat, pixelType, output);
    }


    public override T ReadPixels<T>(int attachment, int x, int y, TextureImageFormat format)
    {
        GL.ReadBuffer((ReadBufferMode)((int)ReadBufferMode.ColorAttachment0 + attachment));
        GLTexture.GetTextureFormatEnums(format, out PixelInternalFormat _, out PixelType pixelType, out PixelFormat pixelFormat);

        T result = default;
        GCHandle handle = GCHandle.Alloc(result, GCHandleType.Pinned);
        GL.ReadPixels(x, y, 1, 1, pixelFormat, pixelType, handle.AddrOfPinnedObject());
        handle.Free();

        return result;
    }

    #endregion


    #region Shaders

    public override GraphicsProgram CompileProgram(List<ShaderSourceDescriptor> shaders) => GLGraphicsProgramFactory.Create(shaders);


    public override void BindProgram(GraphicsProgram program) => (program as GLGraphicsProgram)!.Use();


    public override int GetUniformLocation(GraphicsProgram program, string name)
    {
        string key = $"{program}:{name}";
        if (CachedUniformLocations.TryGetValue(key, out int loc))
            return loc;

        BindProgram(program);
        int newLoc = GL.GetUniformLocation((program as GLGraphicsProgram)!.Handle, name);
        CachedUniformLocations[name] = newLoc;

        return newLoc;
    }


    public override int GetAttribLocation(GraphicsProgram program, string name)
    {
        string key = $"{program}:{name}";
        if (CachedAttribLocations.TryGetValue(key, out int loc))
            return loc;

        BindProgram(program);
        int newLoc = GL.GetAttribLocation((program as GLGraphicsProgram)!.Handle, name);
        CachedAttribLocations[name] = newLoc;

        return newLoc;
    }


    public override void SetUniformF(GraphicsProgram program, string name, float value)
    {
        int loc = GetUniformLocation(program, name);
        SetUniformF(program, loc, value);
    }


    public override void SetUniformI(GraphicsProgram program, string name, int value)
    {
        int loc = GetUniformLocation(program, name);
        SetUniformI(program, loc, value);
    }


    public override void SetUniformV2(GraphicsProgram program, string name, Vector2 value)
    {
        int loc = GetUniformLocation(program, name);
        SetUniformV2(program, loc, value);
    }


    public override void SetUniformV3(GraphicsProgram program, string name, Vector3 value)
    {
        int loc = GetUniformLocation(program, name);
        SetUniformV3(program, loc, value);
    }


    public override void SetUniformV4(GraphicsProgram program, string name, Vector4 value)
    {
        int loc = GetUniformLocation(program, name);
        SetUniformV4(program, loc, value);
    }


    public override void SetUniformMatrix(GraphicsProgram program, string name, int length, bool transpose, in float m11)
    {
        int loc = GetUniformLocation(program, name);
        SetUniformMatrix(program, loc, length, transpose, m11);
    }


    public override void SetUniformTexture(GraphicsProgram program, string name, int slot, GraphicsTexture texture)
    {
        int loc = GetUniformLocation(program, name);
        SetUniformTexture(program, loc, slot, texture);
    }


    public override void SetUniformF(GraphicsProgram program, int loc, float value)
    {
        if (loc == -1)
            return;

        BindProgram(program);
        GL.Uniform1(loc, value);
    }


    public override void SetUniformI(GraphicsProgram program, int loc, int value)
    {
        if (loc == -1)
            return;

        BindProgram(program);
        GL.Uniform1(loc, value);
    }


    public override void SetUniformV2(GraphicsProgram program, int loc, Vector2 value)
    {
        if (loc == -1)
            return;

        BindProgram(program);
        GL.Uniform2(loc, new OpenTK.Mathematics.Vector2((float)value.X, (float)value.Y));
    }


    public override void SetUniformV3(GraphicsProgram program, int loc, Vector3 value)
    {
        if (loc == -1)
            return;

        BindProgram(program);
        GL.Uniform3(loc, new OpenTK.Mathematics.Vector3((float)value.X, (float)value.Y, (float)value.Z));
    }


    public override void SetUniformV4(GraphicsProgram program, int loc, Vector4 value)
    {
        if (loc == -1)
            return;

        BindProgram(program);
        GL.Uniform4(loc, new OpenTK.Mathematics.Vector4((float)value.X, (float)value.Y, (float)value.Z, (float)value.W));
    }


    public override void SetUniformMatrix(GraphicsProgram program, int loc, int length, bool transpose, in float m11)
    {
        if (loc == -1)
            return;

        BindProgram(program);
        fixed (float* ptr = &m11)
        {
            GL.UniformMatrix4(loc, length, transpose, ptr);
        }
    }


    public override void SetUniformTexture(GraphicsProgram program, int loc, int slot, GraphicsTexture texture)
    {
        if (loc == -1)
            return;

        GLTexture glTexture = (texture as GLTexture)!;

        BindProgram(program);
        GL.ActiveTexture((TextureUnit)((uint)TextureUnit.Texture0 + slot));
        GL.BindTexture(glTexture.Target, glTexture.Handle);
        GL.Uniform1(loc, slot);
    }


    public override void ClearUniformTexture(GraphicsProgram program, string name, int slot)
    {
        int loc = GetUniformLocation(program, name);
        ClearUniformTexture(program, loc, slot);
    }


    public override void ClearUniformTexture(GraphicsProgram program, int location, int slot)
    {
        if (location == -1)
            return;

        BindProgram(program);
        GL.ActiveTexture((TextureUnit)((uint)TextureUnit.Texture0 + slot));
        GL.BindTexture(TextureTarget.Texture2D, 0);
        GL.Uniform1(location, 0);
    }

    #endregion


    #region Textures

    public override GraphicsTexture CreateTexture(TextureType type, TextureImageFormat format) => new GLTexture(type, format);

    public override void SetWrapS(GraphicsTexture texture, TextureWrap wrap) => (texture as GLTexture)!.SetWrapS(wrap);

    public override void SetWrapT(GraphicsTexture texture, TextureWrap wrap) => (texture as GLTexture)!.SetWrapT(wrap);

    public override void SetWrapR(GraphicsTexture texture, TextureWrap wrap) => (texture as GLTexture)!.SetWrapR(wrap);

    public override void SetTextureFilters(GraphicsTexture texture, TextureMin min, TextureMag mag) => (texture as GLTexture)!.SetTextureFilters(min, mag);

    public override void GenerateMipmap(GraphicsTexture texture) => (texture as GLTexture)!.GenerateMipmap();

    public override void GetTexImage(GraphicsTexture texture, int mipLevel, void* data) => (texture as GLTexture)!.GetTexImage(mipLevel, data);


    public override void TexImage2D(GraphicsTexture texture, int mipLevel, int width, int height, int border, void* data) =>
        (texture as GLTexture)!.TexImage2D((texture as GLTexture)!.Target, mipLevel, width, height, border, data);


    public override void TexImage2D(GraphicsTexture texture, CubemapFace face, int mipLevel, int width, int height, int border, void* data) =>
        (texture as GLTexture)!.TexImage2D((TextureTarget)face, mipLevel, width, height, border, data);


    public override void TexImage3D(GraphicsTexture texture, int mipLevel, int width, int height, int depth, int border, void* data) =>
        (texture as GLTexture)!.TexImage3D((texture as GLTexture)!.Target, mipLevel, width, height, depth, border, data);


    public override void TexSubImage2D(GraphicsTexture texture, int mipLevel, int xOffset, int yOffset, int width, int height, void* data) =>
        (texture as GLTexture)!.TexSubImage2D((texture as GLTexture)!.Target, mipLevel, xOffset, yOffset, width, height, data);


    public override void TexSubImage2D(GraphicsTexture texture, CubemapFace face, int mipLevel, int xOffset, int yOffset, int width, int height, void* data) =>
        (texture as GLTexture)!.TexSubImage2D((TextureTarget)face, mipLevel, xOffset, yOffset, width, height, data);


    public override void TexSubImage3D(GraphicsTexture texture, int mipLevel, int xOffset, int yOffset, int zOffset, int width, int height, int depth,
        void* data) =>
        (texture as GLTexture)!.TexSubImage3D((texture as GLTexture)!.Target, mipLevel, xOffset, yOffset, zOffset, width, height, depth, data);

    #endregion


    #region Drawing

    public override void DrawArrays(Topology primitiveType, int startIndex, int count)
    {
        PType mode = primitiveType switch
        {
            Topology.Points => PType.Points,
            Topology.Lines => PType.Lines,
            Topology.LineLoop => PType.LineLoop,
            Topology.LineStrip => PType.LineStrip,
            Topology.Triangles => PType.Triangles,
            Topology.TriangleStrip => PType.TriangleStrip,
            Topology.TriangleFan => PType.TriangleFan,
            Topology.Quads => PType.Quads,
            _ => throw new ArgumentOutOfRangeException(nameof(primitiveType), primitiveType, null)
        };
        GL.DrawArrays(mode, startIndex, count);
    }


    public override void DrawElements(Topology triangles, int indexCount, bool isIndex32Bit, void* indexOffset)
    {
        PType mode = triangles switch
        {
            Topology.Points => PType.Points,
            Topology.Lines => PType.Lines,
            Topology.LineLoop => PType.LineLoop,
            Topology.LineStrip => PType.LineStrip,
            Topology.Triangles => PType.Triangles,
            Topology.TriangleStrip => PType.TriangleStrip,
            Topology.TriangleFan => PType.TriangleFan,
            Topology.Quads => PType.Quads,
            _ => throw new ArgumentOutOfRangeException(nameof(triangles), triangles, null)
        };
        GL.DrawElements(mode, indexCount, isIndex32Bit ? DrawElementsType.UnsignedInt : DrawElementsType.UnsignedShort, (IntPtr)indexOffset);
    }

    #endregion


    #region Debugging

#if DEBUG
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

        // In order to access the string pointed to by pMessage, you can use Marshal
        // class to copy its contents to a C# string without unsafe code. You can
        // also use the new function Marshal.PtrToStringUTF8 since .NET Core 1.1.
        string message = Marshal.PtrToStringAnsi(pMessage, length);

        Logger.OpenGl($"[{severity} source={source} type={type} id={id}] {message}");

        if (type == DebugType.DebugTypeError)
            throw new Exception(message);
    }
#endif

    #endregion
}