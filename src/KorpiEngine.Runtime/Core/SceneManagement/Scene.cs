using KorpiEngine.Core.API.Rendering;
using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.API.Rendering.Shaders;
using KorpiEngine.Core.EntityModel;
using KorpiEngine.Core.EntityModel.Components;
using KorpiEngine.Core.EntityModel.Systems.World;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Rendering.Cameras;
using Entity = KorpiEngine.Core.EntityModel.Entity;

namespace KorpiEngine.Core.SceneManagement;

/// <summary>
/// An in-game scene, that can be loaded and unloaded and receives updates.
/// Can create <see cref="EntityModel.Entity"/>s and register systems to process them.
/// </summary>
public abstract class Scene : IDisposable
{
    internal readonly EntityScene EntityScene;
    
    protected CameraComponent SceneCamera { get; private set; } = null!;


    protected Scene()
    {
        EntityScene = new EntityScene();
        
        EntityScene.RegisterSceneSystem<SceneRenderSystem>();
    }
    
    
    public void Dispose()
    {
        OnUnload();
        
        EntityScene.Destroy();
        
        GC.SuppressFinalize(this);
    }


    public Entity CreatePrimitive(PrimitiveType primitiveType, string name)
    {
        Entity e = CreateEntity(name);
        MeshRendererComponent c = e.AddComponent<MeshRendererComponent>();
        c.Mesh = Mesh.CreatePrimitive(primitiveType);
        c.Material = new Material(Shader.Find("Defaults/Standard.shader"));
        return e;
    }
    
    
    /*public void Instantiate<T>(T prefab) where T : Entity
    {
        Entity e = prefab.Clone();
        EntityScene.AddEntity(e);
    }*/
    
    
    internal void InternalLoad()
    {
        SceneCamera = CreateSceneCamera();
        
        OnLoad();
    }
    
    
    internal void InternalUpdate()
    {
        // Explicit call to remove any dependencies to SystemUpdateStage
        EntityScene.PreUpdate();
        
        EntityScene.Update(EntityUpdateStage.PreUpdate);
        EntityScene.Update(EntityUpdateStage.Update);
        EntityScene.Update(EntityUpdateStage.PostUpdate);
    }
    
    
    internal void InternalFixedUpdate()
    {
        EntityScene.Update(EntityUpdateStage.PreFixedUpdate);
        EntityScene.Update(EntityUpdateStage.FixedUpdate);
        EntityScene.Update(EntityUpdateStage.PostFixedUpdate);
    }
    
    
    internal void InternalRender()
    {
        EntityScene.Update(EntityUpdateStage.PreRender);
        EntityScene.Update(EntityUpdateStage.Render);
        EntityScene.Update(EntityUpdateStage.PostRender);
    }
    
    
    private Entity CreateEntity(string name)
    {
        Entity e = new(this, name);
        return e;
    }


    protected void RegisterSceneSystem<T>() where T : SceneSystem, new()
    {
        EntityScene.RegisterSceneSystem<T>();
    }
    
    
    protected void UnregisterSceneSystem<T>() where T : SceneSystem
    {
        EntityScene.UnregisterSceneSystem<T>();
    }
    
    
    protected virtual CameraComponent CreateSceneCamera()
    {
        Entity cameraEntity = CreateEntity("Scene Camera");
        CameraComponent cameraComponent = cameraEntity.AddComponent<CameraComponent>();
        
        cameraComponent.RenderTarget = CameraRenderTarget.Screen;
        cameraComponent.RenderPriority = 0;
        cameraComponent.ClearFlags = CameraClearFlags.Color | CameraClearFlags.Depth;
        
        return cameraComponent;
    }
    
    
    /// <summary>
    /// Called when the scene is loaded.
    /// </summary>
    protected virtual void OnLoad() { }
    
    /// <summary>
    /// Called when the scene is unloaded.
    /// </summary>
    protected virtual void OnUnload() { }
}