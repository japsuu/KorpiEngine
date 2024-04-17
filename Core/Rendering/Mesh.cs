using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace KorpiEngine.Core.Rendering;

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

    public int ByteSize => Size * Type switch
    {
        VertexAttribPointerType.Byte => 1,
        VertexAttribPointerType.UnsignedByte => 1,
        VertexAttribPointerType.Short => 2,
        VertexAttribPointerType.UnsignedShort => 2,
        VertexAttribPointerType.Int => 4,
        VertexAttribPointerType.UnsignedInt => 4,
        VertexAttribPointerType.Float => 4,
        VertexAttribPointerType.Double => 8,
        VertexAttribPointerType.HalfFloat => 2,
        VertexAttribPointerType.Fixed => 4,
        VertexAttribPointerType.UnsignedInt2101010Rev => 4,
        VertexAttribPointerType.UnsignedInt10F11F11FRev => 4,
        VertexAttribPointerType.Int2101010Rev => 4,
        _ => throw new ArgumentOutOfRangeException()
    };


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
    Position = 0,
    Normal = 1,
    Tangent = 2,
    Color = 3,
    TexCoord0 = 4,
    TexCoord1 = 5,
}

/// <summary>
/// The format of the mesh index buffer data.
/// </summary>
public enum IndexFormat
{
    UInt16,
    UInt32
}

/// <summary>
/// Represents a 3D mesh.<br/><br/>
/// 
/// All vertex data is stored in a byte-array blob.<br/>
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
/// VertexAttribute.TexCoord1
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
public sealed class Mesh/* : IDisposable*/
{
    /// <summary>
    /// The format of the mesh index buffer data.
    /// </summary>
    public IndexFormat IndexFormat { get; private set; } = IndexFormat.UInt16;

    /// <summary>
    /// The number of active vertex attributes (see <see cref="VertexAttributeDescriptor"/>).
    /// Together with <see cref="GetVertexAttribute"/> it can be used to query information about which vertex attributes are present in the mesh.
    /// </summary>
    public int VertexAttributeCount { get; private set; }

    /// <summary>
    /// The number of vertices in the mesh.
    /// </summary>
    public int VertexCount { get; private set; }

    /// <summary>
    /// The number of indices in the mesh.
    /// </summary>
    public int IndexCount { get; private set; }

    /// <summary>
    /// True if the mesh is readable, false if it is not.<br/><br/>
    ///
    /// When the mesh is marked as readable, the vertex data is also kept in CPU-accessible memory (in addition to GPU memory).<br/>
    /// When the mesh is NOT marked as readable, the vertex data is uploaded to GPU memory and discarded from CPU memory.
    /// </summary>
    public bool IsReadable { get; private set; } = true;

    private VertexAttributeDescriptor[]? _vertexLayout;
    private bool[] _enabledVertexAttributes = new bool[8];    //TODO: Does not take empty arrays into account.
    private bool _isDirty = true;
    private int _vertexSizeBytes;
    private byte[]? _vertexDataBuffer;
    private byte[]? _indexDataBuffer;
    /*private GLBuffer<byte>? _vertexBuffer;
    private GLBuffer<byte>? _indexBuffer;


    public void Dispose()
    {
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
    }*/


    public Mesh()
    {
        SetVertexBufferLayout(StandardVertex3D.Attributes);
    }


    /// <summary>
    /// Clears all vertex data and all triangle indices from the mesh.
    /// </summary>
    /// <param name="keepVertexLayout"></param>
    public void Clear(bool keepVertexLayout = true)
    {
        _vertexDataBuffer = null;
        _indexDataBuffer = null;
        VertexCount = 0;
        IndexCount = 0;
        _isDirty = true;

        if (keepVertexLayout)
            return;

        VertexAttributeCount = 0;
        _vertexLayout = null;
        _enabledVertexAttributes = new bool[8];
    }


    /// <summary>
    /// Uploads the modified mesh data to the GPU.
    ///
    /// When creating or modifying a Mesh from code, the mesh is internally marked as "dirty" and is sent to the GPU the next time it is drawn.
    ///
    /// Call this method to immediately upload the mesh data to the GPU.
    /// </summary>
    public void UploadMeshData()
    {
        if (!_isDirty)
            return;

        _isDirty = false;

        throw new NotImplementedException("TODO: Upload byte blob data to GPU");
    }


    #region SIMPLE API

    public void SetPositions(Vector3[] positions)
    {
        SetVertexAttributes(positions, VertexAttribute.Position);
    }


    /// <summary>
    /// Fills the data array with the positions of the mesh.
    /// </summary>
    /// <param name="data">The array to fill with positions.</param>
    /// <returns>The number of positions written to the array.</returns>
    public int GetPositions(Vector3[] data) => GetVertexAttributes(data, VertexAttribute.Position);


    public void SetNormals(Vector3[]? normals)
    {
        SetVertexAttributes(normals, VertexAttribute.Normal);
    }


    /// <summary>
    /// Fills the data array with the normals of the mesh.
    /// </summary>
    /// <param name="data">The array to fill with normals.</param>
    /// <returns>The number of normals written to the array.</returns>
    public int GetNormals(Vector3[] data) => GetVertexAttributes(data, VertexAttribute.Normal);


    public void SetTangents(Vector3[]? tangents)
    {
        SetVertexAttributes(tangents, VertexAttribute.Tangent);
    }


    /// <summary>
    /// Fills the data array with the tangents of the mesh.
    /// </summary>
    /// <param name="data">The array to fill with tangents.</param>
    /// <returns>The number of tangents written to the array.</returns>
    public int GetTangents(Vector3[] data) => GetVertexAttributes(data, VertexAttribute.Tangent);


    public void SetColors(Color32[]? colors)
    {
        SetVertexAttributes(colors, VertexAttribute.Color);
    }


    /// <summary>
    /// Fills the data array with the colors of the mesh.
    /// </summary>
    /// <param name="data">The array to fill with colors.</param>
    /// <returns>The number of colors written to the array.</returns>
    public int GetColors(Color32[] data) => GetVertexAttributes(data, VertexAttribute.Color);


    /// <summary>
    /// Sets the UVs for the specified channel.<br/><br/>
    ///
    /// UVs are stored in 0-1 space.<br/>
    /// [0,0] represents the bottom-left corner of the texture, and [1,1] represents the top-right.<br/>
    /// Values are not clamped, so you can use values below 0 and above 1 if needed.
    /// </summary>
    /// <param name="channel">The UV channel in [0..7] range to set.</param>
    /// <param name="uvs">The UV data to set.</param>
    public void SetUVs(Vector2[]? uvs, int channel)
    {
        SetVertexAttributes(uvs, VertexAttribute.TexCoord0 + channel);
    }


    /// <summary>
    /// Fills the data array with the UVs of the mesh.
    /// </summary>
    /// <param name="channel">The UV channel in [0..7] range to get.</param>
    /// <param name="data">The array to fill with UVs.</param>
    /// <returns>The number of UVs written to the array.</returns>
    public int GetUVs(Vector2[] data, int channel) => GetVertexAttributes(data, VertexAttribute.TexCoord0 + channel);


    public void SetIndices(int[] indices)
    {
        // Validate the indices. Each index must be in the range [0, VertexCount - 1], and the number of indices must be a multiple of 3.
        for (int i = 0; i < indices.Length; i++)
            if (indices[i] < 0 || indices[i] >= VertexCount)
                throw new ArgumentOutOfRangeException(nameof(indices), $"The index at position {i} is out of range [0, {VertexCount - 1}].");

        if (indices.Length % 3 != 0)
            throw new ArgumentException("The number of indices must be a multiple of 3.", nameof(indices));

        switch (IndexFormat)
        {
            case IndexFormat.UInt16:
            {
                ushort[] indices16 = new ushort[indices.Length];
                for (int i = 0; i < indices.Length; i++)
                    indices16[i] = (ushort)indices[i];

                SetIndexBufferData(indices16, 0, indices16.Length);
                break;
            }
            case IndexFormat.UInt32:
            {
                uint[] indices32 = new uint[indices.Length];
                for (int i = 0; i < indices.Length; i++)
                    indices32[i] = (uint)indices[i];

                SetIndexBufferData(indices32, 0, indices32.Length);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }

        IndexCount = indices.Length;
    }


    /// <summary>
    /// Fills the data array with the indices of the mesh.
    /// </summary>
    /// <param name="data">The array to fill with indices.</param>
    /// <returns>The number of indices written to the array.</returns>
    public int GetIndices(int[] data)
    {
        switch (IndexFormat)
        {
            case IndexFormat.UInt16:
            {
                ushort[] indices16 = GetIndexBufferData<ushort>();
                for (int i = 0; i < indices16.Length; i++)
                    data[i] = indices16[i];

                return indices16.Length;
            }
            case IndexFormat.UInt32:
            {
                uint[] indices32 = GetIndexBufferData<uint>();
                for (int i = 0; i < indices32.Length; i++)
                    data[i] = (int)indices32[i];

                return indices32.Length;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
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
        SetVertexBufferLayout(attributes);
        SetVertexBufferSize(vertexCount);

        _isDirty = true;
    }


    /// <summary>
    /// Sets the vertex buffer data for this mesh.
    /// The layout of the supplied data has to match the vertex data layout of the mesh (see <see cref="SetVertexBufferParams"/>).
    /// A subset of the provided data may be used, by using the <see cref="vertexDataStart"/> and <see cref="vertexDataCount"/> parameters.
    /// </summary>
    /// <param name="vertexData">The vertex data to set.</param>
    /// <param name="vertexDataStart">The first element in the data array to copy.</param>
    /// <param name="vertexDataCount">The number of elements to copy.</param>
    /// <typeparam name="TVertex">The type of the vertex data.</typeparam>
    public void SetVertexBufferData<TVertex>(TVertex[] vertexData, int vertexDataStart, int vertexDataCount) where TVertex : struct
    {
        ArraySegment<TVertex> data = new(vertexData, vertexDataStart, vertexDataCount);

        WriteStructData(data, ref _vertexDataBuffer, _vertexSizeBytes);
    }


    /// <summary>
    /// Gets the vertex buffer data for this mesh.
    /// The memory layout of the supplied type has to match the vertex data layout of the mesh (see <see cref="SetVertexBufferParams"/>).
    /// </summary>
    /// <typeparam name="T">The type of the vertex data.</typeparam>
    /// <returns>The vertex data as an array of the specified type.</returns>
    public T[] GetVertexBufferData<T>() where T : unmanaged
    {
        if (_vertexDataBuffer == null)
            throw new InvalidOperationException(
                $"The buffer '{nameof(_vertexDataBuffer)}' has not been initialized. Did you forget to call {nameof(SetVertexBufferParams)}?");

        int structSize = Marshal.SizeOf(typeof(T));
        int numStructs = _vertexDataBuffer.Length / structSize;
        T[] structArray = new T[numStructs];

        unsafe
        {
            fixed (byte* bytePtr = _vertexDataBuffer)
            {
                byte* currentPtr = bytePtr;
                for (int i = 0; i < numStructs; i++)
                {
                    structArray[i] = *(T*)currentPtr;
                    currentPtr += structSize;
                }
            }
        }

        return structArray;
    }
    
    
    public void SetVertexBufferLayout(params VertexAttributeDescriptor[] attributes)
    {
        VertexAttributeCount = attributes.Length;
        _vertexLayout = attributes;
        foreach (VertexAttributeDescriptor d in attributes)
            _enabledVertexAttributes[(int)d.Attribute] = true;

        // Set the vertex buffer size
        int vertexSize = attributes.Sum(attribute => attribute.ByteSize);
        _vertexSizeBytes = vertexSize;
    }


    private void SetVertexBufferSize(int vertexCount)
    {
        VertexCount = vertexCount;
        _vertexDataBuffer = new byte[vertexCount * _vertexSizeBytes];
    }


    /// <summary>
    /// Sets the index buffer size and format for this mesh.
    /// </summary>
    /// <param name="indexCount">The number of indices in the buffer.</param>
    /// <param name="format">The format of the index data.</param>
    public void SetIndexBufferParams(int indexCount, IndexFormat format)
    {
        IndexFormat = format;
        IndexCount = indexCount;
        _indexDataBuffer = new byte[indexCount * (format == IndexFormat.UInt16 ? 2 : 4)];

        _isDirty = true;
    }


    /// <summary>
    /// Sets the index buffer data for this mesh.
    /// A subset of the provided data may be used, by using the <see cref="indexDataStart"/> and <see cref="indexDataCount"/> parameters.
    /// </summary>
    /// <param name="indexData">The index data to set.</param>
    /// <param name="indexDataStart">The first element in the data array to copy.</param>
    /// <param name="indexDataCount">The number of elements to copy.</param>
    /// <typeparam name="TIndex">The type of the index data.</typeparam>
    public void SetIndexBufferData<TIndex>(TIndex[] indexData, int indexDataStart, int indexDataCount) where TIndex : struct
    {
        ArraySegment<TIndex> data = new(indexData, indexDataStart, indexDataCount);

        int stride = IndexFormat == IndexFormat.UInt16 ? 2 : 4;
        WriteStructData(data, ref _indexDataBuffer, stride);
    }


    public T[] GetIndexBufferData<T>() where T : unmanaged
    {
        if (_indexDataBuffer == null)
            throw new InvalidOperationException(
                $"The buffer '{nameof(_indexDataBuffer)}' has not been initialized. Did you forget to call {nameof(SetIndexBufferParams)}?");

        int stride = IndexFormat == IndexFormat.UInt16 ? 2 : 4;
        int numIndices = _indexDataBuffer.Length / stride;
        T[] indexArray = new T[numIndices];

        unsafe
        {
            fixed (byte* bytePtr = _indexDataBuffer)
            {
                byte* currentPtr = bytePtr;
                for (int i = 0; i < numIndices; i++)
                {
                    indexArray[i] = *(T*)currentPtr;
                    currentPtr += stride;
                }
            }
        }

        return indexArray;
    }


    /// <summary>
    /// Returns information about a vertex attribute based on the attributes index.
    /// </summary>
    /// <param name="index">The index of the vertex attribute.</param>
    /// <returns>The vertex attribute descriptor.</returns>
    public VertexAttributeDescriptor GetVertexAttribute(int index)
    {
        if (_vertexLayout == null)
            throw new InvalidOperationException($"The vertex buffer has not been initialized. Did you forget to call {nameof(SetVertexBufferParams)}?");

        if (index < 0 || index >= VertexAttributeCount)
            throw new ArgumentOutOfRangeException(nameof(index), $"The index must be in the range [0, {VertexAttributeCount - 1}].");

        return _vertexLayout[index];
    }

    #endregion


    #region STATIC METHODS

    public static Mesh CreatePrimitive(PrimitiveType primitiveType)
    {
        Mesh mesh = new();

        switch (primitiveType)
        {
            case PrimitiveType.Cube:
                break;
            case PrimitiveType.Sphere:
                break;
            case PrimitiveType.Capsule:
                break;
            case PrimitiveType.Quad:
                Vector3[] positions =
                {
                    new(-0.5f, -0.5f, 0),
                    new(0.5f, -0.5f, 0),
                    new(0.5f, 0.5f, 0),
                    new(-0.5f, 0.5f, 0)
                };

                Vector2[] uvs =
                {
                    new(0, 0),
                    new(1, 0),
                    new(1, 1),
                    new(0, 1)
                };

                int[] indices =
                {
                    0, 1, 2, 0, 2, 3
                };

                mesh.SetPositions(positions);
                mesh.SetUVs(uvs, 0);
                mesh.SetIndices(indices);

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(primitiveType), primitiveType, null);
        }

        return mesh;
    }

    #endregion


    /// <summary>
    /// Completely overwrites the destination buffer with the source data.
    /// The byte side of the source data must match the stride.
    /// </summary>
    private static void WriteStructData<TStruct>(ArraySegment<TStruct> source, ref byte[]? destination, int stride) where TStruct : struct
    {
        int structSizeBytes = Marshal.SizeOf(typeof(TStruct));

        if (structSizeBytes != stride)
            throw new ArgumentException("The size of the vertex data does not match the vertex attribute layout of the mesh.", nameof(source));
        
        destination = new byte[source.Count * structSizeBytes];

        WriteStructData(source, destination, 0, stride, structSizeBytes);
    }


    private static void WriteStructData<TStruct>(ArraySegment<TStruct> source, byte[]? destination, int offset, int stride, int sourceElementLengthBytes)
        where TStruct : struct
    {
        if (destination == null)
            throw new InvalidOperationException(
                $"The buffer '{nameof(destination)}' has not been initialized. Did you forget to call {nameof(SetVertexBufferParams)}?");

        IntPtr bufferPtr = Marshal.AllocHGlobal(sourceElementLengthBytes);
        try
        {
            for (int i = 0; i < source.Count; i++)
            {
                Marshal.StructureToPtr(source[i], bufferPtr, false);

                int destinationIndex = offset + i * stride;
                Marshal.Copy(bufferPtr, destination, destinationIndex, sourceElementLengthBytes);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(bufferPtr);
        }
    }


    private static void ReadStructData<TStruct>(byte[] source, int offset, int stride, int sourceElementLengthBytes, IList<TStruct> destination)
        where TStruct : struct
    {
        IntPtr bufferPtr = Marshal.AllocHGlobal(sourceElementLengthBytes);
        try
        {
            for (int i = 0; i < destination.Count; i++)
            {
                int sourceIndex = offset + i * stride;
                Marshal.Copy(source, sourceIndex, bufferPtr, sourceElementLengthBytes);

                destination[i] = Marshal.PtrToStructure<TStruct>(bufferPtr);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(bufferPtr);
        }
    }


    private bool IsVertexAttributeEnabled(VertexAttribute attribute) => _enabledVertexAttributes[(int)attribute];


    /// <summary>
    /// Gets the byte offset of a vertex attribute in the vertex buffer.
    /// Because the vertex attributes are "interleaved" in the byte buffer,
    /// we need to calculate the offset for each vertex attribute to be able to read/write data there.
    /// </summary>
    /// <param name="attribute">The vertex attribute to get the offset for.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="ArgumentException"></exception>
    private int GetVertexAttributeOffset(VertexAttribute attribute)
    {
        if (_vertexLayout == null)
            throw new InvalidOperationException($"The vertex buffer has not been initialized. Did you forget to call {nameof(SetVertexBufferParams)}?");

        //TODO: Cache this offset for each attribute, like the enabled attributes
        int offset = 0;
        foreach (VertexAttributeDescriptor d in _vertexLayout)
        {
            if (d.Attribute == attribute)
                return offset;

            offset += d.ByteSize;
        }

        throw new ArgumentException($"The vertex attribute '{attribute}' is not present in the vertex buffer.");
    }


    private int GetVertexAttributes<T>(T[] data, VertexAttribute attribute) where T : struct
    {
        if (VertexCount == 0 || _vertexDataBuffer == null)
            return 0;

        if (data.Length < VertexCount)
            throw new ArgumentException($"The data array is too small to hold all the '{attribute}' elements.", nameof(data));

        if (!IsVertexAttributeEnabled(attribute))
            throw new InvalidOperationException($"The vertex attribute '{attribute}' is not enabled.");

        int offset = GetVertexAttributeOffset(attribute);
        ReadStructData(_vertexDataBuffer, offset, _vertexSizeBytes, Marshal.SizeOf<T>(), data);

        return VertexCount;
    }


    private void SetVertexAttributes<T>(T[]? data, VertexAttribute attribute) where T : struct
    {
        if (VertexCount != 0 && data != null && VertexCount != data.Length)
            throw new ArgumentException($"The number of elements '{attribute}' must match the number of vertices in the mesh.", nameof(data));

        if (!IsVertexAttributeEnabled(attribute))
            throw new InvalidOperationException($"The '{attribute}' vertex attribute is not enabled.");

        if (data == null)
            throw new NotImplementedException("Clearing vertex attributes is not yet implemented.");

        if (_vertexDataBuffer == null)
        {
            SetVertexBufferSize(data.Length);
        }
        
        int offset = GetVertexAttributeOffset(attribute);
        WriteStructData<T>(data, _vertexDataBuffer, offset, _vertexSizeBytes, Marshal.SizeOf<T>());
    }


    /// <summary>
    /// The mesh standard vertex layout.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
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