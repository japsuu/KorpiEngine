using System.Runtime.InteropServices;
using KorpiEngine.Core.Rendering.Primitives;
using KorpiEngine.Core.Rendering.Shaders;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace KorpiEngine.Core.Rendering.OpenGL;

/// <summary>
/// OpenGL graphics driver.
/// </summary>
public sealed unsafe class GLGraphicsDriver : GraphicsDriver
{
#if DEBUG
    private static readonly DebugProc DebugMessageDelegate = OnDebugMessage;
#endif

    public override GraphicsProgram? CurrentProgram => GLGraphicsProgram.CurrentProgram;

    public static readonly Dictionary<string, int> CachedUniformLocations = new();
    public static readonly Dictionary<string, int> CachedAttribLocations = new();


    #region Initialization and Shutdown

    protected override void InitializeInternal()
    {
#if DEBUG
        GL.DebugMessageCallback(DebugMessageDelegate, IntPtr.Zero);
        GL.Enable(EnableCap.DebugOutput);
        GL.Enable(EnableCap.DebugOutputSynchronous);
#endif

        // Smooth lines
        GL.Enable(EnableCap.LineSmooth);

        // Textures
        Graphics.MaxTextureSize = GL.GetInteger(GLEnum.MaxTextureSize);
        Graphics.MaxCubeMapTextureSize = GL.GetInteger(GLEnum.MaxCubeMapTextureSize);
        Graphics.MaxArrayTextureLayers = GL.GetInteger(GLEnum.MaxArrayTextureLayers);
        Graphics.MaxFramebufferColorAttachments = GL.GetInteger(GLEnum.MaxColorAttachments);
    }


    protected override void ShutdownInternal()
    {
    }

    #endregion


    #region State

    public override void UpdateViewport(int x, int y, int width, int height)
    {
        GL.Viewport(x, y, width, height);
    }


    public override void Clear(float r, float g, float b, float a, ClearFlags v)
    {
        GL.ClearColor(r, g, b, a);

        ClearBufferMask clearBufferMask = 0;
        if (v.HasFlag(ClearFlags.Color))
            clearBufferMask |= ClearBufferMask.ColorBufferBit;
        if (v.HasFlag(ClearFlags.Depth))
            clearBufferMask |= ClearBufferMask.DepthBufferBit;
        if (v.HasFlag(ClearFlags.Stencil))
            clearBufferMask |= ClearBufferMask.StencilBufferBit;
        GL.Clear(clearBufferMask);
    }


    public override void Enable(EnableCap mask)
    {
        GL.Enable(mask);
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

    public override GraphicsVertexArrayObject CreateVertexArray(VertexFormat format, GraphicsBuffer vertices, GraphicsBuffer? indices) =>
        new GLVertexArrayObject(format, vertices, indices);


    public override void BindVertexArray(GraphicsVertexArrayObject? vertexArrayObject)
    {
        GL.BindVertexArray((vertexArrayObject as GLVertexArrayObject)?.Handle ?? 0);
    }

    #endregion


    #region Shaders

    public override GraphicsProgram CompileProgram(List<ShaderSourceDescriptor> shaders) =>
        GLGraphicsProgramFactory.Create(EngineConstants.INTERNAL_SHADER_BASE_PATH, shaders);


    public override void BindProgram(GraphicsProgram program)
    {
        (program as GLGraphicsProgram)!.Use();
    }


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
        GL.Uniform2(loc, value);
    }


    public override void SetUniformV3(GraphicsProgram program, int loc, Vector3 value)
    {
        if (loc == -1)
            return;

        BindProgram(program);
        GL.Uniform3(loc, value);
    }


    public override void SetUniformV4(GraphicsProgram program, int loc, Vector4 value)
    {
        if (loc == -1)
            return;

        BindProgram(program);
        GL.Uniform4(loc, value);
    }


    public override void SetUniformMatrix(GraphicsProgram program, int loc, int length, bool transpose, in float m11)
    {
        if (loc == -1)
            return;

        BindProgram(program);
        fixed (float* ptr = &m11)
            GL.UniformMatrix4(loc, length, transpose, ptr);
    }


    public override void SetUniformTexture(GraphicsProgram program, int loc, int slot, GraphicsTexture texture)
    {
        if (loc == -1)
            return;

        BindProgram(program);
        GL.ActiveTexture((TextureUnit)((uint)TextureUnit.Texture0 + slot));
        GL.BindTexture((texture as GLTexture).Target, (texture as GLTexture).Handle);
        GL.Uniform1(loc, slot);
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