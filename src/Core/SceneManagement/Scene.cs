using KorpiEngine.AssetManagement;
using KorpiEngine.Entities;
using KorpiEngine.Mathematics;
using KorpiEngine.Rendering;
using KorpiEngine.Tools.Gizmos;
using KorpiEngine.UI;
using KorpiEngine.Utils;
using Entity = KorpiEngine.Entities.Entity;

namespace KorpiEngine.SceneManagement;

/// <summary>
/// An in-game scene, that can be loaded and unloaded and receives updates.
/// Can create <see cref="Entity"/>s and register systems to process them.
/// </summary>
public abstract class Scene
{
    private readonly Queue<Entity> _entitiesAwaitingRegistration = [];
    private readonly List<Entity> _entities = [];
    private readonly List<EntityComponent> _components = [];
    private readonly Dictionary<ulong, SceneSystem> _sceneSystems = [];
    private readonly EntitySceneRenderer _renderer = new();
    private bool _isBeingDestroyed;
    private bool _isIteratingEntities;


#region Public API
    
    /// <summary>
    /// Creates a new entity with the given name and adds it to the scene.
    /// </summary>
    /// <param name="name">The name of the entity.</param>
    /// <returns>A new entity.</returns>
    public Entity CreateEntity(string name)
    {
        Entity e = new(this, name);
        return e;
    }

    
    /// <summary>
    /// Creates a new entity with the given primitive mesh and adds it to the scene.
    /// </summary>
    /// <param name="primitiveType">The type of primitive to create.</param>
    /// <param name="name">The name of the entity.</param>
    /// <returns>A new entity with a primitive mesh.</returns>
    public Entity CreatePrimitive(PrimitiveType primitiveType, string name)
    {
        Entity e = CreateEntity(name);
        MeshRenderer c = e.AddComponent<MeshRenderer>();
        Material mat = new(Asset.Load<Shader>("Assets/Defaults/Standard.kshader"), "standard material");
        c.Mesh = Mesh.CreatePrimitive(primitiveType);
        c.Material = mat;
        mat.SetFloat("_EmissionIntensity", 0f);
        mat.SetColor("_EmissiveColor", ColorHDR.Black);
        
        return e;
    }
    
    
    public T? FindObjectOfType<T>() where T : EntityComponent
    {
        foreach (EntityComponent component in _components)
            if (component is T t)
                return t;
        
        return default;
    }

#endregion


#region Internal Lifecycle API

    internal void Load() => OnLoad();
    
    
    internal void Update()
    {
        if (_isBeingDestroyed)
            return;
        
        EnsureEntityInitialization();
        
        UpdateEntities(EntityUpdateStage.PreUpdate);
        UpdateSceneSystems(EntityUpdateStage.PreUpdate);
        
        UpdateEntities(EntityUpdateStage.Update);
        UpdateSceneSystems(EntityUpdateStage.Update);
        
        UpdateEntities(EntityUpdateStage.PostUpdate);
        UpdateSceneSystems(EntityUpdateStage.PostUpdate);
    }
    
    
    internal void FixedUpdate()
    {
        if (_isBeingDestroyed)
            return;
        
        UpdateEntities(EntityUpdateStage.FixedUpdate);
        UpdateSceneSystems(EntityUpdateStage.FixedUpdate);
    }
    
    
    internal void Render()
    {
        if (_isBeingDestroyed)
            return;
        
        _renderer.Render();
        
        InvokeDrawGUI();
        
        UpdateEntities(EntityUpdateStage.PostRender);
        UpdateSceneSystems(EntityUpdateStage.PostRender);
    }
    
    
    internal void Unload()
    {
        _isBeingDestroyed = true;
        OnUnload();
        
        DestroyAllSceneSystems();
        DestroyAllEntities();
    }


#region Entity and component management

    internal void RegisterEntity(Entity entity)
    {
        if (_isBeingDestroyed)
            return;
        
        if (_isIteratingEntities)
        {
            _entitiesAwaitingRegistration.Enqueue(entity);
            return;
        }

        foreach (EntityComponent component in entity.Components)
            RegisterComponent(component);
        
        _entities.Add(entity);
    }


    internal void UnregisterEntity(Entity entity)
    {
        if (_isBeingDestroyed)
            return;

        _entities.Remove(entity);
    }

    
    internal void RegisterComponent(EntityComponent component)
    {
        if (_isBeingDestroyed)
            return;

        _components.Add(component);
        
        foreach (SceneSystem system in _sceneSystems.Values)
            system.TryRegisterComponent(component);
        
        _renderer.TryRegisterComponent(component);
    }


    internal void UnregisterComponent(EntityComponent component)
    {
        if (_isBeingDestroyed)
            return;

        if (!_components.Remove(component))
            throw new InvalidOperationException($"Component {component} is not registered.");
        
        foreach (SceneSystem system in _sceneSystems.Values)
            system.TryUnregisterComponent(component);
        
        _renderer.TryUnregisterComponent(component);
    }
    
    
    internal void RegisterSceneSystem<T>() where T : SceneSystem, new()
    {
        if (_isBeingDestroyed)
            return;

        SceneSystem system = new T();
        ulong id = TypeID.Get<T>();
        
        if (!_sceneSystems.TryAdd(id, system))
            throw new InvalidOperationException($"Scene system {id} already exists. Scene systems are singletons by default.");
        
        system.OnRegister(_components);
    }
    
    
    internal void UnregisterSceneSystem<T>() where T : SceneSystem
    {
        if (_isBeingDestroyed)
            return;

        ulong id = TypeID.Get<T>();
        
        if (!_sceneSystems.Remove(id, out SceneSystem? system))
            throw new InvalidOperationException($"Scene system {id} does not exist.");
        
        system.OnUnregister();
    }

#endregion

#endregion


#region Invoke render methods

    internal void InvokePreRender()
    {
        foreach (EntityComponent comp in _components)
            if (comp.EnabledInHierarchy)
                comp.PreRender();
    }
    
    
    internal void InvokePostRender()
    {
        foreach (EntityComponent comp in _components)
            if (comp.EnabledInHierarchy)
                comp.PostRender();
    }
    
    
    internal void InvokeRenderLighting()
    {
        foreach (EntityComponent comp in _components)
            if (comp is { EnabledInHierarchy: true, RenderOrder: ComponentRenderOrder.LightingPass })
                comp.RenderObject();
    }
    
    
    internal void InvokeRenderGeometry()
    {
        foreach (EntityComponent comp in _components)
            if (comp is { EnabledInHierarchy: true, RenderOrder: ComponentRenderOrder.GeometryPass })
                comp.RenderObject();
    }
    
    
    internal void InvokeRenderGeometryDepth()
    {
        foreach (EntityComponent comp in _components)
            if (comp is { EnabledInHierarchy: true, RenderOrder: ComponentRenderOrder.GeometryPass })
                comp.RenderObjectDepth();
    }
    
    
    internal void InvokeDrawGizmos()
    {
        Gizmos.AllowCreation = true;
        foreach (EntityComponent comp in _components)
        {
            comp.DrawGizmos();
            Gizmos.ResetColor();
        }
        Gizmos.AllowCreation = false;
    }
    
    
    internal void InvokeDrawDepthGizmos()
    {
        Gizmos.AllowCreation = true;
        foreach (EntityComponent comp in _components)
        {
            comp.DrawDepthGizmos();
            Gizmos.ResetColor();
        }
        Gizmos.AllowCreation = false;
    }

#endregion


#region Protected overridable methods
    
    /// <summary>
    /// Called when the scene is loaded.
    /// </summary>
    protected virtual void OnLoad() { }
    
    /// <summary>
    /// Called when the scene is unloaded.
    /// </summary>
    protected virtual void OnUnload() { }

#endregion


#region Utility

    private void InvokeDrawGUI()
    {
        GUI.AllowDraw = true;
        foreach (EntityComponent comp in _components)
        {
            if (!comp.EnabledInHierarchy)
                continue;
            
            comp.DrawGUI();
                
            if (GUI.IsDrawing)
                GUI.End();
        }
        GUI.AllowDraw = false;
    }


    private void EnsureEntityInitialization()
    {
        while (_entitiesAwaitingRegistration.Count > 0)
            RegisterEntity(_entitiesAwaitingRegistration.Dequeue());
        
        _isIteratingEntities = true;
        foreach (Entity e in _entities)
            if (e.IsEnabledInHierarchy)
                e.EnsureComponentInitialization();
        _isIteratingEntities = false;
    }


    private void UpdateEntities(EntityUpdateStage stage)
    {
        _isIteratingEntities = true;
        foreach (Entity entity in _entities)
        {
            // The entity may have been destroyed during the update loop.
            if (entity.IsDestroyed)
                return;

            if (!entity.IsEnabled)
                return;
            
            // Child entities are updated recursively by their parents.
            if (!entity.IsRootEntity)
                continue;
            
            entity.Update(stage);
        }
        _isIteratingEntities = false;
    }


    private void UpdateSceneSystems(EntityUpdateStage stage)
    {
        foreach (SceneSystem system in _sceneSystems.Values)
            system.Update(stage);
    }
    
    
    private void DestroyAllSceneSystems()
    {
        foreach (SceneSystem system in _sceneSystems.Values)
            system.OnUnregister();
        
        _sceneSystems.Clear();
    }
    
    
    private void DestroyAllEntities()
    {
        while (_entitiesAwaitingRegistration.TryDequeue(out Entity? e))
        {
            if (e.IsRootEntity)
                e.DestroyImmediate();
        }
        
        foreach (Entity entity in _entities)
        {
            if (entity.IsRootEntity)
                entity.DestroyImmediate();
        }
        
        _entitiesAwaitingRegistration.Clear();
        _entities.Clear();
        _components.Clear();
    }

#endregion
}