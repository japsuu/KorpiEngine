namespace KorpiEngine.Core.EntityModel.Coroutines;

internal enum CoroutineUpdateStage
{
    /// <summary>
    /// Called from Update loop.
    /// </summary>
    Update,
    
    /// <summary>
    /// Called at the end of the frame after the engine has rendered every Camera and GUI,
    /// just before displaying the frame on screen.
    /// </summary>
    EndOfFrame,
    
    /// <summary>
    /// Called from FixedUpdate loop.
    /// </summary>
    FixedUpdate
}