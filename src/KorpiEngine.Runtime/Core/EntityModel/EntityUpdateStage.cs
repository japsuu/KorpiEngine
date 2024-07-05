namespace KorpiEngine.Core.EntityModel;

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
    /*PreRender,
    Render,
    PostRender,
    RenderDepth,
    DrawGizmos*/
}