using KorpiEngine.Core.API;
using Vector3 = System.Numerics.Vector3;

namespace KorpiEngine.Core.EntityModel.Components;

/// <summary>
/// Can draw gizmos to debug a mesh.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class MeshDebugGizmoDrawer : EntityComponent
{
    public bool DrawNormals = false;
    public bool DrawTangents = false;
    public bool DrawBounds = false;
    public bool IgnoreDepth = false;
    
    public float NormalLength = 0.1f;
    public float TangentLength = 0.1f;
    
    private MeshRenderer? _renderer;


    protected override void OnStart()
    {
        _renderer = Entity.GetComponent<MeshRenderer>();
    }
    
    
    protected override void OnDrawGizmos()
    {
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
        
        if (DrawBounds)
            DrawBoundsGizmos();
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

        for (int i = 0; i < positions.Length; i++)
        {
            Vector3 position = positions[i] + (Vector3)Transform.Position;
            Vector3 direction = directions[i];
            
            // Decide the line color based on the normal, ensure there are no black lines
            float r = Math.Max(0.1f, Math.Abs(direction.X));
            float g = Math.Max(0.1f, Math.Abs(direction.Y));
            float b = Math.Max(0.1f, Math.Abs(direction.Z));
            Gizmos.Color = new Color(r, g, b, 1f);
            
            Gizmos.DrawArrow(position, position + direction * length);
        }
    }
}