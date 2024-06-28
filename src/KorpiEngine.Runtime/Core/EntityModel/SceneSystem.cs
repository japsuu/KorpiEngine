namespace KorpiEngine.Core.EntityModel;

/// <summary>
/// A system that influences all existing components of a given type in the world.
/// </summary>
public abstract class SceneSystem
{
    public abstract void TryRegisterComponent<T>(T c) where T : EntityComponent;
    public abstract void TryUnregisterComponent<T>(T c) where T : EntityComponent;


    public void OnRegister(IEnumerable<EntityComponent> existingComponents)
    {
        foreach (EntityComponent component in existingComponents)
            TryRegisterComponent(component);
        
        Initialize();
    }
    
    
    public void OnUnregister()
    {
        Deinitialize();
    }
    
    protected virtual void Initialize() { }
    protected virtual void Deinitialize() { }

    public abstract void Update(EntityUpdateStage stage);
}