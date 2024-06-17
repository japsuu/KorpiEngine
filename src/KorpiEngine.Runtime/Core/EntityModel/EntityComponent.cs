namespace KorpiEngine.Core.EntityModel;

public abstract class EntityComponent
{
    public Guid Guid { get; }
    public string Name { get; set; }    // Mostly for debugging purposes.

    public virtual void Load() { }
    public virtual void Unload() { }
    public virtual void Initialize() { }
    public virtual void Deinitialize() { }
}


public class SpatialEntityComponent : EntityComponent
{
    
}