using KorpiEngine.Core.API;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Rendering.Primitives;

namespace KorpiEngine.Core.Internal.Rendering;

public class PrimitiveBatch
{
    private struct Vertex
    {
        public float X;
        public float Y;
        public float Z;

        public float R;
        public float G;
        public float B;
        public float A;
    }

    private readonly GraphicsVertexArrayObject? _vao;
    private readonly GraphicsBuffer _vbo;
    private readonly List<Vertex> _vertices = new(50);

    private readonly Topology _primitiveType;

    public bool IsUploaded { get; private set; }


    public PrimitiveBatch(Topology primitiveType)
    {
        _primitiveType = primitiveType;

        _vbo = Graphics.Driver.CreateBuffer(BufferType.VertexBuffer, Array.Empty<byte>(), true);

        MeshVertexLayout format = new(
        [
            new MeshVertexLayout.VertexAttributeDescriptor(0, VertexAttributeType.Float, 3),
            new MeshVertexLayout.VertexAttributeDescriptor(1, VertexAttributeType.Float, 4)
        ]);

        _vao = Graphics.Driver.CreateVertexArray(format, _vbo, null);

        IsUploaded = false;
    }


    public void Reset()
    {
        _vertices.Clear();
        IsUploaded = false;
    }


    public void Line(Vector3 a, Vector3 b, Color colorA, Color colorB)
    {
        System.Numerics.Vector3 af = a;
        System.Numerics.Vector3 bf = b;
        _vertices.Add(
            new Vertex
            {
                X = af.X,
                Y = af.Y,
                Z = af.Z,
                R = colorA.R,
                G = colorA.G,
                B = colorA.B,
                A = colorA.A
            });
        _vertices.Add(
            new Vertex
            {
                X = bf.X,
                Y = bf.Y,
                Z = bf.Z,
                R = colorB.R,
                G = colorB.G,
                B = colorB.B,
                A = colorB.A
            });
    }


    public void Upload()
    {
        if (_vertices.Count == 0)
            return;

        Graphics.Driver.SetBuffer(_vbo, _vertices.ToArray(), true);

        IsUploaded = true;
    }


    public void Draw()
    {
        if (_vertices.Count == 0 || _vao == null)
            return;

        Graphics.Driver.BindVertexArray(_vao);
        Graphics.Driver.DrawArrays(_primitiveType, 0, _vertices.Count);
    }
}