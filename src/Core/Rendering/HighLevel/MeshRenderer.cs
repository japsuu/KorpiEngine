/*namespace KorpiEngine.Core.Rendering.HighLevel;

/// <summary>
/// Represents an object that can be rendered.
/// </summary>
public class MeshRenderer
{
    public Mesh Mesh;
    public Material Material;
}

/// <summary>
/// Contains data of a mesh.
/// </summary>
public class Mesh
{
    private int _vao;
    private int _vbo;
    private int _ebo;
    
    
    public void SetVertexData<T>(T[] data) where T : struct
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(uint), data, BufferUsageHint.StaticDraw);

        if (initialize)
        {
            GL.VertexAttribIPointer(0, 2, VertexAttribIntegerType.UnsignedInt, 0, IntPtr.Zero);
            GL.EnableVertexAttribArray(0);
        }
    }
}

/// <summary>
/// Contains data of a material.
/// </summary>
public class Material
{
    public Shader Shader;
}*/