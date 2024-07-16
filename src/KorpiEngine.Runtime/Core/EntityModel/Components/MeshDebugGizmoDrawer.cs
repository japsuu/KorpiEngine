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
    
    private MeshRenderer? _renderer;


    protected override void OnStart()
    {
        _renderer = Entity.GetComponent<MeshRenderer>();
    }
    
    
    protected override void OnDrawGizmos()
    {
        if (_renderer == null)
            return;
        
        if (DrawNormals)
            DrawNormalsGizmo();
        
        if (DrawTangents)
            DrawTangentsGizmo();
        
        if (DrawBounds)
            DrawBoundsGizmo();
    }
}