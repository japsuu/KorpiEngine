using System.ComponentModel;
using KorpiEngine.Core.Scripting.Components;

namespace KorpiEngine.Core.Scripting;

/// <summary>
/// Base class for all components.
/// Components are used to add functionality to <see cref="Scripting.Entity"/>s.
/// <br/><br/>
/// It's best to think of Components as a thin wrapper around an <see cref="Entity"/> they perform operations on.
/// Components are useless by themselves, as they must be internally bound to an entity.<br/><br/>
/// 
/// When a component is being constructed, it is not yet attached to an entity, or the entity might not even be initialized yet.<br/>
/// This is also the reason why you should not create components directly or use the constructor, but instead use the <see cref="Entity.AddComponent{T}"/> method.
/// </summary>
public abstract class Component
{
    /// <summary>
    /// The entity this component is attached to.
    /// </summary>
    public Entity Entity { get; private set; } = null!;

    /// <summary>
    /// The transform of the entity this component is attached to.
    /// </summary>
    public Transform Transform => Entity.Transform;

    /// <summary>
    /// The native variant of this component, used for <see cref="Entity.GetComponent{T}"/> and similar methods.
    /// </summary>
    internal abstract Type NativeComponentType { get; }
    
    
    [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
    protected Component() { }


    /// <summary>
    /// Internal method to initialize the component.
    /// </summary>
    /// <param name="entity">The entity this component is attached to.</param>
    internal virtual void Bind(Entity entity)
    {
        Entity = entity;
    }


    /// <summary>
    /// Instantiates a new entity with the given component and name, and returns the component.
    /// </summary>
    /// <param name="name">The name of the entity.</param>
    /// <typeparam name="T">The type of the component to add.</typeparam>
    /// <returns>The added component.</returns>
    public T Instantiate<T>(string name) where T : Behaviour, new()
    {
        return Entity.Instantiate<T>(name);
    }
}