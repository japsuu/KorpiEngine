namespace KorpiEngine.EntityModel;

/// <summary>
/// Determines the stage at which a system is updated.
/// </summary>
public enum EntityUpdateStage
{
    PreUpdate,
    Update,
    PostUpdate,
    PreFixedUpdate,
    FixedUpdate,
    PostFixedUpdate,
    PostRender,
    /*PreRender,
    Render,
    PostRender,
    RenderDepth,
    DrawGizmos*/
}