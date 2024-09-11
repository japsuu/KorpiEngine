using KorpiEngine.Entities;
using KorpiEngine.Rendering;
using KorpiEngine.Tools.Gizmos;
using KorpiEngine.UI;
using KorpiEngine.Utils;

namespace KorpiEngine.SceneManagement;

/// <summary>
/// Manages all entities, components, and systems in a scene.
/// </summary>
internal sealed class EntityScene
{
    private bool _isBeingDestroyed;
    private bool _isIteratingEntities;
    private readonly Queue<Entity> _entitiesAwaitingRegistration = [];
    private readonly List<Entity> _entities = [];
    private readonly List<EntityComponent> _components = [];
    private readonly Dictionary<ulong, SceneSystem> _sceneSystems = [];
    private readonly EntitySceneRenderer _renderer = new();
    
    internal IReadOnlyList<EntityComponent> Components => _components;


    #region Entity/Component/System registration/deregistration

    internal void RegisterEntity(Entity entity)
    {
        if (_isBeingDestroyed)
            return;
        
        if (_isIteratingEntities)
        {
            _entitiesAwaitingRegistration.Enqueue(entity);
            return;
        }

        if (entity.ComponentCount > 0)
        {
            foreach (EntityComponent component in entity.Components)
                RegisterComponent(component);
        }
        
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
    
    
    private void DestroyAllSceneSystems()
    {
        foreach (SceneSystem system in _sceneSystems.Values)
            system.OnUnregister();
        
        _sceneSystems.Clear();
    }
    
    
    private void DestroyAllEntities()
    {
        while (_entitiesAwaitingRegistration.TryDequeue(out Entity? e))
            e.ReleaseImmediate();
        
        foreach (Entity entity in _entities)
        {
            if (entity.IsRootEntity)
                entity.ReleaseImmediate();
        }
        
        _entities.Clear();
        _components.Clear();
    }

    #endregion


    #region Update/Render/Destroy methods

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


    internal void Destroy()
    {
        _isBeingDestroyed = true;
        
        DestroyAllSceneSystems();
        DestroyAllEntities();
    }

    #endregion
    
    
    internal void InvokePreRender()
    {
        foreach (EntityComponent comp in Components)
            if (comp.EnabledInHierarchy)
                comp.PreRender();
    }
    
    
    internal void InvokePostRender()
    {
        foreach (EntityComponent comp in Components)
            if (comp.EnabledInHierarchy)
                comp.PostRender();
    }
    
    
    internal void InvokeRenderLighting()
    {
        foreach (EntityComponent comp in Components)
            if (comp is { EnabledInHierarchy: true, RenderOrder: ComponentRenderOrder.LightingPass })
                comp.RenderObject();
    }
    
    
    internal void InvokeRenderGeometry()
    {
        foreach (EntityComponent comp in Components)
            if (comp is { EnabledInHierarchy: true, RenderOrder: ComponentRenderOrder.GeometryPass })
                comp.RenderObject();
    }
    
    
    internal void InvokeRenderGeometryDepth()
    {
        foreach (EntityComponent comp in Components)
            if (comp is { EnabledInHierarchy: true, RenderOrder: ComponentRenderOrder.GeometryPass })
                comp.RenderObjectDepth();
    }
    
    
    internal void InvokeDrawGizmos()
    {
        Gizmos.AllowCreation = true;
        foreach (EntityComponent comp in Components)
        {
            comp.DrawGizmos();
            Gizmos.ResetColor();
        }
        Gizmos.AllowCreation = false;
    }
    
    
    internal void InvokeDrawDepthGizmos()
    {
        Gizmos.AllowCreation = true;
        foreach (EntityComponent comp in Components)
        {
            comp.DrawDepthGizmos();
            Gizmos.ResetColor();
        }
        Gizmos.AllowCreation = false;
    }


    internal T? FindObjectOfType<T>() where T : EntityComponent
    {
        foreach (EntityComponent component in _components)
            if (component is T t)
                return t;
        
        return default;
    }


    private void InvokeDrawGUI()
    {
        GUI.AllowDraw = true;
        foreach (EntityComponent comp in Components)
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
            if (e.EnabledInHierarchy)
                e.EnsureComponentInitialization();
        _isIteratingEntities = false;
    }


    private void UpdateEntities(EntityUpdateStage stage)
    {
        _isIteratingEntities = true;
        foreach (Entity entity in _entities)
        {
            // The entity may have been released during the update loop.
            if (entity.IsReleased)
                return;

            if (!entity.Enabled)
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
}