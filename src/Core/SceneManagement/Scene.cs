using KorpiEngine.Entities;
using KorpiEngine.Mathematics;
using KorpiEngine.Rendering;
using Entity = KorpiEngine.Entities.Entity;

namespace KorpiEngine.SceneManagement;

/// <summary>
/// An in-game scene, that can be loaded and unloaded and receives updates.
/// Can create <see cref="Entity"/>s and register systems to process them.
/// </summary>
public abstract class Scene
{
    private readonly EntityScene _entityScene;


    protected Scene()
    {
        _entityScene = new EntityScene();
    }


#region Public API
    
    public Entity CreateEntity(string name)
    {
        Entity e = new(this, name);
        return e;
    }

    
    public Entity CreatePrimitive(PrimitiveType primitiveType, string name)
    {
        Entity e = CreateEntity(name);
        MeshRenderer c = e.AddComponent<MeshRenderer>();
        Material mat = new(Shader.Find("Assets/Defaults/Standard.kshader"), "standard material");
        
        c.Mesh = Mesh.CreatePrimitive(primitiveType);
        c.Material = mat;
        
        mat.SetFloat("_EmissionIntensity", 0f);
        mat.SetColor("_EmissiveColor", ColorHDR.Black);
        
        return e;
    }
    
    
    public T? FindObjectOfType<T>() where T : EntityComponent
    {
        return _entityScene.FindObjectOfType<T>();
    }

#endregion


#region Protected API

    protected void RegisterSceneSystem<T>() where T : SceneSystem, new() => _entityScene.RegisterSceneSystem<T>();
    protected void UnregisterSceneSystem<T>() where T : SceneSystem => _entityScene.UnregisterSceneSystem<T>();

#endregion


#region Internal API

    // Lifecycle methods
    internal void InternalLoad() => OnLoad();
    internal void InternalUpdate() => _entityScene.Update();
    internal void InternalFixedUpdate() => _entityScene.FixedUpdate();
    internal void InternalRender() => _entityScene.Render();
    internal void InternalUnload()
    {
        OnUnload();
        
        _entityScene.Destroy();
    }
    
    // Entity and component management
    internal void RegisterEntity(Entity entity) => _entityScene.RegisterEntity(entity);
    internal void UnregisterEntity(Entity entity) => _entityScene.UnregisterEntity(entity);
    internal void RegisterComponent(EntityComponent component) => _entityScene.RegisterComponent(component);
    internal void UnregisterComponent(EntityComponent component) => _entityScene.UnregisterComponent(component);

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
}