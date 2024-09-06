using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.Rendering.OpenGL;

internal sealed class GLVertexArrayObject : GraphicsVertexArrayObject
{
    public GLVertexArrayObject(MeshVertexLayout layout, GraphicsBuffer vertices, GraphicsBuffer? indices) : base(GL.GenVertexArray())
    {
        GL.BindVertexArray(Handle);

        BindFormat(layout);

        GL.BindBuffer(BufferTarget.ArrayBuffer, (vertices as GLBuffer)!.Handle);
        if (indices != null)
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, (indices as GLBuffer)!.Handle);
    }


    private static void BindFormat(MeshVertexLayout layout)
    {
        foreach (MeshVertexLayout.VertexAttributeDescriptor element in layout.Attributes)
        {
            int index = element.Semantic;
            GL.EnableVertexAttribArray(index);
            IntPtr offset = element.Offset;

            if (element.AttributeType == VertexAttributeType.Float)
                GL.VertexAttribPointer(index, element.Count, (VertexAttribPointerType)element.AttributeType, element.Normalized, layout.VertexSize, offset);
            else
                GL.VertexAttribIPointer(index, element.Count, (VertexAttribIntegerType)element.AttributeType, layout.VertexSize, offset);
        }
    }

    
    protected override void Dispose(bool manual)
    {
        if (IsDisposed)
            return;
        base.Dispose(manual);

        GL.DeleteVertexArray(Handle);
    }
}