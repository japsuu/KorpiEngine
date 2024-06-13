using KorpiEngine.Core.API;
using KorpiEngine.Core.API.Rendering;
using KorpiEngine.Core.Internal.Rendering;
using KorpiEngine.Core.Rendering.Primitives;

namespace KorpiEngine.Core.Rendering;

public class PrimitiveBatch
{
    private struct Vertex
    {
        public float x;
        public float y;
        public float z;
        
        public float r;
        public float g;
        public float b;
        public float a;
    }

    private GraphicsVertexArrayObject? vao;
    private GraphicsBuffer vbo;
    private List<Vertex> vertices = new(50);
    private Mesh mesh;

    private Topology primitiveType;

    public bool IsUploaded { get; private set; }


    public PrimitiveBatch(Topology primitiveType)
    {
        this.primitiveType = primitiveType;

        vbo = Graphics.Driver.CreateBuffer(BufferType.VertexBuffer, new byte[0], true);
        MeshVertexLayout format = new(
        [
            new MeshVertexLayout.VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeType.Float, 3),
#warning VertexAttribute.TexCoord0 as VertexAttributeType.Float, 4 is not correct
            new MeshVertexLayout.VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeType.Float, 4)
        ]);

        vao = Graphics.Driver.CreateVertexArray(format, vbo, null);

        IsUploaded = false;
    }


    public void Reset()
    {
        vertices.Clear();
        IsUploaded = false;
    }


    public void Line(Vector3 a, Vector3 b, Color colorA, Color colorB)
    {
        System.Numerics.Vector3 af = a;
        System.Numerics.Vector3 bf = b;
        vertices.Add(
            new Vertex
            {
                x = af.X,
                y = af.Y,
                z = af.Z,
                r = colorA.R,
                g = colorA.G,
                b = colorA.B,
                a = colorA.A
            });
        vertices.Add(
            new Vertex
            {
                x = bf.X,
                y = bf.Y,
                z = bf.Z,
                r = colorB.R,
                g = colorB.G,
                b = colorB.B,
                a = colorB.A
            });
    }


    // Implement Quad and QuadWire similarly...


    public void Upload()
    {
        if (vertices.Count == 0)
            return;

        Graphics.Driver.SetBuffer(vbo, vertices.ToArray(), true);

        IsUploaded = true;
    }


    public void Draw()
    {
        if (vertices.Count == 0 || vao == null)
            return;

        Graphics.Driver.BindVertexArray(vao);
        Graphics.Driver.DrawArrays(primitiveType, 0, vertices.Count);
    }
}