using System.Runtime.InteropServices;
using KorpiEngine.Core.Internal.Rendering;
using KorpiEngine.Core.Internal.Utils;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Rendering.Primitives;
using Prowl.Runtime;

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
public sealed class Mesh : EngineObject, ISerializable //TODO: Implement MeshData class to hide some fields
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

    /// <summary>
    /// The bounds of the mesh.
    /// </summary>
    public Bounds Bounds { get; private set; }

    public bool HasUV0 => (_vertexTexCoord0?.Length ?? 0) > 0;
    public bool HasUV1 => (_vertexTexCoord1?.Length ?? 0) > 0;
    public bool HasNormals => (_vertexNormals?.Length ?? 0) > 0;
    public bool HasColors => (_vertexColors?.Length ?? 0) > 0;
    public bool HasColors32 => (_vertexColors32?.Length ?? 0) > 0;
    public bool HasTangents => (_vertexTangents?.Length ?? 0) > 0;
#warning Implement bone rendering
    public bool HasBoneIndices => (_boneIndices?.Length ?? 0) > 0;
    public bool HasBoneWeights => (_boneWeights?.Length ?? 0) > 0;

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
    private byte[]? _vertexColors32;
    private byte[]? _vertexTangents;
    private byte[]? _boneIndices;
    private byte[]? _boneWeights;

    public System.Numerics.Matrix4x4[]? BindPoses; //TODO: Hide this field


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


    public void RecalculateBounds()
    {
        if (_vertexPositions == null)
            throw new ArgumentNullException();

        bool empty = true;
        System.Numerics.Vector3 minVec = System.Numerics.Vector3.One * 99999f;
        System.Numerics.Vector3 maxVec = System.Numerics.Vector3.One * -99999f;
        foreach (System.Numerics.Vector3 ptVector in GetPositions()!)
        {
            minVec.X = minVec.X < ptVector.X ? minVec.X : ptVector.X;
            minVec.Y = minVec.Y < ptVector.Y ? minVec.Y : ptVector.Y;
            minVec.Z = minVec.Z < ptVector.Z ? minVec.Z : ptVector.Z;

            maxVec.X = maxVec.X > ptVector.X ? maxVec.X : ptVector.X;
            maxVec.Y = maxVec.Y > ptVector.Y ? maxVec.Y : ptVector.Y;
            maxVec.Z = maxVec.Z > ptVector.Z ? maxVec.Z : ptVector.Z;

            empty = false;
        }

        if (empty)
            throw new ArgumentException();

        Bounds = new Bounds(minVec, maxVec);
    }


    public void RecalculateNormals()
    {
        if (_vertexPositions == null || _indexData == null)
            return;

        System.Numerics.Vector3[] vertices = GetPositions()!;
        if (vertices.Length < 3)
            return;

        int[] indices = GetIndices()!;
        if (indices.Length < 3)
            return;

        System.Numerics.Vector3[] normals = new System.Numerics.Vector3[vertices.Length];

        for (int i = 0; i < indices.Length; i += 3)
        {
            int ai = indices[i];
            int bi = indices[i + 1];
            int ci = indices[i + 2];

            System.Numerics.Vector3 n = System.Numerics.Vector3.Normalize(
                System.Numerics.Vector3.Cross(
                    vertices[bi] - vertices[ai],
                    vertices[ci] - vertices[ai]
                ));

            normals[ai] += n;
            normals[bi] += n;
            normals[ci] += n;
        }

        for (int i = 0; i < vertices.Length; i++)
            normals[i] = -System.Numerics.Vector3.Normalize(normals[i]);

        SetNormals(normals);
    }


    public void RecalculateTangents()
    {
        if (_vertexPositions == null || _indexData == null)
            return;

        System.Numerics.Vector3[] vertices = GetPositions()!;
        if (vertices.Length < 3)
            return;

        int[] indices = GetIndices()!;
        if (indices.Length < 3)
            return;

        if (_vertexTexCoord0 == null)
            return;
        System.Numerics.Vector2[] uv = GetUVs(0)!;

        System.Numerics.Vector3[] tangents = new System.Numerics.Vector3[vertices.Length];

        for (int i = 0; i < indices.Length; i += 3)
        {
            int ai = indices[i];
            int bi = indices[i + 1];
            int ci = indices[i + 2];

            System.Numerics.Vector3 edge1 = vertices[bi] - vertices[ai];
            System.Numerics.Vector3 edge2 = vertices[ci] - vertices[ai];

            System.Numerics.Vector2 deltaUV1 = uv[bi] - uv[ai];
            System.Numerics.Vector2 deltaUV2 = uv[ci] - uv[ai];

            float f = 1.0f / (deltaUV1.X * deltaUV2.Y - deltaUV2.X * deltaUV1.Y);

            System.Numerics.Vector3 tangent;
            tangent.X = f * (deltaUV2.Y * edge1.X - deltaUV1.Y * edge2.X);
            tangent.Y = f * (deltaUV2.Y * edge1.Y - deltaUV1.Y * edge2.Y);
            tangent.Z = f * (deltaUV2.Y * edge1.Z - deltaUV1.Y * edge2.Z);

            tangents[ai] += tangent;
            tangents[bi] += tangent;
            tangents[ci] += tangent;
        }

        for (int i = 0; i < vertices.Length; i++)
            tangents[i] = System.Numerics.Vector3.Normalize(tangents[i]);

        SetTangents(tangents);
    }


    #region SIMPLE API

    public int GetPositionsNonAlloc(IList<System.Numerics.Vector3> destination) => GetVertexAttributeDataNonAlloc(_vertexPositions, destination);

    public System.Numerics.Vector3[]? GetPositions() => GetVertexAttributeData<System.Numerics.Vector3>(_vertexPositions);

    public void SetPositions(ArraySegment<System.Numerics.Vector3>? positions) => SetVertexAttributeData(positions, ref _vertexPositions);

    public int GetNormalsNonAlloc(IList<System.Numerics.Vector3> destination) => GetVertexAttributeDataNonAlloc(_vertexNormals, destination);

    public System.Numerics.Vector3[]? GetNormalsNonAlloc() => GetVertexAttributeData<System.Numerics.Vector3>(_vertexNormals);

    public void SetNormals(ArraySegment<System.Numerics.Vector3>? normals) => SetVertexAttributeData(normals, ref _vertexNormals);

    public int GetTangentsNonAlloc(IList<System.Numerics.Vector3> destination) => GetVertexAttributeDataNonAlloc(_vertexTangents, destination);

    public System.Numerics.Vector3[]? GetTangents() => GetVertexAttributeData<System.Numerics.Vector3>(_vertexTangents);

    public void SetTangents(ArraySegment<System.Numerics.Vector3>? tangents) => SetVertexAttributeData(tangents, ref _vertexTangents);

    public int GetColors32NonAlloc(IList<Color32> destination) => GetVertexAttributeDataNonAlloc(_vertexColors32, destination);

    public Color32[]? GetColors32() => GetVertexAttributeData<Color32>(_vertexColors32);

    public void SetColors32(ArraySegment<Color32>? colors) => SetVertexAttributeData(colors, ref _vertexColors32);

    public int GetColorsNonAlloc(IList<Color> destination) => GetVertexAttributeDataNonAlloc(_vertexColors, destination);

    public Color[]? GetColors() => GetVertexAttributeData<Color>(_vertexColors);

    public void SetColors(ArraySegment<Color>? colors) => SetVertexAttributeData(colors, ref _vertexColors);


    #region BONES

    public int GetBoneIndicesNonAlloc(IList<System.Numerics.Vector4> destination) => GetVertexAttributeDataNonAlloc(_boneIndices, destination);

    public System.Numerics.Vector4[]? GetBoneIndices() => GetVertexAttributeData<System.Numerics.Vector4>(_boneIndices);

    public void SetBoneIndices(ArraySegment<System.Numerics.Vector4>? indices) => SetVertexAttributeData(indices, ref _boneIndices);

    
    public int GetBoneWeightsNonAlloc(IList<System.Numerics.Vector4> destination) => GetVertexAttributeDataNonAlloc(_boneWeights, destination);

    public System.Numerics.Vector4[]? GetBoneWeights() => GetVertexAttributeData<System.Numerics.Vector4>(_boneWeights);

    public void SetBoneWeights(ArraySegment<System.Numerics.Vector4>? weights) => SetVertexAttributeData(weights, ref _boneWeights);

    #endregion


    public int GetUVsNonAlloc(IList<System.Numerics.Vector2> destination, int channel) => GetVertexAttributeDataNonAlloc(
        VertexAttribute.TexCoord0 + channel == VertexAttribute.TexCoord0 ? _vertexTexCoord0 : _vertexTexCoord1, destination);


    public System.Numerics.Vector2[]? GetUVs(int channel) =>
        GetVertexAttributeData<System.Numerics.Vector2>(VertexAttribute.TexCoord0 + channel == VertexAttribute.TexCoord0 ? _vertexTexCoord0 : _vertexTexCoord1);


    public void SetUVs(ArraySegment<System.Numerics.Vector2>? uvs, int channel)
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
        switch (primitiveType)
        {
            case PrimitiveType.Quad:
            {
                System.Numerics.Vector3[] positions =
                [
                    new System.Numerics.Vector3(-0.5f, -0.5f, 0),
                    new System.Numerics.Vector3(0.5f, -0.5f, 0),
                    new System.Numerics.Vector3(0.5f, 0.5f, 0),
                    new System.Numerics.Vector3(-0.5f, 0.5f, 0)
                ];

                System.Numerics.Vector2[] uvs =
                [
                    new System.Numerics.Vector2(0, 0),
                    new System.Numerics.Vector2(1, 0),
                    new System.Numerics.Vector2(1, 1),
                    new System.Numerics.Vector2(0, 1)
                ];

                int[] indices =
                [
                    0,
                    1,
                    2,
                    0,
                    2,
                    3
                ];

                Mesh mesh = new();
                mesh.SetPositions(positions);
                mesh.SetUVs(uvs, 0);
                mesh.SetIndices(indices);
                return mesh;
            }
            case PrimitiveType.Cube:
                return null!;
            case PrimitiveType.Sphere:
            {
                const float radius = 0.5f;
                const int rings = 8;
                const int slices = 16;
                List<System.Numerics.Vector3> vertices = [];
                List<System.Numerics.Vector2> uvs = [];
                List<int> indices = [];

                for (int i = 0; i <= rings; i++)
                {
                    float v = 1 - (float)i / rings;
                    float phi = v * MathF.PI;

                    for (int j = 0; j <= slices; j++)
                    {
                        float u = (float)j / slices;
                        float theta = u * MathF.PI * 2;

                        float x = MathF.Sin(phi) * MathF.Cos(theta);
                        float y = MathF.Cos(phi);
                        float z = MathF.Sin(phi) * MathF.Sin(theta);

                        vertices.Add(new System.Numerics.Vector3(x, y, z) * radius);
                        uvs.Add(new System.Numerics.Vector2(u, v));
                    }
                }

                for (int i = 0; i < rings; i++)
                for (int j = 0; j < slices; j++)
                {
                    int a = i * (slices + 1) + j;
                    int b = a + slices + 1;

                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(a + 1);

                    indices.Add(b);
                    indices.Add(b + 1);
                    indices.Add(a + 1);
                }

                Mesh mesh = new();
                mesh.SetPositions(vertices.ToArray());
                mesh.SetUVs(uvs.ToArray(), 0);
                mesh.SetIndices(indices.ToArray());

                mesh.RecalculateBounds();
                mesh.RecalculateNormals();
                mesh.RecalculateTangents();
                return mesh;
            }
            case PrimitiveType.Capsule:
                return null!;
            default:
                throw new ArgumentOutOfRangeException(nameof(primitiveType), primitiveType, null);
        }
    }


    public static Mesh GetFullscreenQuad()
    {
        if (fullScreenQuadCached != null)
            return fullScreenQuadCached;
        Mesh mesh = new();
        System.Numerics.Vector3[] positions = new System.Numerics.Vector3[4];
        positions[0] = new System.Numerics.Vector3(-1, -1, 0);
        positions[1] = new System.Numerics.Vector3(1, -1, 0);
        positions[2] = new System.Numerics.Vector3(-1, 1, 0);
        positions[3] = new System.Numerics.Vector3(1, 1, 0);

        int[] indices = new int[6];
        indices[0] = 0;
        indices[1] = 2;
        indices[2] = 1;
        indices[3] = 2;
        indices[4] = 3;
        indices[5] = 1;

        System.Numerics.Vector2[] uvs = new System.Numerics.Vector2[4];
        uvs[0] = new System.Numerics.Vector2(0, 0);
        uvs[1] = new System.Numerics.Vector2(1, 0);
        uvs[2] = new System.Numerics.Vector2(0, 1);
        uvs[3] = new System.Numerics.Vector2(1, 1);

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
            byte[]? attributeBytes = GetVertexAttributeBytes(attribute.Semantic);
            if (attributeBytes == null)
                continue;

            if (attributeBytes.Length % attribute.Stride != 0)
                throw new InvalidOperationException(
                    $"The vertex attribute '{(VertexAttribute)attribute.Semantic}' has an invalid stride. Expected a multiple of {attribute.Stride}, but got {attributeBytes.Length}.");

            for (int vertexIndex = 0; vertexIndex < VertexCount; vertexIndex++)
            {
                int bufferIndex = vertexIndex * vertexLayout.VertexSize + attribute.Offset;
                int attributeIndex = vertexIndex * attribute.Stride;

                for (int j = 0; j < attribute.Stride; j++)
                {
                    int sourceIndex = attributeIndex + j;
                    int destinationIndex = bufferIndex + j;
                    buffer[destinationIndex] = attributeBytes[sourceIndex];
                }
            }
        }

        return buffer;
    }


    /// <returns>The byte array containing the vertex attribute data for the specified attribute.</returns>
    private ref byte[]? GetVertexAttributeBytes(int attributeSemantic)
    {
        VertexAttribute attribute = (VertexAttribute)attributeSemantic;

        switch (attribute)
        {
            case VertexAttribute.Position:
                return ref _vertexPositions;
            case VertexAttribute.TexCoord0:
                return ref _vertexTexCoord0;
            case VertexAttribute.TexCoord1:
                return ref _vertexTexCoord1;
            case VertexAttribute.Normal:
                return ref _vertexNormals;
            case VertexAttribute.Color:
                return ref _vertexColors;
            case VertexAttribute.Color32:
                return ref _vertexColors32;
            case VertexAttribute.Tangent:
                return ref _vertexTangents;
            case VertexAttribute.BoneIndex:
                return ref _boneIndices;
            case VertexAttribute.BoneWeight:
                return ref _boneWeights;
            default:
                throw new ArgumentOutOfRangeException(nameof(attribute), attribute, null);
        }
    }


    /// <returns>The default layout based on the enabled vertex attributes.</returns>
    private MeshVertexLayout GetVertexLayout()
    {
        List<MeshVertexLayout.VertexAttributeDescriptor> attributes =
            [new MeshVertexLayout.VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeType.Float, 3)];

        if (HasUV0)
            attributes.Add(new MeshVertexLayout.VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeType.Float, 2));

        if (HasUV1)
            attributes.Add(new MeshVertexLayout.VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeType.Float, 2));

        if (HasNormals)
            attributes.Add(new MeshVertexLayout.VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeType.Float, 3, true));

        if (HasColors)
            attributes.Add(new MeshVertexLayout.VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeType.Float, 4));

        if (HasColors32)
            attributes.Add(new MeshVertexLayout.VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeType.UnsignedByte, 4));

        if (HasTangents)
            attributes.Add(new MeshVertexLayout.VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeType.Float, 3, true));

        if (HasBoneIndices)
            attributes.Add(new MeshVertexLayout.VertexAttributeDescriptor(VertexAttribute.BoneIndex, VertexAttributeType.Float, 4));

        if (HasBoneWeights)
            attributes.Add(new MeshVertexLayout.VertexAttributeDescriptor(VertexAttribute.BoneWeight, VertexAttributeType.Float, 4));

        return new MeshVertexLayout(attributes.ToArray());
    }


    private void ClearAllVertexData()
    {
        _vertexPositions = null;
        _vertexTexCoord0 = null;
        _vertexTexCoord1 = null;
        _vertexNormals = null;
        _vertexColors = null;
        _vertexColors32 = null;
        _vertexTangents = null;

        _boneIndices = null;
        _boneWeights = null;
        BindPoses = null;

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


    public SerializedProperty Serialize(Serializer.SerializationContext ctx)
    {
        SerializedProperty compoundTag = SerializedProperty.NewCompound();

        using MemoryStream memoryStream = new();
        using BinaryWriter writer = new(memoryStream);
        
        writer.Write((byte)_indexFormat);
        writer.Write((byte)_topology);

        writer.Write(_vertexPositions?.Length ?? 0);
        if (_vertexPositions != null)
            foreach (byte vertex in _vertexPositions)
                writer.Write(vertex);

        writer.Write(_vertexNormals?.Length ?? 0);
        if (_vertexNormals != null)
            foreach (byte normal in _vertexNormals)
                writer.Write(normal);

        writer.Write(_vertexTangents?.Length ?? 0);
        if (_vertexTangents != null)
            foreach (byte tangent in _vertexTangents)
                writer.Write(tangent);

        writer.Write(_vertexColors?.Length ?? 0);
        if (_vertexColors != null)
            foreach (byte color in _vertexColors)
                writer.Write(color);

        writer.Write(_vertexColors32?.Length ?? 0);
        if (_vertexColors32 != null)
            foreach (byte color in _vertexColors32)
                writer.Write(color);

        writer.Write(_vertexTexCoord0?.Length ?? 0);
        if (_vertexTexCoord0 != null)
            foreach (byte uv in _vertexTexCoord0)
                writer.Write(uv);

        writer.Write(_vertexTexCoord1?.Length ?? 0);
        if (_vertexTexCoord1 != null)
            foreach (byte uv in _vertexTexCoord1)
                writer.Write(uv);

        writer.Write(_indexData?.Length ?? 0);
        if (_indexData != null)
            foreach (byte index in _indexData)
                writer.Write(index);

        writer.Write(_boneIndices?.Length ?? 0);
        if (_boneIndices != null)
            foreach (byte boneIndex in _boneIndices)
                writer.Write(boneIndex);

        writer.Write(_boneWeights?.Length ?? 0);
        if (_boneWeights != null)
            foreach (byte boneWeight in _boneWeights)
                writer.Write(boneWeight);

        writer.Write(BindPoses?.Length ?? 0);
        if (BindPoses != null)
            foreach (System.Numerics.Matrix4x4 bindPose in BindPoses)
            {
                writer.Write(bindPose.M11);
                writer.Write(bindPose.M12);
                writer.Write(bindPose.M13);
                writer.Write(bindPose.M14);

                writer.Write(bindPose.M21);
                writer.Write(bindPose.M22);
                writer.Write(bindPose.M23);
                writer.Write(bindPose.M24);

                writer.Write(bindPose.M31);
                writer.Write(bindPose.M32);
                writer.Write(bindPose.M33);
                writer.Write(bindPose.M34);

                writer.Write(bindPose.M41);
                writer.Write(bindPose.M42);
                writer.Write(bindPose.M43);
                writer.Write(bindPose.M44);
            }


        compoundTag.Add("MeshData", new SerializedProperty(memoryStream.ToArray()));

        return compoundTag;
    }


    public void Deserialize(SerializedProperty value, Serializer.SerializationContext ctx)
    {
        using MemoryStream memoryStream = new(value["MeshData"].ByteArrayValue);
        using BinaryReader reader = new(memoryStream);

        _indexFormat = (IndexFormat)reader.ReadByte();
        _topology = (Topology)reader.ReadByte();

        _vertexPositions = reader.ReadInt32() > 0 ? reader.ReadBytes(reader.ReadInt32()) : null;
        _vertexNormals = reader.ReadInt32() > 0 ? reader.ReadBytes(reader.ReadInt32()) : null;
        _vertexTangents = reader.ReadInt32() > 0 ? reader.ReadBytes(reader.ReadInt32()) : null;
        _vertexColors = reader.ReadInt32() > 0 ? reader.ReadBytes(reader.ReadInt32()) : null;
        _vertexColors32 = reader.ReadInt32() > 0 ? reader.ReadBytes(reader.ReadInt32()) : null;
        _vertexTexCoord0 = reader.ReadInt32() > 0 ? reader.ReadBytes(reader.ReadInt32()) : null;
        _vertexTexCoord1 = reader.ReadInt32() > 0 ? reader.ReadBytes(reader.ReadInt32()) : null;

        _indexData = reader.ReadInt32() > 0 ? reader.ReadBytes(reader.ReadInt32()) : null;

        _boneIndices = reader.ReadInt32() > 0 ? reader.ReadBytes(reader.ReadInt32()) : null;
        _boneWeights = reader.ReadInt32() > 0 ? reader.ReadBytes(reader.ReadInt32()) : null;

        int bindPosesCount = reader.ReadInt32();
        if (bindPosesCount > 0)
        {
            BindPoses = new System.Numerics.Matrix4x4[bindPosesCount];
            for (int i = 0; i < bindPosesCount; i++)
                BindPoses[i] = new System.Numerics.Matrix4x4
                {
                    M11 = reader.ReadSingle(),
                    M12 = reader.ReadSingle(),
                    M13 = reader.ReadSingle(),
                    M14 = reader.ReadSingle(),

                    M21 = reader.ReadSingle(),
                    M22 = reader.ReadSingle(),
                    M23 = reader.ReadSingle(),
                    M24 = reader.ReadSingle(),

                    M31 = reader.ReadSingle(),
                    M32 = reader.ReadSingle(),
                    M33 = reader.ReadSingle(),
                    M34 = reader.ReadSingle(),

                    M41 = reader.ReadSingle(),
                    M42 = reader.ReadSingle(),
                    M43 = reader.ReadSingle(),
                    M44 = reader.ReadSingle()
                };
        }

        _isDirty = true;
    }
}