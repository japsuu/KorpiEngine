namespace KorpiEngine.Core.EntityModel;

public abstract class EntityComponent
{
    public readonly Guid Guid;


    public EntityComponent()
    {
        Guid = Guid.NewGuid();
    }
    

    public virtual void OnRegister() { }
    public virtual void OnUnregister() { }
}


public class SpatialEntityComponent : EntityComponent
{
    
}