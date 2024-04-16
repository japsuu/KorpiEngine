using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using KorpiEngine.Core.ECS;
using KorpiEngine.Core.SceneManagement;
using KorpiEngine.Core.Scripting.Components;

namespace KorpiEngine.Core.Scripting;

/// <summary>
/// High-level wrapper around an <see cref="EntityRef"/>.
/// </summary>
public sealed class Entity
{
    /// <summary>
    /// The scene this entity is a part of.
    /// </summary>
    public readonly Scene Scene;
    
    /// <summary>
    /// Reference to the underlying entity.
    /// </summary>
    public readonly EntityReference EntityRef;

    /// <summary>
    /// Whether the underlying entity has been destroyed.
    /// </summary>
    public bool IsDestroyed => !EntityRef.IsAlive();

    public readonly Transform Transform;


    public Entity(EntityReference entityRef, Scene scene)
    {
        EntityRef = entityRef;
        Scene = scene;
        Transform = new Transform();
        Transform.Bind(this);
    }


    /// <summary>
    /// Instantiates a new entity with the given component and name, and returns the component.
    /// </summary>
    /// <param name="name">The name of the entity.</param>
    /// <typeparam name="T">The type of the component to add.</typeparam>
    /// <returns>The added component.</returns>
    public T Instantiate<T>(string name) where T : Behaviour, new()
    {
        return Scene.Instantiate<T>(name);
    }
    
    
    public T AddComponent<T>() where T : Component, new()
    {
        if (IsDestroyed)
            throw new KorpiException("Cannot add a component to a destroyed Entity.");
        
        T component = new();
        component.Bind(this);     // Automatically attaches the component to this entity
        
        return component;
    }
    
    
    public T? GetComponent<T>() where T : class // NOTE: We cannot use a Component constraint here, as we need to check if the provided type is a component or a plain C# interface.
    {
        //TODO: Better solution for this, that does not use reflection.
        //WARN: Might not work properly with components inheriting Behaviour, since Behaviour's NativeComponentType is BehaviourComponent.
        if (IsDestroyed)
            throw new KorpiException("Cannot get a component from a destroyed Entity.");

        if (typeof(T).IsSubclassOf(typeof(Component)))
        {
            // The provided type is a component, so get the native component type for it.
            // Create a new instance of the provided type using reflection, to basically call T.NativeComponentType.
            object instance = Activator.CreateInstance(typeof(T))!;

            Component componentInstance = (Component)instance;
            Type nativeComponentType = componentInstance.NativeComponentType;   // We know 'instance' is a component, so cast it to one to access the NativeComponentType property.
            
            if (!HasNativeComponent(nativeComponentType))
                return null;
            
            componentInstance.Bind(this);   // Attach the component to this entity.

            return (T)instance; // We know T is a component, so cast and return instance.
        }

        // The provided type is not a component, so it could be an interface.
        // Check all behaviours if they implement the provided type.
        List<Behaviour>? behaviours = EntityRef.Entity.Get<BehaviourComponent>().Behaviours;
        if (behaviours == null)
            return null;
        
        foreach (Behaviour behaviour in behaviours)
        {
            if (behaviour is T t)
                return t;
        }

        return null;
    }


    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref T AddNativeComponent<T>() where T : INativeComponent, new()
    {
        if (IsDestroyed)
            throw new KorpiException("Cannot add a component to a destroyed Entity.");
        
        if (EntityRef.Entity.Has<T>())
            throw new KorpiException("Cannot add a component to an Entity that already has it.");

        EntityRef.Entity.Add<T>();
        
        return ref EntityRef.Entity.Get<T>();
    }


    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool HasNativeComponent<T>() where T : INativeComponent
    {
        if (IsDestroyed)
            throw new KorpiException("Cannot check for a component in a destroyed Entity.");

        return EntityRef.Entity.Has<T>();
    }


    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool HasNativeComponent(Type type)
    {
        if (IsDestroyed)
            throw new KorpiException("Cannot check for a component in a destroyed Entity.");

        return EntityRef.Entity.Has(type);
    }


    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref T GetNativeComponent<T>() where T : INativeComponent
    {
        if (IsDestroyed)
            throw new KorpiException("Cannot get a component from a destroyed Entity.");

        return ref EntityRef.Entity.Get<T>();
    }


    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool TryGetNativeComponent<T>([NotNullWhen(true)] out T? component) where T : INativeComponent
    {
        if (IsDestroyed)
            throw new KorpiException("Cannot get a component from a destroyed Entity.");

        return EntityRef.Entity.TryGet(out component);
    }
}