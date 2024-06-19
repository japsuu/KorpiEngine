namespace KorpiEngine.Core.EntityModel;

/// <summary>
/// A system that influences all existing components of a given type in the world.
/// </summary>
public abstract class WorldSystem
{
    /// <summary>
    /// The component types that this system affects.
    /// </summary>
    protected abstract Type[] AffectedTypes { get; }


    public void OnRegister()
    {
        Initialize();
    }
    
    
    public void OnUnregister()
    {
        Deinitialize();
    }
    
    protected virtual void Initialize() { }
    protected virtual void Deinitialize() { }
}