using Arch.Core;
using KorpiEngine.Core.API;
using KorpiEngine.Core.API.Rendering;
using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.API.Rendering.Shaders;
using KorpiEngine.Core.ECS;
using KorpiEngine.Core.ECS.Systems;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Scripting;
using Entity = KorpiEngine.Core.Scripting.Entity;

namespace KorpiEngine.Core.SceneManagement;

/// <summary>
/// An in-game scene, that can be loaded and unloaded and receives updates.
/// Can create <see cref="Scripting.Entity"/>s and register systems to process them.
/// </summary>
public abstract class Scene : IDisposable
{
    private readonly BehaviourSystem _behaviourSystem;
    private readonly RenderSystem _renderSystem;
    
    /// <summary>
    /// The ESC world for this scene.
    /// </summary>
    internal readonly World World;


    protected Scene()
    {
        World = World.Create();
        _behaviourSystem = new BehaviourSystem(this);
        _renderSystem = new RenderSystem(this);
    }


    public Entity CreatePrimitive(PrimitiveType primitiveType, string name)
    {
        Entity e = CreateEntity(name);
        ref MeshRendererComponent c = ref e.AddNativeComponent<MeshRendererComponent>();
        c.Mesh = Mesh.CreatePrimitive(primitiveType);
        c.Material = new Material(Shader.Find("Defaults/Standard.shader"));
        return e;
    }


    /// <summary>
    /// Internally instantiates a new entity with the given component and name, and returns the component.
    /// </summary>
    /// <param name="name">The name of the entity.</param>
    /// <typeparam name="T">The type of the component to add.</typeparam>
    /// <returns>The added component.</returns>
    public T Instantiate<T>(string name) where T : Behaviour, new()
    {
        Entity e = CreateEntity(name);
        return e.AddComponent<T>();
    }
    
    
    protected virtual void CreateSceneCamera()
    {
        Entity cameraEntity = CreateEntity("Scene Camera");
        CameraComponent c = new();
        ref CameraComponent cameraComponent = ref cameraEntity.AddNativeComponent(c);
        cameraComponent.FOVDegrees = 60;
        cameraComponent.RenderPriority = 0;
    }


    protected virtual void CreateDirectionalLight()
    {
        Entity lightEntity = CreateEntity("Directional Light");
        DirectionalLightComponent c = new();
        lightEntity.AddNativeComponent(c);
        
        lightEntity.Transform.Position = new Vector3(0, 10, 0);
        lightEntity.Transform.Rotate(new Vector3(-45, 45, 0));
    }
    
    
    /*protected void DestroyEntity(Arch.Core.Entity entity)
    {
        World.Destroy(entity);
    }*/


    private Entity CreateEntity(string name)
    {
        return Entity.Create(name, this);
    }
    
    
    internal void InternalLoad()
    {
        CreateSceneCamera();
        CreateDirectionalLight();
        
        // Initialize systems.
        _behaviourSystem.Initialize();
        
        Load();
    }


    internal void InternalUpdate()
    {
        _behaviourSystem.Update();
        
        Update();
    }
    
    
    internal void InternalFixedUpdate()
    {
        _behaviourSystem.FixedUpdate();
        
        FixedUpdate();
    }
    
    
    internal void InternalDraw()
    {
        _renderSystem.Update();
        
        LateUpdate();
    }
    
    
    /// <summary>
    /// Called when the scene is loaded.
    /// </summary>
    protected virtual void Load() { }
    
    /// <summary>
    /// Called every frame.
    /// </summary>
    protected virtual void Update() { }
    
    /// <summary>
    /// Called every frame after <see cref="Update"/>, just before the scene is drawn.
    /// </summary>
    protected virtual void LateUpdate() { }
    
    /// <summary>
    /// Called from the fixed update loop.
    /// </summary>
    protected virtual void FixedUpdate() { }
    
    /// <summary>
    /// Called when the scene is unloaded.
    /// </summary>
    protected virtual void Unload() { }
    
    
    public void Dispose()
    {
        Unload();
        
        _behaviourSystem.Dispose();
        _renderSystem.Dispose();
        World.Dispose();
        //World.Destroy(World);
        GC.SuppressFinalize(this);
    }
}