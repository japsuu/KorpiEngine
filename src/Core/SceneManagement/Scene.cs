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
    internal readonly EntityScene EntityScene = new();


    #region Creation and destruction

    public void Destroy()
    {
        OnUnload();
        
        EntityScene.Destroy();
    }

    #endregion


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
        Material mat = new Material(Shader.Find("Assets/Defaults/Standard.kshader"), "standard material");
        
        c.Mesh = Mesh.CreatePrimitive(primitiveType);
        c.Material = mat;
        
        mat.SetFloat("_EmissionIntensity", 0f);
        mat.SetColor("_EmissiveColor", ColorHDR.Black);
        
        return e;
    }
    
    
    public T? FindObjectOfType<T>() where T : EntityComponent
    {
        return EntityScene.FindObjectOfType<T>();
    }

    #endregion


    #region Protected API

    protected void RegisterSceneSystem<T>() where T : SceneSystem, new()
    {
        EntityScene.RegisterSceneSystem<T>();
    }
    
    
    protected void UnregisterSceneSystem<T>() where T : SceneSystem
    {
        EntityScene.UnregisterSceneSystem<T>();
    }

    #endregion


    #region Protected overridable methods

    protected virtual Camera CreateSceneCamera()
    {
        Entity cameraEntity = CreateEntity("Scene Camera");
        Camera camera = cameraEntity.AddComponent<Camera>();
        
        return camera;
    }

    protected virtual void CreateLights()
    {
        Entity dlEntity = CreateEntity("Directional Light");
        DirectionalLight dlComp = dlEntity.AddComponent<DirectionalLight>();
        dlComp.Transform.LocalEulerAngles = new Vector3(130, 45, 0);
        
        Entity alEntity = CreateEntity("Ambient Light");
        AmbientLight alComp = alEntity.AddComponent<AmbientLight>();
        alComp.SkyIntensity = 0.4f;
        alComp.GroundIntensity = 0.1f;
    }
    
    
    /// <summary>
    /// Called when the scene is loaded.
    /// </summary>
    protected virtual void OnLoad() { }
    
    /// <summary>
    /// Called when the scene is unloaded.
    /// </summary>
    protected virtual void OnUnload() { }

    #endregion


    #region Internal calls

    internal void InternalLoad()
    {
        CreateLights();
        CreateSceneCamera();
        
        OnLoad();
    }
    
    
    internal void InternalUpdate()
    {
        EntityScene.Update();
    }
    
    
    internal void InternalFixedUpdate()
    {
        EntityScene.FixedUpdate();
    }
    
    
    internal void InternalRender()
    {
        EntityScene.Render();
    }

    #endregion
}