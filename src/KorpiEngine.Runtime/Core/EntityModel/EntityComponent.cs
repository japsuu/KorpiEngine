using KorpiEngine.Core.EntityModel.IDs;

namespace KorpiEngine.Core.EntityModel;

public abstract class EntityComponent
{
    /// <summary>
    /// The unique identifier of this component.
    /// </summary>
    public readonly ComponentID ComponentID;
    
    /// <summary>
    /// The ID of the entity that owns this component.
    /// </summary>
    public EntityID EntityID { get; internal set; }
    
    /// <summary>
    /// The name of this component.
    /// </summary>
    public string Name;


    internal EntityComponent()
    {
        ComponentID = ComponentID.Generate();
        Name = $"Component {ComponentID}";
    }
    

    public virtual void OnRegister() { }
    public virtual void OnUnregister() { }
}