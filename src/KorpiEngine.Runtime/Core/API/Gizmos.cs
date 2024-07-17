using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.API.Rendering.Shaders;
using KorpiEngine.Core.Internal.Rendering;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Rendering.Cameras;
using KorpiEngine.Core.Rendering.Primitives;

namespace KorpiEngine.Core.API;

public static class Gizmos
{
    private static readonly List<(Gizmo, Matrix4x4)> GizmosList = new(100);
    private static PrimitiveBatch? lineBatch;
    private static Material? gizmosMat;

    public static Matrix4x4 Matrix = Matrix4x4.Identity;
    public static Color Color = Color.White;


    public static void DrawLine(Vector3 from, Vector3 to)
    {
        from -= Camera.RenderingCamera.Entity.Transform.Position;
        to -= Camera.RenderingCamera.Entity.Transform.Position;
        Add(new LineGizmo(from, to, Color));
    }


    public static void DrawCube(Vector3 center, Vector3 size)
    {
        center -= Camera.RenderingCamera.Entity.Transform.Position;
        Matrix = Matrix4x4.CreateScale(size) * Matrix * Matrix4x4.CreateTranslation(center);
        Add(new CubeGizmo(Color));
    }


    public static void DrawPolygon(Vector3[] points, bool closed = false)
    {
        for (int i = 0; i < points.Length; i++)
            points[i] -= Camera.RenderingCamera.Entity.Transform.Position;
        Add(new PolygonGizmo(points, Color, closed));
    }


    public static void DrawCylinder(Vector3 center, float radius, float height)
    {
        center -= Camera.RenderingCamera.Entity.Transform.Position;
        Matrix = Matrix4x4.CreateScale(new Vector3(radius * 2f, height, radius * 2f)) * Matrix * Matrix4x4.CreateTranslation(center);
        Add(new CylinderGizmo(Color));
    }


    public static void DrawCapsule(Vector3 center, float radius, float height)
    {
        center -= Camera.RenderingCamera.Entity.Transform.Position;
        Matrix = Matrix4x4.CreateScale(new Vector3(radius * 2f, height, radius * 2f)) * Matrix * Matrix4x4.CreateTranslation(center);
        Add(new CapsuleGizmo(Color));
    }


    public static void DrawCircle(Vector3 center, float radius)
    {
        center -= Camera.RenderingCamera.Entity.Transform.Position;
        Matrix = Matrix4x4.CreateScale(new Vector3(radius, radius, radius)) * Matrix * Matrix4x4.CreateTranslation(center);
        Add(new CircleGizmo(Color));
    }


    public static void DrawSphere(Vector3 center, float radius)
    {
        center -= Camera.RenderingCamera.Entity.Transform.Position;
        Matrix = Matrix4x4.CreateScale(new Vector3(radius, radius, radius)) * Matrix * Matrix4x4.CreateTranslation(center);
        Add(new SphereGizmo(Color));
    }


    public static void DrawDirectionalLight(Vector3 center)
    {
        center -= Camera.RenderingCamera.Entity.Transform.Position;
        Matrix = Matrix * Matrix4x4.CreateTranslation(center);
        Add(new DirectionalLightGizmo(Color));
    }


    public static void DrawSpotlight(Vector3 position, float distance, float spotAngle)
    {
        position -= Camera.RenderingCamera.Entity.Transform.Position;
        Matrix = Matrix * Matrix4x4.CreateTranslation(position);
        Add(new SpotlightGizmo(distance, spotAngle, Color));
    }


    public static void Add(Gizmo gizmo)
    {
        GizmosList.Add((gizmo, Matrix));
        Matrix = Matrix4x4.Identity;
    }


    public static void Render()
    {
        gizmosMat ??= new Material(Shader.Find("Defaults/Gizmos.shader"), "Gizmos Material");
        lineBatch ??= new PrimitiveBatch(Topology.Lines);

        if (lineBatch.IsUploaded == false)
        {
            foreach ((Gizmo, Matrix4x4) gizmo in GizmosList)
            {
                try
                {
                    gizmo.Item1.Render(lineBatch, gizmo.Item2);
                }
                catch
                {
                    // Nothing, errors are normal here
                }
            }

            lineBatch.Upload();
        }

        Matrix4x4 mvp = Matrix4x4.Identity;
        mvp = Matrix4x4.Multiply(mvp, Graphics.ViewMatrix);
        mvp = Matrix4x4.Multiply(mvp, Graphics.ProjectionMatrix);
        gizmosMat.SetMatrix("_MatMVP", mvp);
        gizmosMat.SetPass(0, true);
        lineBatch.Draw();
    }


    public static void Clear()
    {
        GizmosList.Clear();
        lineBatch?.Reset();
        Color = Color.White;
    }
}