using KorpiEngine.Core.Rendering.Primitives;
using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.Core.Rendering.OpenGL;

public sealed class GLBuffer : GraphicsBuffer
{
    public readonly BufferType OriginalType;
    public readonly BufferTarget Target;
    public readonly int SizeInBytes;

    private static readonly int[] BoundBuffers = new int[(int)BufferType.Count];


    public unsafe GLBuffer(BufferType type, int sizeInBytes, void* data, bool dynamic) : base(GL.GenBuffer())
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


    public unsafe void Set(int sizeInBytes, void* data, bool dynamic)
    {
        Bind();
        BufferUsageHint usage = dynamic ? BufferUsageHint.DynamicDraw : BufferUsageHint.StaticDraw;
        GL.BufferData(Target, sizeInBytes, (IntPtr)data, usage);
    }


    public unsafe void Update(int offsetInBytes, int sizeInBytes, void* data)
    {
        Bind();
        GL.BufferSubData(Target, (IntPtr)offsetInBytes, (IntPtr)sizeInBytes, (IntPtr)data);
    }


    protected override void Dispose(bool manual)
    {
        if (!manual)
            return;
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