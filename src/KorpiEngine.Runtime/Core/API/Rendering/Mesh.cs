﻿using System.Runtime.InteropServices;
using KorpiEngine.Core.Internal.Rendering;
using KorpiEngine.Core.Internal.Utils;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Rendering.Primitives;
using OpenTK.Mathematics;

namespace KorpiEngine.Core.API.Rendering;

/// <summary>
/// Represents a 3D mesh.<br/><br/>
/// 
/// All vertex attributes are stored in separate byte-arrays.<br/>
/// 
/// For every vertex there can be a vertex position, normal, tangent, color and up to 2 texture coordinates.<br/><br/>
///
/// The mesh face data, i.e. the triangles it is made of, is simply three vertex indices for each triangle.<br/>
/// For example, if the mesh has 10 triangles, then the indices array should be 30 elements, with each element indicating which vertex to use.<br/>
/// The first three elements in the indices array are the indices for the vertices that make up that triangle; the second three elements make up another triangle and so on.<br/><br/>
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
/// mesh.SetVertexAttributeData(...);
/// mesh.SetIndexBufferData(...);
/// </code>
/// </summary>
public sealed class Mesh : EngineObject //TODO: Implement MeshData class to hide some fields
{
    /// <summary>
    /// The format of the mesh index buffer data.
    /// Changing the format usually requires resetting of the index buffer data.
    /// </summary>
    public IndexFormat IndexFormat
    {
        get => _indexFormat;
        set
        {
            _indexFormat = value;
            ClearAllIndexData();
            _isDirty = true;
        }
    }

    /// <summary>
    /// The number of vertices in the mesh.
    /// </summary>
    public int VertexCount
    {
        get => _vertexCount;
        private set
        {
            _vertexCount = value;
            _isDirty = true;
        }
    }

    /// <summary>
    /// The number of indices in the mesh.
    /// </summary>
    public int IndexCount
    {
        get => _indexCount;
        private set
        {
            _indexCount = value;
            _isDirty = true;
        }
    }

    /// <summary>
    /// True if the mesh is readable, false if it is not.<br/><br/>
    ///
    /// When the mesh is marked as readable, the vertex data is also kept in CPU-accessible memory (in addition to GPU memory).<br/>
    /// When the mesh is NOT marked as readable, the vertex data is uploaded to GPU memory and discarded from CPU memory.
    /// </summary>
    public bool IsReadable
    {
        get => _isReadable;
        set
        {
            _isReadable = value;
            _isDirty = true;
        }
    }

    /// <summary>
    /// The current topology of the mesh.
    /// </summary>
    public Topology Topology
    {
        get => _topology;
        set
        {
            _topology = value;
            _isDirty = true;
        }
    }

    public bool HasUV0 => (_vertexTexCoord0?.Length ?? 0) > 0;
    public bool HasUV1 => (_vertexTexCoord1?.Length ?? 0) > 0;
    public bool HasNormals => (_vertexNormals?.Length ?? 0) > 0;
    public bool HasColors => (_vertexColors?.Length ?? 0) > 0;
    public bool HasTangents => (_vertexTangents?.Length ?? 0) > 0;

    internal GraphicsVertexArrayObject? VertexArrayObject { get; private set; }

    private static Mesh? fullScreenQuadCached;
    private Topology _topology = Topology.TriangleStrip;
    private IndexFormat _indexFormat = IndexFormat.UInt16;
    private bool _isReadable = true;
    private bool _isDirty = true;
    private int _vertexCount;
    private int _indexCount;
    private GraphicsBuffer? _vertexBuffer;
    private GraphicsBuffer? _indexBuffer;
    private byte[]? _indexData;
    private byte[]? _vertexPositions;
    private byte[]? _vertexTexCoord0;
    private byte[]? _vertexTexCoord1;
    private byte[]? _vertexNormals;
    private byte[]? _vertexColors;
    private byte[]? _vertexTangents;


    protected override void OnDispose()
    {
        DeleteGPUBuffers();
    }


    /// <summary>
    /// Clears all vertex data and all triangle indices from the mesh.
    /// </summary>
    public void Clear()
    {
        ClearAllVertexData();
        ClearAllIndexData();
        _isDirty = true;

        DeleteGPUBuffers();
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
        if (!_isDirty && VertexArrayObject != null)
            return;

        _isDirty = false;

        DeleteGPUBuffers();

        switch (Topology)
        {
            case Topology.Triangles:
                if (IndexCount % 3 != 0)
                    throw new InvalidOperationException(
                        $"Triangle mesh doesn't have the right amount of indices. Has: {IndexCount}. Should be a multiple of 3");
                break;
            case Topology.TriangleStrip:
                if (IndexCount < 3)
                    throw new InvalidOperationException(
                        $"Triangle Strip mesh doesn't have the right amount of indices. Has: {IndexCount}. Should have at least 3");
                break;

            case Topology.Lines:
                if (IndexCount % 2 != 0)
                    throw new InvalidOperationException($"Line mesh doesn't have the right amount of indices. Has: {IndexCount}. Should be a multiple of 2");
                break;

            case Topology.LineStrip:
                if (IndexCount < 2)
                    throw new InvalidOperationException($"Line Strip mesh doesn't have the right amount of indices. Has: {IndexCount}. Should have at least 2");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        MeshVertexLayout vertexLayout = GetVertexLayout();

        byte[]? vertexDataBlob = MakeVertexDataBlob(vertexLayout);

        if (vertexDataBlob == null || _indexData == null)
            return;

        _vertexBuffer = Graphics.Driver.CreateBuffer(BufferType.VertexBuffer, vertexDataBlob);
        _indexBuffer = Graphics.Driver.CreateBuffer(BufferType.ElementsBuffer, _indexData);
        VertexArrayObject = Graphics.Driver.CreateVertexArray(vertexLayout, _vertexBuffer, _indexBuffer);

        Graphics.Driver.BindVertexArray(null);

        Application.Logger.Debug($"[VAO ID={VertexArrayObject}] Mesh uploaded successfully to VRAM (GPU)");

        // Clear the CPU-side data only if the mesh is not readable.
        if (IsReadable)
            return;

        ClearAllVertexData();
        ClearAllIndexData();
    }


    #region SIMPLE API

    public int GetPositionsNonAlloc(IList<Vector3> destination) => GetVertexAttributeDataNonAlloc(_vertexPositions, destination);

    public Vector3[]? GetPositions() => GetVertexAttributeData<Vector3>(_vertexPositions);

    public void SetPositions(ArraySegment<Vector3>? positions) => SetVertexAttributeData(positions, ref _vertexPositions);

    public int GetNormalsNonAlloc(IList<Vector3> destination) => GetVertexAttributeDataNonAlloc(_vertexNormals, destination);

    public Vector3[]? GetNormalsNonAlloc() => GetVertexAttributeData<Vector3>(_vertexNormals);

    public void SetNormals(ArraySegment<Vector3>? normals) => SetVertexAttributeData(normals, ref _vertexNormals);

    public int GetTangentsNonAlloc(IList<Vector3> destination) => GetVertexAttributeDataNonAlloc(_vertexTangents, destination);

    public Vector3[]? GetTangents() => GetVertexAttributeData<Vector3>(_vertexTangents);

    public void SetTangents(ArraySegment<Vector3>? tangents) => SetVertexAttributeData(tangents, ref _vertexTangents);

    public int GetColorsNonAlloc(IList<Color32> destination) => GetVertexAttributeDataNonAlloc(_vertexColors, destination);

    public Color32[]? GetColors() => GetVertexAttributeData<Color32>(_vertexColors);

    public void SetColors(ArraySegment<Color32>? colors) => SetVertexAttributeData(colors, ref _vertexColors);


    public int GetUVsNonAlloc(IList<Vector2> destination, int channel) => GetVertexAttributeDataNonAlloc(
        VertexAttribute.TexCoord0 + channel == VertexAttribute.TexCoord0 ? _vertexTexCoord0 : _vertexTexCoord1, destination);


    public Vector2[]? GetUVs(int channel) =>
        GetVertexAttributeData<Vector2>(VertexAttribute.TexCoord0 + channel == VertexAttribute.TexCoord0 ? _vertexTexCoord0 : _vertexTexCoord1);


    public void SetUVs(ArraySegment<Vector2>? uvs, int channel)
    {
        if (channel == 0)
            SetVertexAttributeData(uvs, ref _vertexTexCoord0);
        else
            SetVertexAttributeData(uvs, ref _vertexTexCoord1);
    }


    public int GetIndicesNonAlloc(IList<int> destination)
    {
        switch (IndexFormat)
        {
            case IndexFormat.UInt16:
            {
                ushort[]? indices16 = GetIndexBufferData<ushort>();

                if (indices16 == null)
                {
                    destination.Clear();
                    return 0;
                }

                for (int i = 0; i < indices16.Length; i++)
                    destination[i] = indices16[i];

                return indices16.Length;
            }
            case IndexFormat.UInt32:
            {
                uint[]? indices32 = GetIndexBufferData<uint>();

                if (indices32 == null)
                {
                    destination.Clear();
                    return 0;
                }

                for (int i = 0; i < indices32.Length; i++)
                    destination[i] = (int)indices32[i];

                return indices32.Length;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }


    public int[]? GetIndices()
    {
        int[] indices = new int[IndexCount];

        switch (IndexFormat)
        {
            case IndexFormat.UInt16:
            {
                ushort[]? indices16 = GetIndexBufferData<ushort>();

                if (indices16 == null)
                    return null;

                for (int i = 0; i < indices16.Length; i++)
                    indices[i] = indices16[i];

                return indices;
            }
            case IndexFormat.UInt32:
            {
                uint[]? indices32 = GetIndexBufferData<uint>();

                if (indices32 == null)
                    return null;

                for (int i = 0; i < indices32.Length; i++)
                    indices[i] = (int)indices32[i];

                return indices;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }


    public void SetIndices(int[] indices)
    {
        // Validate the indices. Each index must be in the range [0, VertexCount - 1], and the number of indices must be a multiple of 3.
        if (indices.Length % 3 != 0)
            throw new ArgumentException("The number of indices must be a multiple of 3.", nameof(indices));
        for (int i = 0; i < indices.Length; i++)
            if (indices[i] < 0 || indices[i] >= VertexCount)
                throw new ArgumentOutOfRangeException(nameof(indices), $"The index at position {i} is out of range [0, {VertexCount - 1}].");

        switch (IndexFormat)
        {
            case IndexFormat.UInt16:
            {
                ushort[] indices16 = new ushort[indices.Length];
                for (int i = 0; i < indices.Length; i++)
                {
                    int index = indices[i];
                    if (index >= ushort.MaxValue)
                        throw new InvalidOperationException($"[Mesh] Invalid value '{index}' for 16-bit indices");
                    indices16[i] = (ushort)index;
                }

                SetIndexBufferData<ushort>(new ArraySegment<ushort>(indices16, 0, indices16.Length));
                break;
            }
            case IndexFormat.UInt32:
            {
                uint[] indices32 = new uint[indices.Length];
                for (int i = 0; i < indices.Length; i++)
                    indices32[i] = (uint)indices[i];

                SetIndexBufferData<uint>(new ArraySegment<uint>(indices32, 0, indices32.Length));
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }

        IndexCount = indices.Length;
    }

    #endregion


    #region ADVANCED API

    public void SetVertexAttributeData<TAttribute>(ArraySegment<TAttribute>? attributeData, VertexAttribute attribute) where TAttribute : unmanaged
    {
        ref byte[]? bytes = ref GetVertexAttributeBytes((int)attribute);

        if (attributeData == null)
        {
            // Clear the data for this attribute.
            bytes = null;
            return;
        }

        // If this is the first time setting the vertex data, set the vertex count.
        if (VertexCount == 0)
            VertexCount = attributeData.Value.Count;
        else if (attributeData.Value.Count != VertexCount)
            throw new ArgumentException(
                $"The vertex data array must have the same length as the current vertex count. Expected {VertexCount}, but got {attributeData.Value.Count}.",
                nameof(attributeData));

        bytes ??= new byte[Marshal.SizeOf<TAttribute>() * VertexCount];

        MemoryUtils.WriteStructArray(attributeData.Value, bytes);
    }


    public void SetVertexAttributeData<TAttribute>(ArraySegment<TAttribute>? attributeData, ref byte[]? bytes) where TAttribute : unmanaged
    {
        if (attributeData == null)
        {
            // Clear the data for this attribute.
            bytes = null;
            return;
        }

        // If this is the first time setting the vertex data, set the vertex count.
        if (VertexCount == 0)
            VertexCount = attributeData.Value.Count;
        else if (attributeData.Value.Count != VertexCount)
            throw new ArgumentException(
                $"The vertex data array must have the same length as the current vertex count. Expected {VertexCount}, but got {attributeData.Value.Count}.",
                nameof(attributeData));

        bytes ??= new byte[Marshal.SizeOf<TAttribute>() * VertexCount];

        MemoryUtils.WriteStructArray(attributeData.Value, bytes);
    }


    public TAttribute[]? GetVertexAttributeData<TAttribute>(VertexAttribute attribute) where TAttribute : unmanaged
    {
        if (!IsReadable)
            throw new InvalidOperationException("The mesh is not readable. Cannot read mesh data.");

        if (VertexCount == 0)
            return null;

        byte[]? attributeData = GetVertexAttributeBytes((int)attribute);

        if (attributeData == null)
            return null;

        return MemoryUtils.ReadStructArray<TAttribute>(attributeData);
    }


    public TAttribute[]? GetVertexAttributeData<TAttribute>(byte[]? attributeData) where TAttribute : unmanaged
    {
        if (!IsReadable)
            throw new InvalidOperationException("The mesh is not readable. Cannot read mesh data.");

        if (VertexCount == 0)
            return null;

        if (attributeData == null)
            return null;

        return MemoryUtils.ReadStructArray<TAttribute>(attributeData);
    }


    public int GetVertexAttributeDataNonAlloc<TAttribute>(VertexAttribute attribute, IList<TAttribute> destination) where TAttribute : unmanaged
    {
        if (!IsReadable)
            throw new InvalidOperationException("The mesh is not readable. Cannot read mesh data.");

        if (VertexCount == 0)
        {
            destination.Clear();
            return 0;
        }

        if (destination.Count < VertexCount)
            throw new ArgumentException("The destination array is too small to hold all the requested attribute elements.", nameof(destination));

        byte[]? attributeData = GetVertexAttributeBytes((int)attribute);

        if (attributeData == null)
        {
            destination.Clear();
            return 0;
        }

        MemoryUtils.ReadStructArrayNonAlloc(attributeData, VertexCount, Marshal.SizeOf<TAttribute>(), destination);
        return VertexCount;
    }


    public int GetVertexAttributeDataNonAlloc<TAttribute>(byte[]? attributeData, IList<TAttribute> destination) where TAttribute : unmanaged
    {
        if (!IsReadable)
            throw new InvalidOperationException("The mesh is not readable. Cannot read mesh data.");

        if (VertexCount == 0)
        {
            destination.Clear();
            return 0;
        }

        if (destination.Count < VertexCount)
            throw new ArgumentException("The destination array is too small to hold all the requested attribute elements.", nameof(destination));

        if (attributeData == null)
        {
            destination.Clear();
            return 0;
        }

        MemoryUtils.ReadStructArrayNonAlloc(attributeData, VertexCount, Marshal.SizeOf<TAttribute>(), destination);
        return VertexCount;
    }


    public void SetIndexBufferData<TIndex>(ArraySegment<TIndex>? indexData) where TIndex : unmanaged
    {
        if (indexData == null)
        {
            ClearAllIndexData();
            return;
        }

        // If this is the first time setting the index data, set the index count.
        if (IndexCount == 0)
            IndexCount = indexData.Value.Count;
        else if (indexData.Value.Count != IndexCount)
            throw new ArgumentException(
                $"The index data array must have the same length as the current index count. Expected {IndexCount}, but got {indexData.Value.Count}.",
                nameof(indexData));

        _indexData ??= new byte[indexData.Value.Count * (IndexFormat == IndexFormat.UInt16 ? 2 : 4)];

        int stride = IndexFormat == IndexFormat.UInt16 ? 2 : 4;
        MemoryUtils.WriteStructArray(indexData.Value, _indexData, stride);
    }


    public TIndex[]? GetIndexBufferData<TIndex>() where TIndex : unmanaged
    {
        if (!IsReadable)
            throw new InvalidOperationException("The mesh is not readable. Cannot read mesh data.");

        if (IndexCount == 0 || _indexData == null)
            return null;

        int stride = IndexFormat == IndexFormat.UInt16 ? 2 : 4;
        int numIndices = _indexData.Length / stride;
        TIndex[] indexArray = new TIndex[numIndices];

        unsafe
        {
            fixed (byte* bytePtr = _indexData)
            {
                byte* currentPtr = bytePtr;
                for (int i = 0; i < numIndices; i++)
                {
                    indexArray[i] = *(TIndex*)currentPtr;
                    currentPtr += stride;
                }
            }
        }

        return indexArray;
    }

    #endregion


    #region STATIC METHODS

    public static Mesh CreatePrimitive(PrimitiveType primitiveType)
    {
        Mesh mesh = new();

        switch (primitiveType)
        {
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
                    0,
                    1,
                    2,
                    0,
                    2,
                    3
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


    public static Mesh GetFullscreenQuad()
    {
        if (fullScreenQuadCached != null)
            return fullScreenQuadCached;
        Mesh mesh = new();
        Vector3[] positions = new Vector3[4];
        positions[0] = new Vector3(-1, -1, 0);
        positions[1] = new Vector3(1, -1, 0);
        positions[2] = new Vector3(-1, 1, 0);
        positions[3] = new Vector3(1, 1, 0);

        int[] indices = new int[6];
        indices[0] = 0;
        indices[1] = 2;
        indices[2] = 1;
        indices[3] = 2;
        indices[4] = 3;
        indices[5] = 1;

        Vector2[] uvs = new Vector2[4];
        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(1, 0);
        uvs[2] = new Vector2(0, 1);
        uvs[3] = new Vector2(1, 1);

        mesh.SetPositions(positions);
        mesh.SetIndices(indices);
        mesh.SetUVs(uvs, 0);

        fullScreenQuadCached = mesh;
        return mesh;
    }

    #endregion


    /// <returns>A byte array containing the vertex data for the mesh.</returns>
    private byte[]? MakeVertexDataBlob(MeshVertexLayout vertexLayout)
    {
        if (VertexCount == 0)
            return null;

        byte[] buffer = new byte[VertexCount * vertexLayout.VertexSize];

        foreach (MeshVertexLayout.VertexAttributeDescriptor attribute in vertexLayout.Attributes)
        {
            byte[]? data = GetVertexAttributeBytes(attribute.Semantic);
            if (data == null)
                continue;

            if (data.Length % attribute.Stride != 0)
                throw new InvalidOperationException(
                    $"The vertex attribute '{(VertexAttribute)attribute.Semantic}' has an invalid stride. Expected a multiple of {attribute.Stride}, but got {data.Length}.");

            for (int i = 0; i < data.Length; i++)
            {
                int bufferIndex = i * vertexLayout.VertexSize + attribute.Offset;
                int attributeIndex = i * attribute.Stride;

                for (int j = 0; j < attribute.Stride; j++)
                    buffer[bufferIndex + j] = data[attributeIndex + j];
            }
        }

        return buffer;
    }


    /// <returns>The byte array containing the vertex attribute data for the specified attribute.</returns>
    private ref byte[]? GetVertexAttributeBytes(int attributeSemantic)
    {
        VertexAttribute attribute = (VertexAttribute)attributeSemantic;

        if (attribute == VertexAttribute.Position)
            return ref _vertexPositions;
        if (attribute == VertexAttribute.TexCoord0)
            return ref _vertexTexCoord0;
        if (attribute == VertexAttribute.TexCoord1)
            return ref _vertexTexCoord1;
        if (attribute == VertexAttribute.Normal)
            return ref _vertexNormals;
        if (attribute == VertexAttribute.Color)
            return ref _vertexColors;
        if (attribute == VertexAttribute.Tangent)
            return ref _vertexTangents;
        throw new ArgumentOutOfRangeException(nameof(attribute), attribute, null);
    }


    /// <returns>The default layout based on the enabled vertex attributes.</returns>
    private MeshVertexLayout GetVertexLayout()
    {
        List<MeshVertexLayout.VertexAttributeDescriptor> attributes = new()
        {
            new MeshVertexLayout.VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeType.Float, 3)
        };

        if (HasUV0)
            attributes.Add(new MeshVertexLayout.VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeType.Float, 2));

        if (HasUV1)
            attributes.Add(new MeshVertexLayout.VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeType.Float, 2));

        if (HasNormals)
            attributes.Add(new MeshVertexLayout.VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeType.Float, 3, true));

        if (HasColors)
            attributes.Add(new MeshVertexLayout.VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeType.Float, 4));

        if (HasTangents)
            attributes.Add(new MeshVertexLayout.VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeType.Float, 3, true));

        return new MeshVertexLayout(attributes.ToArray());
    }


    private void ClearAllVertexData()
    {
        _vertexPositions = null;
        _vertexTexCoord0 = null;
        _vertexTexCoord1 = null;
        _vertexNormals = null;
        _vertexColors = null;
        _vertexTangents = null;
        VertexCount = 0;
        _isDirty = true;
    }


    private void ClearAllIndexData()
    {
        _indexData = null;
        IndexCount = 0;
        _isDirty = true;
    }


    private void DeleteGPUBuffers()
    {
        VertexArrayObject?.Dispose();
        VertexArrayObject = null;

        _vertexBuffer?.Dispose();
        _vertexBuffer = null;

        _indexBuffer?.Dispose();
        _indexBuffer = null;
    }
}