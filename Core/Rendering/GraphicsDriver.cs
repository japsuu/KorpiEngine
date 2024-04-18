using KorpiEngine.Core.Logging;
using KorpiEngine.Core.Rendering.Primitives;
using KorpiEngine.Core.Rendering.Shaders;
using KorpiEngine.Core.Rendering.Textures;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace KorpiEngine.Core.Rendering;

public abstract class GraphicsDriver
{
    protected static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(GraphicsDriver));

    public abstract GraphicsProgram? CurrentProgram { get; }


    #region Initialization and Shutdown

    public void Initialize()
    {
        Logger.Info("Initializing...");
        InitializeInternal();
    }


    protected abstract void InitializeInternal();


    public void Shutdown()
    {
        Logger.Info("Shutting down...");
        ShutdownInternal();
    }


    protected abstract void ShutdownInternal();

    #endregion


    #region State

    public abstract void UpdateViewport(int x, int y, int width, int height);

    public abstract void Clear(float r, float g, float b, float a, ClearFlags v);

    public abstract void Enable(EnableCap mask);

    #endregion
    
    
    #region Buffers

    /// <summary> Create a graphics buffer with the given type and data. </summary>
    public abstract GraphicsBuffer CreateBuffer<T>(BufferType bufferType, T[] data, bool dynamic = false) where T : unmanaged;

    /// <summary> Set the data of the given buffer with the given data. </summary>
    public abstract void SetBuffer<T>(GraphicsBuffer buffer, T[] data, bool dynamic = false) where T : unmanaged;

    /// <summary> Update the given buffer with the given data at the given offset in bytes. </summary>
    public abstract void UpdateBuffer<T>(GraphicsBuffer buffer, int offsetInBytes, T[] data) where T : unmanaged;

    public abstract void BindBuffer(GraphicsBuffer buffer);

    #endregion

    
    #region Vertex Arrays

    public abstract GraphicsVertexArrayObject CreateVertexArray(VertexFormat format, GraphicsBuffer vertices, GraphicsBuffer? indices);
    public abstract void BindVertexArray(GraphicsVertexArrayObject? vertexArrayObject);

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
    
    #endregion

    
    #region Textures

    public abstract GraphicsTexture CreateTexture(TextureType type, TextureImageFormat format);
    public abstract void SetWrapS(GraphicsTexture texture, TextureWrap wrap);
    public abstract void SetWrapT(GraphicsTexture texture, TextureWrap wrap);
    public abstract void SetWrapR(GraphicsTexture texture, TextureWrap wrap);
    public abstract void SetTextureFilters(GraphicsTexture texture, TextureMin min, TextureMag mag);
    public abstract void GenerateMipmap(GraphicsTexture texture);

    public abstract unsafe void GetTexImage(GraphicsTexture texture, int mip, void* data);

    public abstract unsafe void TexImage2D(GraphicsTexture texture, int v1, uint size1, uint size2, int v2, void* v3);
    public abstract unsafe void TexImage2D(GraphicsTexture texture, TextureCubemap.CubemapFace face, int v1, uint size1, uint size2, int v2, void* v3);
    public abstract unsafe void TexSubImage2D(GraphicsTexture texture, int v, int rectX, int rectY, uint rectWidth, uint rectHeight, void* ptr);
    public abstract unsafe void TexSubImage2D(GraphicsTexture texture, TextureCubemap.CubemapFace face, int v, int rectX, int rectY, uint rectWidth, uint rectHeight, void* ptr);
    public abstract unsafe void TexSubImage3D(GraphicsTexture texture, int v, int rectX, int rectY, int rectZ, uint rectWidth, uint rectHeight, uint rectDepth, void* ptr);
    public abstract unsafe void TexImage3D(GraphicsTexture texture, int v1nt, uint width, uint height, uint depth, int v2, void* v3);

    #endregion


    #region Drawing

    public abstract void DrawArrays(Topology primitiveType, int v, uint count);
    public abstract unsafe void DrawElements(Topology triangles, uint indexCount, bool isIndex32Bit, void* value);

    #endregion
}