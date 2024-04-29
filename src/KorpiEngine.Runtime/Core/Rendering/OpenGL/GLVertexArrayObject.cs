using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.Core.Rendering.OpenGL;

internal sealed class GLVertexArrayObject : GraphicsVertexArrayObject
{
    public GLVertexArrayObject(VertexFormat format, GraphicsBuffer vertices, GraphicsBuffer? indices) : base(GL.GenVertexArray())
    {
        GL.BindVertexArray(Handle);

        BindFormat(format);

        GL.BindBuffer(BufferTarget.ArrayBuffer, (vertices as GLBuffer)!.Handle);
        if (indices != null)
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, (indices as GLBuffer)!.Handle);
    }


    private static void BindFormat(VertexFormat format)
    {
        foreach (VertexFormat.Element element in format.Elements)
        {
            uint index = element.Semantic;
            GL.EnableVertexAttribArray(index);
            IntPtr offset = (IntPtr)element.Offset;

            if (element.Type == VertexFormat.VertexType.Float)
                GL.VertexAttribPointer(index, element.Count, (VertexAttribPointerType)element.Type, element.Normalized, format.VertexSize, offset);
            else
                GL.VertexAttribIPointer(index, element.Count, (VertexAttribIntegerType)element.Type, format.VertexSize, offset);
        }
    }

    
    protected override void Dispose(bool manual)
    {
        if (!manual)
            return;
        
        GL.DeleteVertexArray(Handle);
    }
}