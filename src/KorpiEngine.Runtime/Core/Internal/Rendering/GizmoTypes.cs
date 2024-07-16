using KorpiEngine.Core.API;

namespace KorpiEngine.Core.Internal.Rendering;

public abstract class Gizmo
{
    public Matrix4x4 Matrix;


    public Vector3 Pos(Vector3 worldPos)
    {
        Vector3 transformedPos = Vector3.Transform(worldPos, Matrix);
        return transformedPos;
    }


    public abstract void Render(PrimitiveBatch batch, Matrix4x4 worldMatrix);
}

public class LineGizmo(Vector3 start, Vector3 end, Color color) : Gizmo
{
    public override void Render(PrimitiveBatch batch, Matrix4x4 m)
    {
        Matrix = m;
        batch.Line(Pos(start), Pos(end), color, color);
    }
}

public class PolygonGizmo(Vector3[] points, Color color, bool closed = false) : Gizmo
{
    public override void Render(PrimitiveBatch batch, Matrix4x4 m)
    {
        Matrix = m;
        for (int i = 0; i < points.Length - 1; i++)
            batch.Line(Pos(points[i]), Pos(points[i + 1]), color, color);
        if (closed)
            batch.Line(Pos(points[^1]), Pos(points[0]), color, color);
    }
}

public class CircleGizmo(Color color) : Gizmo
{
    public override void Render(PrimitiveBatch batch, Matrix4x4 m)
    {
        Matrix = m;

        int numSegments = 12;

        for (int i = 0; i < numSegments; i++)
        {
            float angle = (float)i / numSegments * 2f * MathF.PI;
            float angle2 = (float)(i + 1) / numSegments * 2f * MathF.PI;

            Vector3 point1 = new(MathF.Cos(angle), MathF.Sin(angle), 0f);
            Vector3 point2 = new(MathF.Cos(angle2), MathF.Sin(angle2), 0f);

            batch.Line(Pos(point1), Pos(point2), color, color);
        }
    }
}

public class DirectionalLightGizmo(Color color) : Gizmo
{
    public override void Render(PrimitiveBatch batch, Matrix4x4 m)
    {
        Matrix = m;

        int numSegments = 6; // Adjust for smoother or more segmented circle

        for (int i = 0; i < numSegments; i++)
        {
            float angle = (float)i / numSegments * 2f * MathF.PI;
            float angle2 = (float)(i + 1) / numSegments * 2f * MathF.PI;

            Vector3 point1 = 0.5f * new Vector3(MathF.Cos(angle), MathF.Sin(angle), 0f);
            Vector3 point2 = 0.5f * new Vector3(MathF.Cos(angle2), MathF.Sin(angle2), 0f);

            batch.Line(Pos(point1), Pos(point1 + Vector3.Forward), color, color);
            batch.Line(Pos(point1), Pos(point2), color, color);
        }
    }
}

public class SphereGizmo(Color color) : Gizmo
{
    public override void Render(PrimitiveBatch batch, Matrix4x4 m)
    {
        Matrix = m;

        // Use 3 Circle3D gizmo's
        new CircleGizmo(color).Render(batch, m);
        m = Matrix4x4.CreateRotationX(MathF.PI / 2f) * m;
        new CircleGizmo(color).Render(batch, m);
        m = Matrix4x4.CreateRotationY(MathF.PI / 2f) * m;
        new CircleGizmo(color).Render(batch, m);
    }
}

public class CubeGizmo(Color color) : Gizmo
{
    public override void Render(PrimitiveBatch batch, Matrix4x4 m)
    {
        Matrix = m;

        // Draw cube lines
        Vector3[] points = new Vector3[8];
        points[0] = new Vector3(-0.5f, -0.5f, -0.5f);
        points[1] = new Vector3(0.5f, -0.5f, -0.5f);
        points[2] = new Vector3(0.5f, 0.5f, -0.5f);
        points[3] = new Vector3(-0.5f, 0.5f, -0.5f);
        points[4] = new Vector3(-0.5f, -0.5f, 0.5f);
        points[5] = new Vector3(0.5f, -0.5f, 0.5f);
        points[6] = new Vector3(0.5f, 0.5f, 0.5f);
        points[7] = new Vector3(-0.5f, 0.5f, 0.5f);

        batch.Line(Pos(points[0]), Pos(points[1]), color, color);
        batch.Line(Pos(points[1]), Pos(points[2]), color, color);
        batch.Line(Pos(points[2]), Pos(points[3]), color, color);
        batch.Line(Pos(points[3]), Pos(points[0]), color, color);
        batch.Line(Pos(points[4]), Pos(points[5]), color, color);
        batch.Line(Pos(points[5]), Pos(points[6]), color, color);
        batch.Line(Pos(points[6]), Pos(points[7]), color, color);
        batch.Line(Pos(points[7]), Pos(points[4]), color, color);
        batch.Line(Pos(points[0]), Pos(points[4]), color, color);
        batch.Line(Pos(points[1]), Pos(points[5]), color, color);
        batch.Line(Pos(points[2]), Pos(points[6]), color, color);
        batch.Line(Pos(points[3]), Pos(points[7]), color, color);
    }
}

public class CylinderGizmo(Color color) : Gizmo
{
    public override void Render(PrimitiveBatch batch, Matrix4x4 m)
    {
        Matrix = m;

        int numSegments = 12; // Adjust for smoother or more segmented circle

        for (int i = 0; i < numSegments; i++)
        {
            float angle = (float)i / numSegments * 2f * MathF.PI;
            float angle2 = (float)(i + 1) / numSegments * 2f * MathF.PI;

            Vector3 point1 = new Vector3(MathF.Cos(angle), -1f, MathF.Sin(angle)) * 0.5;
            Vector3 point2 = new Vector3(MathF.Cos(angle2), -1f, MathF.Sin(angle2)) * 0.5;
            Vector3 point3 = new Vector3(MathF.Cos(angle), 1f, MathF.Sin(angle)) * 0.5;
            Vector3 point4 = new Vector3(MathF.Cos(angle2), 1f, MathF.Sin(angle2)) * 0.5;

            batch.Line(Pos(point1), Pos(point2), color, color);
            batch.Line(Pos(point3), Pos(point4), color, color);
            batch.Line(Pos(point1), Pos(point3), color, color);
        }
    }
}

public class CapsuleGizmo(Color color) : Gizmo
{
    public override void Render(PrimitiveBatch batch, Matrix4x4 m)
    {
        Matrix = m;

        int numSegments = 12; // Adjust for smoother or more segmented circle

        // Draw the cylinder part
        for (int i = 0; i < numSegments; i++)
        {
            float angle = (float)i / numSegments * 2f * MathF.PI;
            float angle2 = (float)(i + 1) / numSegments * 2f * MathF.PI;

            Vector3 point1 = new Vector3(MathF.Cos(angle), -1f, MathF.Sin(angle)) * 0.5;
            Vector3 point2 = new Vector3(MathF.Cos(angle2), -1f, MathF.Sin(angle2)) * 0.5;
            Vector3 point3 = new Vector3(MathF.Cos(angle), 1f, MathF.Sin(angle)) * 0.5;
            Vector3 point4 = new Vector3(MathF.Cos(angle2), 1f, MathF.Sin(angle2)) * 0.5;

            batch.Line(Pos(point1), Pos(point2), color, color);
            batch.Line(Pos(point3), Pos(point4), color, color);
            batch.Line(Pos(point1), Pos(point3), color, color);
        }

        // Draw the Top Half Sphere
        Matrix4x4 topMatrix = Matrix4x4.CreateTranslation(0f, 0.5f, 0f) * m;
        Matrix = topMatrix;
        DrawHalfSphere(batch, color, true);

        // Draw the Bottom Half Sphere
        Matrix4x4 bottomMatrix = Matrix4x4.CreateTranslation(0f, -0.5f, 0f) * m;
        Matrix = bottomMatrix;
        DrawHalfSphere(batch, color, false);
    }


    private void DrawHalfSphere(PrimitiveBatch batch, Color lineColor, bool isTop)
    {
        int numSegments = 12; // Adjust for smoother or more segmented circle

        float angleStart = isTop ? 0f : (float)Math.PI;
        float angleEnd = isTop ? (float)Math.PI / 2f : (float)Math.PI * 3f / 2f;

        for (int i = 0; i < numSegments / 4; i++)
        {
            float angle1 = angleStart + (float)i / (numSegments / 4) * (angleEnd - angleStart);
            float angle2 = angleStart + (float)(i + 1) / (numSegments / 4) * (angleEnd - angleStart);

            for (int j = 0; j < numSegments; j++)
            {
                float longitude = (float)j / numSegments * 2f * MathF.PI;
                float longitude2 = (float)(j + 1) / numSegments * 2f * MathF.PI;

                Vector3 point1 = new Vector3(
                    MathF.Sin(angle1) * MathF.Cos(longitude),
                    MathF.Cos(angle1),
                    MathF.Sin(angle1) * MathF.Sin(longitude)) * 0.5f;

                Vector3 point2 = new Vector3(
                    MathF.Sin(angle1) * MathF.Cos(longitude2),
                    MathF.Cos(angle1),
                    MathF.Sin(angle1) * MathF.Sin(longitude2)) * 0.5f;

                Vector3 point3 = new Vector3(
                    MathF.Sin(angle2) * MathF.Cos(longitude),
                    MathF.Cos(angle2),
                    MathF.Sin(angle2) * MathF.Sin(longitude)) * 0.5f;

                Vector3 point4 = new Vector3(
                    MathF.Sin(angle2) * MathF.Cos(longitude2),
                    MathF.Cos(angle2),
                    MathF.Sin(angle2) * MathF.Sin(longitude2)) * 0.5f;

                batch.Line(Pos(point1), Pos(point2), lineColor, lineColor);
                batch.Line(Pos(point1), Pos(point3), lineColor, lineColor);

                // Connect top and bottom rings
                if (isTop)
                    batch.Line(Pos(point2), Pos(point4), lineColor, lineColor);
                else
                    batch.Line(Pos(point3), Pos(point4), lineColor, lineColor);
            }
        }
    }
}

public class SpotlightGizmo(float distance, float angle, Color color) : Gizmo
{
    public override void Render(PrimitiveBatch batch, Matrix4x4 m)
    {
        Matrix = m;

        // Calculate the cone vertices
        Vector3 coneBaseLeft = Vector3.Transform(Vector3.Forward * distance, Matrix4x4.CreateRotationY(-(angle / 2)));
        Vector3 coneBaseRight = Vector3.Transform(Vector3.Forward * distance, Matrix4x4.CreateRotationY(angle / 2));
        Vector3 coneBaseTop = Vector3.Transform(Vector3.Forward * distance, Matrix4x4.CreateRotationX(-(angle / 2)));
        Vector3 coneBaseBottom = Vector3.Transform(Vector3.Forward * distance, Matrix4x4.CreateRotationX(angle / 2));
        float coneBaseRadius = MathF.Tan(angle / 2) * distance;
        float coneBaseDistance = MathF.Sqrt(coneBaseRadius * coneBaseRadius + distance * distance);

        // Draw cone lines
        batch.Line(Pos(Vector3.Zero), Pos(coneBaseLeft), color, color);
        batch.Line(Pos(Vector3.Zero), Pos(coneBaseRight), color, color);
        batch.Line(Pos(Vector3.Zero), Pos(coneBaseTop), color, color);
        batch.Line(Pos(Vector3.Zero), Pos(coneBaseBottom), color, color);

        // Use 3 Circle3D gizmo's
        m = Matrix4x4.CreateTranslation(Vector3.Forward * coneBaseDistance) * m;
        m = Matrix4x4.CreateScale(coneBaseRadius) * m;
        new CircleGizmo(color).Render(batch, m);
    }
}