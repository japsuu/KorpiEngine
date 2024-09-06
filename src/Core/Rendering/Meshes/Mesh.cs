using System.Runtime.InteropServices;
using KorpiEngine.AssetManagement;
using KorpiEngine.Utils;
using InvalidOperationException = System.InvalidOperationException;

namespace KorpiEngine.Rendering;

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
public sealed class Mesh : AssetInstance //TODO: Implement MeshData class to hide some fields
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
    public AABox Bounds { get; private set; }

    /// <summary>
    /// The bind poses of the mesh, used for skinning.
    /// </summary>
    public Matrix4x4[]? BindPoses { get; set; }

    public bool HasVertexUV0 => (_vertexTexCoord0?.Length ?? 0) > 0;
    public bool HasVertexUV1 => (_vertexTexCoord1?.Length ?? 0) > 0;
    public bool HasVertexNormals => (_vertexNormals?.Length ?? 0) > 0;
    public bool HasVertexTangents => (_vertexTangents?.Length ?? 0) > 0;
    public bool HasVertexColors => (_vertexColors?.Length ?? 0) > 0;
    public bool HasBoneWeights => (_boneWeights?.Length ?? 0) > 0;
    public bool HasBoneIndices => (_boneIndices?.Length ?? 0) > 0;
    
    internal GraphicsVertexArrayObject? VertexArrayObject { get; private set; }

    private static Mesh? fullScreenQuadCached;
    private Topology _topology = Topology.Triangles;
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
    private byte[]? _vertexTangents;
    private byte[]? _vertexColors;
    private byte[]? _boneWeights;
    private byte[]? _boneIndices;


    protected override void OnDispose(bool manual)
    {
#if TOOLS
        if (!manual)
            throw new ResourceLeakException($"Mesh '{Name}' was not disposed of explicitly, and is now being disposed by the GC. This is a memory leak!");
#endif

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
                throw new InvalidOperationException($"Unknown topology: {Topology}");
        }

        MeshVertexLayout vertexLayout = GetVertexLayout();

        byte[]? vertexDataBlob = MakeVertexDataBlob(vertexLayout);

        if (vertexDataBlob == null || _indexData == null)
            return;

        _vertexBuffer = Graphics.Device.CreateBuffer(BufferType.VertexBuffer, vertexDataBlob);
        _indexBuffer = Graphics.Device.CreateBuffer(BufferType.ElementsBuffer, _indexData);
        VertexArrayObject = Graphics.Device.CreateVertexArray(vertexLayout, _vertexBuffer, _indexBuffer);

        Graphics.Device.BindVertexArray(null);

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
            throw new InvalidOperationException("Cannot recalculate bounds without vertex positions.");

        Bounds = AABox.Create(GetVertexPositions()!);
    }


    public void RecalculateNormals()
    {
        if (_vertexPositions == null || _indexData == null)
            return;
        
        Vector3[] vertices = GetVertexPositions()!;
        if (vertices.Length < 3)
            return;
        
        int[] indices = GetIndices()!;
        if (indices.Length < 3)
            return;

        Vector3[] normals = new Vector3[vertices.Length];

        for (int i = 0; i < indices.Length; i += 3)
        {
            int ai = indices[i];
            int bi = indices[i + 1];
            int ci = indices[i + 2];

            Vector3 n = MathOps.Normalize(
                MathOps.Cross(
                    vertices[bi] - vertices[ai],
                    vertices[ci] - vertices[ai]
                ));

            normals[ai] += n;
            normals[bi] += n;
            normals[ci] += n;
        }

        for (int i = 0; i < vertices.Length; i++)
            normals[i] = MathOps.Normalize(normals[i]);

        SetVertexNormals(normals);
    }


    public void RecalculateTangents()
    {
        if (_vertexPositions == null || _indexData == null)
            return;
        
        Vector3[] vertices = GetVertexPositions()!;
        if (vertices.Length < 3)
            return;
        
        int[] indices = GetIndices()!;
        if (indices.Length < 3)
            return;
        
        if (_vertexTexCoord0 == null)
            return;
        Vector2[] uv = GetVertexUVs(0)!;

        Vector3[] tangents = new Vector3[vertices.Length];

        for (int i = 0; i < indices.Length; i += 3)
        {
            int ai = indices[i];
            int bi = indices[i + 1];
            int ci = indices[i + 2];

            Vector3 edge1 = vertices[bi] - vertices[ai];
            Vector3 edge2 = vertices[ci] - vertices[ai];

            Vector2 deltaUV1 = uv[bi] - uv[ai];
            Vector2 deltaUV2 = uv[ci] - uv[ai];

            float f = 1.0f / (deltaUV1.X * deltaUV2.Y - deltaUV2.X * deltaUV1.Y);

            float x = f * (deltaUV2.Y * edge1.X - deltaUV1.Y * edge2.X);
            float y = f * (deltaUV2.Y * edge1.Y - deltaUV1.Y * edge2.Y);
            float z = f * (deltaUV2.Y * edge1.Z - deltaUV1.Y * edge2.Z);
            Vector3 tangent = new Vector3(x, y, z);

            tangents[ai] += tangent;
            tangents[bi] += tangent;
            tangents[ci] += tangent;
        }

        for (int i = 0; i < vertices.Length; i++)
            tangents[i] = MathOps.Normalize(tangents[i]);

        SetVertexTangents(tangents);
    }


    #region SIMPLE API

    public int GetVertexPositionsNonAlloc(IList<Vector3> destination) => GetVertexAttributeDataNonAlloc(_vertexPositions, destination);
    public Vector3[]? GetVertexPositions() => GetVertexAttributeData<Vector3>(_vertexPositions);
    public void SetVertexPositions(ArraySegment<Vector3>? positions) => SetVertexAttributeData(positions, ref _vertexPositions);


    public int GetVertexUVsNonAlloc(IList<Vector2> destination, int channel) => GetVertexAttributeDataNonAlloc(
        VertexAttribute.TexCoord0 + channel == VertexAttribute.TexCoord0 ? _vertexTexCoord0 : _vertexTexCoord1, destination);
    public Vector2[]? GetVertexUVs(int channel) =>
        GetVertexAttributeData<Vector2>(VertexAttribute.TexCoord0 + channel == VertexAttribute.TexCoord0 ? _vertexTexCoord0 : _vertexTexCoord1);
    public void SetVertexUVs(ArraySegment<Vector2>? uvs, int channel)
    {
        if (channel == 0)
            SetVertexAttributeData(uvs, ref _vertexTexCoord0);
        else
            SetVertexAttributeData(uvs, ref _vertexTexCoord1);
    }

    
    public int GetVertexNormalsNonAlloc(IList<Vector3> destination) => GetVertexAttributeDataNonAlloc(_vertexNormals, destination);
    public Vector3[]? GetVertexNormals() => GetVertexAttributeData<Vector3>(_vertexNormals);
    public void SetVertexNormals(ArraySegment<Vector3>? normals) => SetVertexAttributeData(normals, ref _vertexNormals);

    
    public int GetVertexTangentsNonAlloc(IList<Vector3> destination) => GetVertexAttributeDataNonAlloc(_vertexTangents, destination);
    public Vector3[]? GetVertexTangents() => GetVertexAttributeData<Vector3>(_vertexTangents);
    public void SetVertexTangents(ArraySegment<Vector3>? tangents) => SetVertexAttributeData(tangents, ref _vertexTangents);

    
    public int GetVertexColorsNonAlloc(IList<ColorRGBA> destination) => GetVertexAttributeDataNonAlloc(_vertexColors, destination);
    public ColorRGBA[]? GetVertexColors() => GetVertexAttributeData<ColorRGBA>(_vertexColors);
    public void SetVertexColors(ArraySegment<ColorRGBA>? colors) => SetVertexAttributeData(colors, ref _vertexColors);

    
    public int GetBoneWeightsNonAlloc(IList<Vector4> destination) => GetVertexAttributeDataNonAlloc(_boneWeights, destination);
    public Vector4[]? GetBoneWeights() => GetVertexAttributeData<Vector4>(_boneWeights);
    public void SetBoneWeights(ArraySegment<Vector4>? weights) => SetVertexAttributeData(weights, ref _boneWeights);

    
    public int GetBoneIndicesNonAlloc(IList<Vector4> destination) => GetVertexAttributeDataNonAlloc(_boneIndices, destination);
    public Vector4[]? GetBoneIndices() => GetVertexAttributeData<Vector4>(_boneIndices);
    public void SetBoneIndices(ArraySegment<Vector4>? indices) => SetVertexAttributeData(indices, ref _boneIndices);


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
                throw new InvalidOperationException("Invalid index format.");
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
                throw new InvalidOperationException("Invalid index format.");
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
                throw new InvalidOperationException("Invalid index format.");
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
                Vector3[] positions =
                [
                    new(-0.5f, -0.5f, 0),
                    new(0.5f, -0.5f, 0),
                    new(0.5f, 0.5f, 0),
                    new(-0.5f, 0.5f, 0)
                ];

                Vector2[] uvs =
                [
                    new(0, 0),
                    new(1, 0),
                    new(1, 1),
                    new(0, 1)
                ];

                int[] indices = [0, 1, 2, 2, 3, 0];

                Mesh mesh = new();
                mesh.SetVertexPositions(positions);
                mesh.SetVertexUVs(uvs, 0);
                mesh.SetIndices(indices);
                
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();
                mesh.RecalculateTangents();
                
                return mesh;
            }
            case PrimitiveType.Cube:
            {
                Vector3 size = new(1, 1, 1);
                return CreateCube(size);
            }
            case PrimitiveType.Sphere:
            {
                const float radius = 0.5f;
                const int rings = 8;
                const int slices = 8;
                return CreateSphere(radius, rings, slices);
            }
            case PrimitiveType.Capsule:
                return null!;
            case PrimitiveType.Torus:
            {
                const float radius1 = 1f;
                const float radius2 = .3f;
                const int nbRadSeg = 24;
                const int nbSides = 18;
                return CreateTorus(radius1, radius2, nbRadSeg, nbSides);
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(primitiveType), primitiveType, null);
        }
    }


    /// <summary>
    /// Creates a new mesh with a cube shape.
    /// </summary>
    /// <param name="size">The size of the cube.</param>
    /// <returns></returns>
    public static Mesh CreateCube(Vector3 size)
    {
        float x = size.X / 2f;
        float y = size.Y / 2f;
        float z = size.Z / 2f;

        Vector3[] positions =
        [
            // X+ face
            new(x, -y, z), new(x, -y, -z), new(x, y, -z), new(x, y, z),
            // X- face
            new(-x, -y, -z), new(-x, -y, z), new(-x, y, z), new(-x, y, -z),
            // Y+ face
            new(-x, y, -z), new(-x, y, z), new(x, y, z), new(x, y, -z),
            // Y- face
            new(-x, -y, z), new(-x, -y, -z), new(x, -y, -z), new(x, -y, z),
            // Z+ face
            new(-x, -y, z), new(x, -y, z), new(x, y, z), new(-x, y, z),
            // Z- face
            new(x, -y, -z), new(-x, -y, -z), new(-x, y, -z), new(x, y, -z)
        ];

        Vector2[] uvs =
        [
            // X+ face
            new(0, 0), new(1, 0), new(1, 1), new(0, 1),
            // X- face
            new(0, 0), new(1, 0), new(1, 1), new(0, 1),
            // Y+ face
            new(0, 0), new(1, 0), new(1, 1), new(0, 1),
            // Y- face
            new(0, 0), new(1, 0), new(1, 1), new(0, 1),
            // Z+ face
            new(0, 0), new(1, 0), new(1, 1), new(0, 1),
            // Z- face
            new(0, 0), new(1, 0), new(1, 1), new(0, 1)
        ];

        int[] indices = new int[6 * 6];
        for (int i = 0; i < 6; i++)
        {
            // First triangle.
            indices[i * 6 + 0] = i * 4 + 0;
            indices[i * 6 + 1] = i * 4 + 1;
            indices[i * 6 + 2] = i * 4 + 2;
            // Second triangle.
            indices[i * 6 + 3] = i * 4 + 2;
            indices[i * 6 + 4] = i * 4 + 3;
            indices[i * 6 + 5] = i * 4 + 0;
        }

        Mesh mesh = new();
        mesh.SetVertexPositions(positions);
        mesh.SetVertexUVs(uvs, 0);
        mesh.SetIndices(indices);
                
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        return mesh;
    }


    /// <summary>
    /// Creates a new mesh with a sphere shape.
    /// </summary>
    /// <param name="radius">The radius of the sphere.</param>
    /// <param name="rings">Latitude ---</param>
    /// <param name="slices">Longitude |||</param>
    /// <returns></returns>
    public static Mesh CreateSphere(float radius, int rings, int slices)
    {
        Vector3[] vertices = CreateSphereVertices(radius, rings, slices);
        Vector3[] normals = CreateSphereNormals(vertices);
        Vector2[] uvs = CreateSphereUVs(rings, slices, vertices);
        int[] triangles = CreateSphereTriangles(rings, slices, vertices);
        Vector3[] tangents = CreateSphereTangents(vertices);

        Mesh mesh = new();
        if (vertices.Length > 65535)
            mesh.IndexFormat = IndexFormat.UInt32;
        
        mesh.Name = "UV Sphere";
        mesh.SetVertexPositions(vertices);
        mesh.SetVertexNormals(normals);
        mesh.SetVertexUVs(uvs, 0);
        mesh.SetVertexTangents(tangents);
        mesh.SetIndices(triangles);
        mesh.RecalculateBounds();
        
        return mesh;
    }


    private static Vector3[] CreateSphereVertices(float radius, int rings, int slices)
    {
        Vector3[] vertices = new Vector3[(slices + 1) * (rings + 1)];
        const float pi = MathF.PI;
        const float _2pi = pi * 2f;

        for (int lat = 0; lat <= rings; lat++)
        {
            float a1 = pi * lat / rings;
            float sin1 = MathF.Sin(a1);
            float cos1 = MathF.Cos(a1);

            for (int lon = 0; lon <= slices; lon++)
            {
                float a2 = pi + _2pi * (lon == slices ? 0 : lon) / slices;
                float sin2 = MathF.Sin(a2);
                float cos2 = MathF.Cos(a2);

                vertices[lon + lat * (slices + 1)] = new Vector3(sin1 * cos2, cos1, sin1 * sin2) * radius;
            }
        }

        return vertices;
    }


    private static Vector3[] CreateSphereNormals(Vector3[] vertices)
    {
        Vector3[] normals = new Vector3[vertices.Length];
        for (int n = 0; n < vertices.Length; n++)
            normals[n] = MathOps.Normalize(vertices[n]);
        return normals;
    }


    private static Vector2[] CreateSphereUVs(int rings, int slices, Vector3[] vertices)
    {
        Vector2[] uvs = new Vector2[vertices.Length];
        uvs[0] = Vector2.Up;
        uvs[^1] = Vector2.Zero;
        for (int lat = 0; lat <= rings; lat++)
        {
            for (int lon = 0; lon <= slices; lon++)
                uvs[lon + lat * (slices + 1)] = new Vector2((float)lon / slices, 1f - (float)lat / rings);
        }
        return uvs;
    }


    private static int[] CreateSphereTriangles(int rings, int slices, Vector3[] vertices)
    {
        int nbFaces = vertices.Length;
        int nbTriangles = nbFaces * 2;
        int nbIndexes = nbTriangles * 3;
        int[] triangles = new int[nbIndexes];

        // Middle
        int i = 0;
        for (int lat = 0; lat < rings; lat++)
        {
            for (int lon = 0; lon < slices; lon++)
            {
                int current = lon + lat * (slices + 1);
                int next = current + (slices + 1);

                triangles[i++] = current;
                triangles[i++] = current + 1;
                triangles[i++] = next + 1;

                triangles[i++] = current;
                triangles[i++] = next + 1;
                triangles[i++] = next;
            }
        }

        return triangles;
    }

    
    private static Vector3[] CreateSphereTangents(Vector3[] vertices)
    {
        Vector3[] tangents = new Vector3[vertices.Length];
            
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = vertices[i];
            if (MathOps.AlmostZero(v.X) && MathOps.AlmostZero(v.Z))
                tangents[i] = new Vector3(1, 0, 0);
            else
                tangents[i] = MathOps.Normalize(new Vector3(v.Z, 0, -v.X));
        }

        return tangents;
    }


    public static Mesh CreateTorus(float radiusOuter, float radiusInner, int radialSegments, int sideSegments)
    {
        throw new NotImplementedException();
    }


    public static Mesh GetFullscreenQuad()
    {
        if (fullScreenQuadCached != null)
            return fullScreenQuadCached;
        
        Mesh mesh = new();
        Vector3[] positions = new Vector3[4];
        positions[0] = new Vector3(-1, -1, 0);
        positions[1] = new Vector3(1, -1, 0);
        positions[2] = new Vector3(1, 1, 0);
        positions[3] = new Vector3(-1, 1, 0);

        int[] indices = [0, 1, 2, 2, 3, 0];

        Vector2[] uvs = new Vector2[4];
        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(1, 0);
        uvs[2] = new Vector2(1, 1);
        uvs[3] = new Vector2(0, 1);

        mesh.SetVertexPositions(positions);
        mesh.SetIndices(indices);
        mesh.SetVertexUVs(uvs, 0);
        
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

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

        if (attribute == VertexAttribute.Position)
            return ref _vertexPositions;
        if (attribute == VertexAttribute.TexCoord0)
            return ref _vertexTexCoord0;
        if (attribute == VertexAttribute.TexCoord1)
            return ref _vertexTexCoord1;
        if (attribute == VertexAttribute.Normal)
            return ref _vertexNormals;
        if (attribute == VertexAttribute.Tangent)
            return ref _vertexTangents;
        if (attribute == VertexAttribute.Color)
            return ref _vertexColors;
        if (attribute == VertexAttribute.BoneWeights)
            return ref _boneWeights;
        if (attribute == VertexAttribute.BoneIndices)
            return ref _boneIndices;
        throw new ArgumentOutOfRangeException(nameof(attributeSemantic), attribute, null);
    }


    /// <returns>The default layout based on the enabled vertex attributes.</returns>
    private MeshVertexLayout GetVertexLayout()
    {
        List<MeshVertexLayout.VertexAttributeDescriptor> attributes =
            [new MeshVertexLayout.VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeType.Float, 3)];

        if (HasVertexUV0)
            attributes.Add(new MeshVertexLayout.VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeType.Float, 2));

        if (HasVertexUV1)
            attributes.Add(new MeshVertexLayout.VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeType.Float, 2));

        if (HasVertexNormals)
            attributes.Add(new MeshVertexLayout.VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeType.Float, 3, true));

        if (HasVertexTangents)
            attributes.Add(new MeshVertexLayout.VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeType.Float, 3, true));

        if (HasVertexColors)
            attributes.Add(new MeshVertexLayout.VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeType.Float, 4));
        
        if (HasBoneWeights)
            attributes.Add(new MeshVertexLayout.VertexAttributeDescriptor(VertexAttribute.BoneWeights, VertexAttributeType.Float, 4));
        
        if (HasBoneIndices)
            attributes.Add(new MeshVertexLayout.VertexAttributeDescriptor(VertexAttribute.BoneIndices, VertexAttributeType.Float, 4));

        return new MeshVertexLayout(attributes.ToArray());
    }


    private void ClearAllVertexData()
    {
        _vertexPositions = null;
        _vertexTexCoord0 = null;
        _vertexTexCoord1 = null;
        _vertexNormals = null;
        _vertexTangents = null;
        _vertexColors = null;
        _boneWeights = null;
        _boneIndices = null;
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