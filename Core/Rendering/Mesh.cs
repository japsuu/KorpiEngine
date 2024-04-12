using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace KorpiEngine.Core.Rendering;

/// <summary>
/// Represents a 3D mesh.<br/><br/>
/// 
/// All vertex data is stored in separate arrays of the same size.<br/>
/// For example, if you have a mesh of 64 vertices, and want to have a position, normal and two texture coordinates for each vertex,
/// then the mesh should have vertices, normals, uv and uv2 arrays, each being 64 in size.<br/>
/// Data for i-th vertex is at index "i" in each array.<br/><br/>
/// 
/// For every vertex there can be a vertex position, normal, tangent, color and up to 8 texture coordinates.<br/><br/>
///
/// The mesh face data, i.e. the triangles it is made of, is simply three vertex indices for each triangle.<br/>
/// For example, if the mesh has 10 triangles, then the indices array should be 30 elements, with each element indicating which vertex to use.<br/>
/// The first three elements in the indices array are the indices for the vertices that make up that triangle; the second three elements make up another triangle and so on.<br/><br/>
/// 
/// Attributes of a vertex are laid out one after another, in the following order:
/// <code>
/// VertexAttribute.Position,
/// VertexAttribute.Normal,
/// VertexAttribute.Tangent,
/// VertexAttribute.Color,
/// VertexAttribute.TexCoord0,
/// ...,
/// VertexAttribute.TexCoord7
/// </code><br/><br/>
///
/// The basic usage of the Mesh class is to set the vertex buffer data using the simple API:<br/>
/// <code>
/// Mesh mesh = new Mesh();
/// mesh.SetPositions(...);
/// mesh.SetUVs(...);
/// mesh.SetIndices(...);
/// </code><br/><br/>
///
/// The advanced API allows for more control over the mesh data layout and allows for partial updates of the mesh data:<br/>
/// <code>
/// Mesh mesh = new Mesh();
/// mesh.SetVertexBufferParams(...);
/// mesh.SetVertexBufferData(...);
/// mesh.SetIndexBufferParams(...);
/// mesh.SetIndexBufferData(...);
/// </code>
/// </summary>
public class Mesh
{
    /// <summary>
    /// The format of the mesh index buffer data.
    /// </summary>
    public IndexFormat IndexFormat { get; private set; }
    
    /// <summary>
    /// The number of active vertex attributes (see <see cref="VertexAttributeDescriptor"/>).
    /// Together with <see cref="GetVertexAttribute"/> it can be used to query information about which vertex attributes are present in the mesh.
    /// </summary>
    public int VertexAttributeCount { get; private set; }
    
    /// <summary>
    /// The number of vertices in the mesh.
    /// </summary>
    public int VertexCount { get; private set; }


    public Mesh()
    {
        SetVertexBufferParams(0, StandardVertex3D.Attributes);
    }
    
    
    /// <summary>
    /// Clears all vertex data and all triangle indices from the mesh.
    /// </summary>
    /// <param name="keepVertexLayout"></param>
    public void Clear(bool keepVertexLayout = true)
    {
        
    }


    #region SIMPLE API

    public void SetPositions(Vector3[] positions)
    {
        
    }

    
    public void SetPositions(Vector3[] positions, int start, int length)
    {
        
    }
    
    
    /// <summary>
    /// Fills the data array with the positions of the mesh.
    /// </summary>
    /// <param name="data">The array to fill with positions.</param>
    /// <returns>The number of positions written to the array.</returns>
    public int GetPositions(Vector3[] data)
    {
        
    }
    
    
    public void SetNormals(Vector3[] normals)
    {
        
    }
    
    
    public void SetNormals(Vector3[] normals, int start, int length)
    {
        
    }
    
    
    /// <summary>
    /// Fills the data array with the normals of the mesh.
    /// </summary>
    /// <param name="data">The array to fill with normals.</param>
    /// <returns>The number of normals written to the array.</returns>
    public int GetNormals(Vector3[] data)
    {
        
    }
    
    
    public void SetTangents(Vector3[] tangents)
    {
        
    }
    
    
    public void SetTangents(Vector3[] tangents, int start, int length)
    {
        
    }
    
    
    /// <summary>
    /// Fills the data array with the tangents of the mesh.
    /// </summary>
    /// <param name="data">The array to fill with tangents.</param>
    /// <returns>The number of tangents written to the array.</returns>
    public int GetTangents(Vector3[] data)
    {
        
    }
    
    
    public void SetColors(Color32[] colors)
    {
        
    }
    
    
    public void SetColors(Color32[] colors, int start, int length)
    {
        
    }
    
    
    /// <summary>
    /// Fills the data array with the colors of the mesh.
    /// </summary>
    /// <param name="data">The array to fill with colors.</param>
    /// <returns>The number of colors written to the array.</returns>
    public int GetColors(Color32[] data)
    {
        
    }
    
    
    /// <summary>
    /// Sets the UVs for the specified channel.<br/><br/>
    ///
    /// UVs are stored in 0-1 space.<br/>
    /// [0,0] represents the bottom-left corner of the texture, and [1,1] represents the top-right.<br/>
    /// Values are not clamped, so you can use values below 0 and above 1 if needed.
    /// </summary>
    /// <param name="channel">The UV channel in [0..7] range to set.</param>
    /// <param name="uvs">The UV data to set.</param>
    public void SetUVs(int channel, Vector2[] uvs)
    {
        
    }


    /// <summary>
    /// Fills the data array with the UVs of the mesh.
    /// </summary>
    /// <param name="channel">The UV channel in [0..7] range to get.</param>
    /// <param name="data">The array to fill with UVs.</param>
    /// <returns>The number of UVs written to the array.</returns>
    public int GetUVs(int channel, Vector2[] data)
    {
        
    }
    
    
    public void SetIndices(int[] indices)
    {
        
    }
    
    
    public void SetIndices(int[] indices, int start, int length)
    {
        
    }
    
    
    /// <summary>
    /// Fills the data array with the indices of the mesh.
    /// </summary>
    /// <param name="data">The array to fill with indices.</param>
    /// <returns>The number of indices written to the array.</returns>
    public int GetIndices(int[] data)
    {
        
    }

    #endregion


    #region ADVANCED API


    /// <summary>
    /// Sets the vertex buffer size and layout for this mesh.
    /// </summary>
    /// <param name="vertexCount">The number of vertices in the buffer.</param>
    /// <param name="attributes">The layout of the vertex attributes.</param>
    public void SetVertexBufferParams(int vertexCount, params VertexAttributeDescriptor[] attributes)
    {
        
    }

    /// <summary>
    /// Sets the vertex buffer data for this mesh.
    /// The layout of the supplied data has to match the vertex data layout of the mesh (see <see cref="SetVertexBufferParams"/>).
    /// The data can be partially updated, using the <see cref="dataStart"/>, <see cref="meshBufferStart"/> and <see cref="count"/> parameters
    /// </summary>
    /// <param name="data">The vertex data to set.</param>
    /// <param name="dataStart">The first element in the data array to copy.</param>
    /// <param name="meshBufferStart">The first element in the mesh buffer to set.</param>
    /// <param name="count">The number of elements to copy.</param>
    /// <typeparam name="TVertex">The type of the vertex data.</typeparam>
    public void SetVertexBufferData<TVertex>(TVertex[] data, int dataStart, int meshBufferStart, int count)
    {
        
    }
    
    
    /// <summary>
    /// Sets the index buffer size and format for this mesh.
    /// </summary>
    /// <param name="indexCount">The number of indices in the buffer.</param>
    /// <param name="format">The format of the index data.</param>
    public void SetIndexBufferParams(int indexCount, IndexFormat format)
    {
        
    }

    /// <summary>
    /// Sets the index buffer data for this mesh.
    /// The data can be partially updated, using the <see cref="dataStart"/>, <see cref="meshBufferStart"/> and <see cref="count"/> parameters
    /// </summary>
    /// <param name="data">The index data to set.</param>
    /// <param name="dataStart">The first element in the data array to copy.</param>
    /// <param name="meshBufferStart">The first element in the mesh buffer to set.</param>
    /// <param name="count">The number of elements to copy.</param>
    /// <typeparam name="TIndex">The type of the index data.</typeparam>
    public void SetIndexBufferData<TIndex>(TIndex[] data, int dataStart, int meshBufferStart, int count)
    {
        
    }
    
    
    /// <summary>
    /// Returns information about a vertex attribute based on the attributes index.
    /// </summary>
    /// <param name="index">The index of the vertex attribute.</param>
    /// <returns>The vertex attribute descriptor.</returns>
    public VertexAttributeDescriptor GetVertexAttribute(int index)
    {
        
    }

    #endregion


    #region STATIC METHODS

    public static Mesh CreatePrimitive(PrimitiveType primitiveType)
    {
        throw new NotImplementedException();
    }

    #endregion


    /// <summary>
    /// The mesh standard vertex layout.
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct StandardVertex3D
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector3 Tangent;
        public Color32 Color;
        public Vector2 TexCoord0;

        public static readonly VertexAttributeDescriptor[] Attributes =
        {
            new(VertexAttribute.Position, VertexAttribPointerType.Float, 3),
            new(VertexAttribute.Normal, VertexAttribPointerType.Float, 3),
            new(VertexAttribute.Tangent, VertexAttribPointerType.Float, 3),
            new(VertexAttribute.Color, VertexAttribPointerType.UnsignedByte, 4),
            new(VertexAttribute.TexCoord0, VertexAttribPointerType.Float, 2)
        };
    }
}

/// <summary>
/// Describes how a single <see cref="VertexAttribute"/> is laid out in a mesh vertex buffer.
/// </summary>
public readonly struct VertexAttributeDescriptor
{
    /// <summary>
    /// The attribute this descriptor describes.
    /// </summary>
    public VertexAttribute Attribute { get; }
    
    /// <summary>
    /// The data type of the attribute.
    /// </summary>
    public VertexAttribPointerType Type { get; }
    
    /// <summary>
    /// How many elements of the specified type are used for this attribute.
    /// </summary>
    public int Size { get; }


    public VertexAttributeDescriptor(VertexAttribute attribute, VertexAttribPointerType type, int size)
    {
        Attribute = attribute;
        Type = type;
        Size = size;
    }
}

/// <summary>
/// The type of vertex attribute.
/// </summary>
public enum VertexAttribute
{
    Position,
    Normal,
    Tangent,
    Color,
    TexCoord0,
    TexCoord1,
    TexCoord2,
    TexCoord3,
    TexCoord4,
    TexCoord5,
    TexCoord6,
    TexCoord7
}

/// <summary>
/// The format of the mesh index buffer data.
/// </summary>
public enum IndexFormat
{
    UInt16,
    UInt32
}