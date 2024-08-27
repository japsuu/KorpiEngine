using KorpiEngine.Core.API;
using Vector3 = System.Numerics.Vector3;

namespace KorpiEngine.Core.EntityModel.Components;

/// <summary>
/// Can draw gizmos to debug a mesh.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class MeshDebugGizmoDrawer : EntityComponent
{
    public bool DrawNormals { get; set; }
    public bool DrawTangents { get; set; }
    public bool DrawBounds { get; set; }
    public bool IgnoreDepth { get; set; }
    
    public float NormalLength { get; set; } = 0.1f;
    public float TangentLength { get; set; } = 0.1f;
    
    private MeshRenderer? _renderer;


    protected override void OnStart()
    {
        _renderer = Entity.GetComponent<MeshRenderer>();
    }
    
    
    protected override void OnDrawGizmos()
    {
        if (DrawBounds)
            DrawBoundsGizmos();
        
        if (!IgnoreDepth)
            return;
        
        Draw();
    }
    
    
    protected override void OnDrawDepthGizmos()
    {
        if (IgnoreDepth)
            return;

        Draw();
    }


    private void Draw()
    {
        if (!Enabled)
            return;
        
        if (_renderer == null)
            return;
        
        if (DrawNormals)
            DrawNormalsGizmos();
        
        if (DrawTangents)
            DrawTangentsGizmos();
    }


    private void DrawNormalsGizmos()
    {
        Vector3[]? vertices = GetMeshVertexPositions();
        Vector3[]? normals = GetMeshNormals();
        
        DrawLines(vertices, normals, NormalLength);
    }
    
    
    private void DrawTangentsGizmos()
    {
        Vector3[]? vertices = GetMeshVertexPositions();
        Vector3[]? tangents = GetMeshTangents();
        
        DrawLines(vertices, tangents, TangentLength);
    }
    
    
    private void DrawBoundsGizmos()
    {
        if (_renderer == null || !_renderer.Mesh.IsAvailable)
            return;
        
        Gizmos.DrawCube(Transform.Position + _renderer.Mesh.Res!.Bounds.Center, _renderer.Mesh.Res!.Bounds.Size);
    }


    private Vector3[]? GetMeshVertexPositions()
    {
        if (_renderer == null || !_renderer.Mesh.IsAvailable)
            return null;
        
        return _renderer.Mesh.Res!.GetVertexPositions();
    }
    
    
    private Vector3[]? GetMeshNormals()
    {
        if (_renderer == null || !_renderer.Mesh.IsAvailable)
            return null;
        
        return _renderer.Mesh.Res!.GetVertexNormals();
    }
    
    
    private Vector3[]? GetMeshTangents()
    {
        if (_renderer == null || !_renderer.Mesh.IsAvailable)
            return null;
        
        return _renderer.Mesh.Res!.GetVertexTangents();
    }


    private void DrawLines(Vector3[]? positions, Vector3[]? directions, float length)
    {
        if (positions == null || directions == null)
            return;
        
        Matrix4x4 localToWorldMatrix = Transform.LocalToWorldMatrix;

        for (int i = 0; i < positions.Length; i++)
        {
            Vector3 position = positions[i];
            Vector3 direction = directions[i];

            float dirLength = direction.Length();
            if (dirLength < 0.001f || dirLength > 1f)
            {
                Application.Logger.Warn($"Normal or tangent direction is invalid ({dirLength}), skip drawing line.");
                continue;
            }
            
            // Transform the position and direction vectors by the local-to-world matrix
            position = KorpiEngine.Core.API.Vector3.Transform(position, localToWorldMatrix);
            direction = KorpiEngine.Core.API.Vector3.TransformNormal(direction, localToWorldMatrix);
            
            // Decide the line color based on the normal, ensure there are no black lines
            float r = Math.Abs(direction.X);
            float g = Math.Abs(direction.Y);
            float b = Math.Abs(direction.Z);
            Gizmos.Color = new Color(r, g, b, 1f);
            
            Gizmos.DrawArrow(position, position + direction * length);
        }
    }
}