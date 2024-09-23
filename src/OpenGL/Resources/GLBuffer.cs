using KorpiEngine.Rendering;
using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.OpenGL;

internal sealed class GLBuffer : GraphicsBuffer
{
    public readonly BufferType OriginalType;
    public readonly BufferTarget Target;

    private static readonly int[] BoundBuffers = new int[(int)BufferType.Count];
    
    internal override int SizeInBytes { get; }


    public GLBuffer(BufferType type, int sizeInBytes, nint data, bool dynamic) : base(GL.GenBuffer())
    {
        if (type == BufferType.Count)
            throw new ArgumentOutOfRangeException(nameof(type), type, null);
        
        SizeInBytes = sizeInBytes;

        OriginalType = type;
        Target = type switch
        {
            BufferType.VertexBuffer => BufferTarget.ArrayBuffer,
            BufferType.ElementsBuffer => BufferTarget.ElementArrayBuffer,
            BufferType.UniformBuffer => BufferTarget.UniformBuffer,
            BufferType.StructuredBuffer => BufferTarget.ShaderStorageBuffer,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        Bind();
        if (sizeInBytes != 0)
            Set(sizeInBytes, data, dynamic);
    }


    public void Set(int sizeInBytes, nint data, bool dynamic)
    {
        Bind();
        BufferUsageHint usage = dynamic ? BufferUsageHint.DynamicDraw : BufferUsageHint.StaticDraw;
        GL.BufferData(Target, sizeInBytes, data, usage);
    }


    public void Update(int offsetInBytes, int sizeInBytes, nint data)
    {
        Bind();
        GL.BufferSubData(Target, offsetInBytes, sizeInBytes, data);
    }


    protected override void DisposeResources()
    {
        GL.DeleteBuffer(Handle);
    }


    private void Bind()
    {
        if (BoundBuffers[(int)OriginalType] == Handle)
            return;
        
        GL.BindBuffer(Target, Handle);
        BoundBuffers[(int)OriginalType] = Handle;
    }
}