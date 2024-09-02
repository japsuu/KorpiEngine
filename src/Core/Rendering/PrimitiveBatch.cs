using KorpiEngine.Rendering.Primitives;

namespace KorpiEngine.Rendering;

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

        _vbo = Graphics.Device.CreateBuffer(BufferType.VertexBuffer, Array.Empty<byte>(), true);

        MeshVertexLayout format = new(
        [
            new MeshVertexLayout.VertexAttributeDescriptor(0, VertexAttributeType.Float, 3),
            new MeshVertexLayout.VertexAttributeDescriptor(1, VertexAttributeType.Float, 4)
        ]);

        _vao = Graphics.Device.CreateVertexArray(format, _vbo, null);

        IsUploaded = false;
    }


    public void Reset()
    {
        _vertices.Clear();
        IsUploaded = false;
    }


    public void Line(Vector3 a, Vector3 b, ColorHDR colorA, ColorHDR colorB)
    {
        _vertices.Add(
            new Vertex
            {
                X = a.X,
                Y = a.Y,
                Z = a.Z,
                R = colorA.R,
                G = colorA.G,
                B = colorA.B,
                A = colorA.A
            });
        _vertices.Add(
            new Vertex
            {
                X = b.X,
                Y = b.Y,
                Z = b.Z,
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

        Graphics.Device.SetBuffer(_vbo, _vertices.ToArray(), true);

        IsUploaded = true;
    }


    public void Draw()
    {
        if (_vertices.Count == 0 || _vao == null)
            return;

        Graphics.Device.BindVertexArray(_vao);
        Graphics.Device.DrawArrays(_primitiveType, 0, _vertices.Count);
    }
}