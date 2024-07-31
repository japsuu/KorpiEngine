using KorpiEngine.Core.API;
using KorpiEngine.Core.API.Rendering;
using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.API.Rendering.Shaders;
using KorpiEngine.Core.API.Rendering.Textures;
using KorpiEngine.Core.EntityModel;
using KorpiEngine.Core.EntityModel.Components;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Rendering.Cameras;
using KorpiEngine.Core.Rendering.Lighting;
using Entity = KorpiEngine.Core.EntityModel.Entity;

namespace KorpiEngine.Core.SceneManagement;

/// <summary>
/// An in-game scene, that can be loaded and unloaded and receives updates.
/// Can create <see cref="EntityModel.Entity"/>s and register systems to process them.
/// </summary>
public abstract class Scene : IDisposable
{
    internal readonly EntityScene EntityScene;
    
    protected Camera SceneCamera { get; private set; } = null!;


    #region Creation and destruction

    protected Scene()
    {
        EntityScene = new EntityScene();
    }
    
    
    public void Dispose()
    {
        OnUnload();
        
        EntityScene.Destroy();
        
        GC.SuppressFinalize(this);
    }

    #endregion


    #region Public API

    public Entity CreatePrimitive(PrimitiveType primitiveType, string name)
    {
        Entity e = CreateEntity(name);
        MeshRenderer c = e.AddComponent<MeshRenderer>();
        Material mat = new Material(Shader.Find("Defaults/Standard.shader"), "standard material");
        
        c.Mesh = Mesh.CreatePrimitive(primitiveType);
        c.Material = mat;
        
        mat.SetColor("_MainColor", Color.White);
        mat.SetFloat("_EmissionIntensity", 0f);
        mat.SetColor("_EmissiveColor", Color.Black);
        mat.SetTexture("_MainTex", Texture2D.Load("Defaults/grid.png"));
        mat.SetTexture("_NormalTex", Texture2D.Load("Defaults/default_normal.png"));
        mat.SetTexture("_SurfaceTex", Texture2D.Load("Defaults/default_surface.png"));
        mat.SetTexture("_EmissionTex", Texture2D.Load("Defaults/default_emission.png"));
        
        return e;
    }
    
    
    public T? FindObjectOfType<T>() where T : EntityComponent
    {
        return EntityScene.FindObjectOfType<T>();
    }
    
    
    /*public void Instantiate<T>(T prefab) where T : Entity
    {
        Entity e = prefab.Clone();
        EntityScene.AddEntity(e);
    }*/

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
        
        camera.RenderPriority = 0;
        camera.ClearFlags = CameraClearFlags.Color | CameraClearFlags.Depth;
        
        return camera;
    }

    protected virtual void CreateLights()
    {
        /*Entity dlEntity = CreateEntity("Directional Light");
        DirectionalLight dlComp = dlEntity.AddComponent<DirectionalLight>();
        dlComp.Transform.LocalEulerAngles = new Vector3(50, 225, 0);*/
        
        Entity alEntity = CreateEntity("Ambient Light");
        AmbientLight alComp = alEntity.AddComponent<AmbientLight>();
        //alComp.SkyIntensity = 0.4f;
        //alComp.GroundIntensity = 0.1f;
        alComp.SkyIntensity = 1f;
        alComp.GroundIntensity = 1f;
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
        SceneCamera = CreateSceneCamera();
        
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
    
    
    private Entity CreateEntity(string name)
    {
        Entity e = new(this, name);
        return e;
    }
}