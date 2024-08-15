using KorpiEngine.Core.API;
using KorpiEngine.Core.EntityModel.IDs;
using KorpiEngine.Core.EntityModel.Systems;
using KorpiEngine.Core.Rendering.Cameras;

namespace KorpiEngine.Core.EntityModel;

/// <summary>
/// Manages all entities, components, and systems in a scene.
/// </summary>
internal sealed class EntityScene
{
    private bool _isBeingDestroyed;
    private readonly List<Entity> _entities = [];
    private readonly List<EntityComponent> _components = [];
    private readonly Dictionary<SceneSystemID, SceneSystem> _sceneSystems = [];
    private readonly EntitySceneRenderer _renderer = new();
    
    internal IReadOnlyList<EntityComponent> Components => _components;


    #region Entity/Component/System registration

    internal void RegisterEntity(Entity entity)
    {
        if (_isBeingDestroyed)
            return;
        
        if (entity.ComponentCount > 0)
            throw new InvalidOperationException($"Entity {entity} has components before being registered. This is not allowed.");
        
        if (entity.SystemCount > 0)
            throw new InvalidOperationException($"Entity {entity} has systems before being registered. This is not allowed.");
        
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
        SceneSystemID id = SceneSystemID.Generate<T>();
        
        if (!_sceneSystems.TryAdd(id, system))
            throw new InvalidOperationException($"Scene system {id} already exists. Scene systems are singletons by default.");
        
        system.OnRegister(_components);
    }
    
    
    internal void UnregisterSceneSystem<T>() where T : SceneSystem
    {
        if (_isBeingDestroyed)
            return;

        SceneSystemID id = SceneSystemID.Generate<T>();
        
        if (!_sceneSystems.Remove(id, out SceneSystem? system))
            throw new InvalidOperationException($"Scene system {id} does not exist.");
        
        system.OnUnregister();
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
    }


    internal void Destroy()
    {
        _isBeingDestroyed = true;
        
        // Destroy all scene systems.
        foreach (SceneSystem system in _sceneSystems.Values)
            system.OnUnregister();
        
        // Destroy all entities (includes their systems and components).
        foreach (Entity entity in _entities)
            entity.DestroyImmediate();
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
            if (comp.EnabledInHierarchy)
                if (comp.RenderOrder == ComponentRenderOrder.LightingPass)
                    comp.RenderObject();
    }
    
    
    internal void InvokeRenderGeometry()
    {
        foreach (EntityComponent comp in Components)
            if (comp.EnabledInHierarchy)
                if (comp.RenderOrder == ComponentRenderOrder.GeometryPass)
                    comp.RenderObject();
    }
    
    
    internal void InvokeRenderGeometryDepth()
    {
        foreach (EntityComponent comp in Components)
            if (comp.EnabledInHierarchy)
                if (comp.RenderOrder == ComponentRenderOrder.GeometryPass)
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


    private void EnsureEntityInitialization()
    {
        foreach (Entity e in _entities)
            if (e.EnabledInHierarchy)
                e.EnsureComponentInitialization();
    }


    private void UpdateEntities(EntityUpdateStage stage)
    {
        foreach (Entity entity in _entities)
        {
            if (entity.IsDestroyed)
                throw new InvalidOperationException($"Entity {entity.InstanceID} has been destroyed.");

            if (!entity.Enabled)
                return;
            
            // Child entities are updated recursively by their parents.
            if (!entity.IsRootEntity)
                continue;
            
            entity.Update(stage);
        }
    }


    private void UpdateSceneSystems(EntityUpdateStage stage)
    {
        foreach (SceneSystem system in _sceneSystems.Values)
            system.Update(stage);
    }
}