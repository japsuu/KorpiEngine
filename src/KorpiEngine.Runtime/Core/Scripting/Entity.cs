using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using KorpiEngine.Core.API;
using KorpiEngine.Core.ECS;
using KorpiEngine.Core.Rendering.Cameras;
using KorpiEngine.Core.SceneManagement;
using KorpiEngine.Core.Scripting.Components;

namespace KorpiEngine.Core.Scripting;

/// <summary>
/// High-level wrapper around an <see cref="EntityRef"/>.
/// </summary>
public sealed class Entity : EngineObject
{
    internal static event Action<Entity>? Constructed;

    private bool _enabled = true;

    /// <summary>
    /// The scene this entity is a part of.
    /// </summary>
    public readonly Scene Scene;

    /// <summary>
    /// Reference to the underlying entity.
    /// </summary>
    public readonly EntityReference EntityRef;

    public readonly Transform Transform = new();

    public Entity? Parent => Transform.Parent?.Entity;

    /// <summary> Gets whether this entity is enabled explicitly </summary>
    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (_enabled != value)
                SetEnabled(value);
        }
    }

    /// <summary> Gets whether this entity is enabled in the hierarchy, so if its parent is disabled this will return false </summary>
    public bool EnabledInHierarchy { get; internal set; } = true;
    
    /// <summary>Returns a matrix relative/local to the currently rendering camera, Will throw if used outside a rendering method</summary>
    public Matrix4x4 GlobalCamRelative
    {
        get
        {
            Matrix4x4 t = Transform.LocalToWorldMatrix;
            t.Translation -= Camera.RenderingCamera!.Entity.Transform.Position;
            return t;
        }
    }


    /// <summary>
    /// Creates a new Entity from an existing entity reference.
    /// </summary>
    internal static Entity Wrap(EntityReference entityRef, Scene scene) => new(entityRef.Entity.Get<NameComponent>().Name, entityRef, scene, false);


    /// <summary>
    /// Creates a new Entity without triggering the global Constructed event.
    /// </summary>
    internal static Entity CreateSilently() => Create(null, SceneManager.CurrentScene, false);


    internal static Entity Create(string? name, Scene scene, bool invoke = true)
    {
        UUID uuid = new();
        string nameString = string.IsNullOrWhiteSpace(name) ? "New Entity" : name;
        Arch.Core.Entity entity = scene.World.Create(new IdComponent(uuid), new NameComponent(nameString), new TransformComponent());
        return new Entity(nameString, entity.Reference(), scene, invoke);
    }


    private Entity(string name, EntityReference entityRef, Scene scene, bool invoke) : base(name)
    {
        EntityRef = entityRef;
        Scene = scene;
        Transform.Bind(this);

        if (invoke)
            Constructed?.Invoke(this);
    }


    /// <summary>
    /// Instantiates a new entity with the given component and name, and returns the component.
    /// </summary>
    /// <param name="name">The name of the entity.</param>
    /// <typeparam name="T">The type of the component to add.</typeparam>
    /// <returns>The added component.</returns>
    public T Instantiate<T>(string name) where T : Behaviour, new() => Scene.Instantiate<T>(name);


    #region Component Interface

    public T AddComponent<T>() where T : Component, new()
    {
        if (IsDestroyed)
            throw new KorpiException("Cannot add a component to a destroyed object.");

        if (!EntityRef.IsAlive())
            throw new KorpiException("The underlying entity has been destroyed.");

        T component = new();
        component.Bind(this); // Automatically attaches the component to this entity

        return component;
    }


    public T? GetComponent<T>() where T : class // NOTE: We cannot use a Component constraint here, as we need to check if the provided type is a component or a plain C# interface.
    {
#warning TODO: Better solution for getting components, that does not use reflection.
        //WARN: Might not work properly with components inheriting Behaviour, since Behaviour's NativeComponentType is BehaviourComponent.
        if (IsDestroyed)
            throw new KorpiException("Cannot get a component from a destroyed object.");

        if (!EntityRef.IsAlive())
            throw new KorpiException("The underlying entity has been destroyed.");

        if (typeof(T).IsSubclassOf(typeof(Component)))
        {
            // The provided type is a component, so get the native component type for it.
            // Create a new instance of the provided type using reflection, to basically call T.NativeComponentType.
            object instance = Activator.CreateInstance(typeof(T))!;

            Component componentInstance = (Component)instance;
            Type nativeComponentType =
                componentInstance.NativeComponentType; // We know 'instance' is a component, so cast it to one to access the NativeComponentType property.

            if (!HasNativeComponent(nativeComponentType))
                return null;

            componentInstance.Bind(this); // Attach the component to this entity.

            return (T)instance; // We know T is a component, so cast and return instance.
        }

        // The provided type is not a component, so it could be an interface.
        // Check all behaviours if they implement the provided type.
        List<Behaviour>? behaviours = EntityRef.Entity.Get<BehaviourComponent>().Behaviours;
        if (behaviours == null)
            return null;

        foreach (Behaviour behaviour in behaviours)
            if (behaviour is T t)
                return t;

        return null;
    }


    public IEnumerable<T> GetComponentsInChildren<T>(bool includeSelf = true, bool includeInactive = false) where T : class
    {
        if (includeInactive)
            throw new NotImplementedException("IncludeInactive is not yet implemented.");
        
        if (IsDestroyed)
            throw new KorpiException("Cannot get components from a destroyed object.");

        if (!EntityRef.IsAlive())
            throw new KorpiException("The underlying entity has been destroyed.");

        List<T> components = [];

        if (includeSelf)
        {
            T? component = GetComponent<T>();
            if (component != null)
                components.Add(component);
        }

        foreach (Transform child in Transform.Children)
            components.AddRange(child.Entity.GetComponentsInChildren<T>(true, includeInactive));

        return components;
    }


    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref T AddNativeComponent<T>() where T : INativeComponent, new()
    {
        if (IsDestroyed)
            throw new KorpiException("Cannot add a component to a destroyed object.");

        if (!EntityRef.IsAlive())
            throw new KorpiException("The underlying entity has been destroyed.");

        if (EntityRef.Entity.Has<T>())
            throw new KorpiException("Cannot add a component to an Entity that already has it.");

        EntityRef.Entity.Add<T>();

        return ref EntityRef.Entity.Get<T>();
    }


    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref T AddNativeComponent<T>(in T component) where T : INativeComponent, new()
    {
        if (IsDestroyed)
            throw new KorpiException("Cannot add a component to a destroyed object.");

        if (!EntityRef.IsAlive())
            throw new KorpiException("The underlying entity has been destroyed.");

        if (EntityRef.Entity.Has<T>())
            throw new KorpiException("Cannot add a component to an Entity that already has it.");

        EntityRef.Entity.Add(component);

        return ref EntityRef.Entity.Get<T>();
    }


    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool HasNativeComponent<T>() where T : INativeComponent
    {
        if (IsDestroyed)
            throw new KorpiException("Cannot check for a component in a destroyed object.");

        if (!EntityRef.IsAlive())
            throw new KorpiException("The underlying entity has been destroyed.");

        return EntityRef.Entity.Has<T>();
    }


    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool HasNativeComponent(Type type)
    {
        if (IsDestroyed)
            throw new KorpiException("Cannot check for a component in a destroyed object.");

        if (!EntityRef.IsAlive())
            throw new KorpiException("The underlying entity has been destroyed.");

        return EntityRef.Entity.Has(type);
    }


    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref T GetNativeComponent<T>() where T : INativeComponent
    {
        if (IsDestroyed)
            throw new KorpiException("Cannot get a component from a destroyed object.");

        if (!EntityRef.IsAlive())
            throw new KorpiException("The underlying entity has been destroyed.");

        return ref EntityRef.Entity.Get<T>();
    }


    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool TryGetNativeComponent<T>([NotNullWhen(true)] out T? component) where T : INativeComponent
    {
        if (IsDestroyed)
            throw new KorpiException("Cannot get a component from a destroyed object.");

        if (!EntityRef.IsAlive())
            throw new KorpiException("The underlying entity has been destroyed.");

        return EntityRef.Entity.TryGet(out component);
    }

    #endregion

    public void DontDestroyOnLoad() => throw new NotImplementedException();


    private void SetEnabled(bool state)
    {
        _enabled = state;
        Transform.HierarchyStateChanged();
    }

    internal bool IsParentEnabled() => Parent == null || Parent.EnabledInHierarchy;
}