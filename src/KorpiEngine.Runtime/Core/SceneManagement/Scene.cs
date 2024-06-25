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


    protected Scene()
    {
        EntityScene = new EntityScene();
        
        EntityScene.RegisterSceneSystem<SceneRenderSystem>();
    }
    
    
    public void Dispose()
    {
        Unload();
        
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
    
    
    internal void InternalLoad()
    {
        CreateSceneCamera();
        
        Load();
    }
    
    
    internal void InternalUpdate()
    {
        EntityScene.Update(SystemUpdateStage.PreUpdate);
        EntityScene.Update(SystemUpdateStage.Update);
        EntityScene.Update(SystemUpdateStage.PostUpdate);
    }
    
    
    internal void InternalFixedUpdate()
    {
        EntityScene.Update(SystemUpdateStage.PreFixedUpdate);
        EntityScene.Update(SystemUpdateStage.FixedUpdate);
        EntityScene.Update(SystemUpdateStage.PostFixedUpdate);
    }
    
    
    internal void InternalRender()
    {
        EntityScene.Update(SystemUpdateStage.PreRender);
        EntityScene.Update(SystemUpdateStage.Render);
        EntityScene.Update(SystemUpdateStage.PostRender);
    }
    
    
    private Entity CreateEntity(string name)
    {
        Entity e = new(this, name);
        return e;
    }
    
    
    protected virtual void CreateSceneCamera()
    {
        Entity cameraEntity = CreateEntity("Scene Camera");
        CameraComponent cameraComponent = cameraEntity.AddComponent<CameraComponent>();
        cameraComponent.RenderTarget = CameraRenderTarget.Screen;
        cameraComponent.RenderPriority = 0;
        cameraComponent.ClearFlags = CameraClearFlags.Color | CameraClearFlags.Depth;
    }
    
    
    /// <summary>
    /// Called when the scene is loaded.
    /// </summary>
    protected virtual void Load() { }
    
    /// <summary>
    /// Called when the scene is unloaded.
    /// </summary>
    protected virtual void Unload() { }
}