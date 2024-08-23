using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.API.Rendering.Shaders;
using KorpiEngine.Core.Internal.Rendering;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Rendering.Cameras;
using KorpiEngine.Core.Rendering.Primitives;

namespace KorpiEngine.Core.API;

public static class Gizmos
{
    private static readonly Color DefaultColor = Color.White;
    private static readonly List<(Gizmo, Matrix4x4)> GizmosList = new(100);
    private static PrimitiveBatch? lineBatch;
    private static Material? gizmosMat;

    internal static bool AllowCreation { get; set; } = false;
    public static Matrix4x4 Matrix { get; set; } = Matrix4x4.Identity;
    public static Color Color { get; set; } = DefaultColor;


    public static void DrawLine(Vector3 from, Vector3 to)
    {
        from -= Camera.RenderingCamera.Entity.Transform.Position;
        to -= Camera.RenderingCamera.Entity.Transform.Position;
        Add(new LineGizmo(from, to, Color));
    }


    public static void DrawArrow(Vector3 from, Vector3 to, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        from -= Camera.RenderingCamera.Entity.Transform.Position;
        to -= Camera.RenderingCamera.Entity.Transform.Position;
        Add(new ArrowGizmo(from, to, Color, arrowHeadLength, arrowHeadAngle));
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
        if (!AllowCreation)
            throw new InvalidOperationException("Gizmos should only be drawn inside OnDrawGizmos().");
        
        GizmosList.Add((gizmo, Matrix));
        Matrix = Matrix4x4.Identity;
    }


    public static void Render(bool enableDepthTest)
    {
        gizmosMat ??= new Material(Shader.Find("Defaults/Gizmos.kshader"), "Gizmos Material", false);
        lineBatch ??= new PrimitiveBatch(Topology.Lines);

        if (!lineBatch.IsUploaded)
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
        
        // Set raster state overrides
        Graphics.Device.SetEnableDepthTest(enableDepthTest);
        Graphics.Device.SetEnableDepthWrite(enableDepthTest);
        
        lineBatch.Draw();
    }


    public static void Clear()
    {
        GizmosList.Clear();
        lineBatch?.Reset();
    }
    
    
    public static void ResetColor()
    {
        Color = DefaultColor;
    }
}