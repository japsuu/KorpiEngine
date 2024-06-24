using KorpiEngine.Core.API.Rendering;
using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.API.Rendering.Shaders;
using KorpiEngine.Core.ECS;
using KorpiEngine.Core.ECS.Systems;
using KorpiEngine.Core.Rendering;
using Entity = KorpiEngine.Core.EntityModel.Entity;

namespace KorpiEngine.Core.SceneManagement;

/// <summary>
/// An in-game scene, that can be loaded and unloaded and receives updates.
/// Can create <see cref="EntityModel.Entity"/>s and register systems to process them.
/// </summary>
public abstract class Scene : IDisposable
{
    /// <summary>
    /// The systems for this scene that are updated from the <see cref="Update"/> method.
    /// Typically used for game logic.
    /// </summary>
    private readonly SceneSystemGroup _simulationSystems;
    
    /// <summary>
    /// The systems for this scene that are updated from the <see cref="FixedUpdate"/> method.
    /// Typically used for physics.
    /// </summary>
    private readonly SceneSystemGroup _fixedSimulationSystems;
    
    /// <summary>
    /// The systems for this scene that are updated when the scene is drawn.
    /// Typically used for rendering and post-rendering effects.
    /// </summary>
    private readonly SceneSystemGroup _presentationSystems;


    protected Scene()
    {
        _simulationSystems = new SceneSystemGroup("SimulationSystems");
        _fixedSimulationSystems = new SceneSystemGroup("FixedSimulationSystems");
        _presentationSystems = new SceneSystemGroup("PresentationSystems");
    }


    public Entity CreatePrimitive(PrimitiveType primitiveType, string name)
    {
        Entity e = CreateEntity(name);
        ref MeshRendererComponent c = ref e.AddNativeComponent<MeshRendererComponent>();
        c.Mesh = Mesh.CreatePrimitive(primitiveType);
        c.Material = new Material(Shader.Find("Defaults/Standard.shader"));
        return e;
    }
    
    
    protected virtual void CreateSceneCamera()
    {
        Entity cameraEntity = CreateEntity("Scene Camera");
        CameraComponent comp = new CameraComponent();
        ref CameraComponent cameraComponent = ref cameraEntity.AddNativeComponent(comp);
        cameraComponent.FOVDegrees = 60;
        cameraComponent.RenderPriority = 0;
    }
    
    
    internal void InternalLoad()
    {
        CreateSceneCamera();
        
        // Register systems.
        RegisterSimulationSystems(_simulationSystems);
        RegisterFixedSimulationSystems(_fixedSimulationSystems);
        RegisterPresentationSystems(_presentationSystems);
        
        // Initialize systems.
        _simulationSystems.Initialize();
        _fixedSimulationSystems.Initialize();
        _presentationSystems.Initialize();
        
        Load();
    }
    
    
    internal void InternalUpdate()
    {
        _simulationSystems.BeforeUpdate(Time.DeltaTimeDouble);
        _simulationSystems.Update(Time.DeltaTimeDouble);
        _simulationSystems.AfterUpdate(Time.DeltaTimeDouble);
        
        Update();
    }
    
    
    internal void InternalFixedUpdate()
    {
        _fixedSimulationSystems.BeforeUpdate(Time.DeltaTimeDouble);
        _fixedSimulationSystems.Update(Time.DeltaTimeDouble);
        _fixedSimulationSystems.AfterUpdate(Time.DeltaTimeDouble);
        
        FixedUpdate();
    }
    
    
    internal void InternalRender()
    {
        _presentationSystems.BeforeUpdate(Time.DeltaTimeDouble);
        _presentationSystems.Update(Time.DeltaTimeDouble);
        _presentationSystems.AfterUpdate(Time.DeltaTimeDouble);
        
        LateUpdate();
    }


    /// <summary>
    /// Register new simulation systems to the scene.
    /// Called before <see cref="Load"/>
    /// The systems will be automatically updated in the update loop, in the order they were registered.
    /// </summary>
    protected virtual void RegisterSimulationSystems(SceneSystemGroup systems)
    {
        systems.Add(new BehaviourSystem(this));
    }


    /// <summary>
    /// Register new fixed simulation systems to the scene.
    /// Called before <see cref="Load"/>
    /// The systems will be automatically updated in the fixed update loop, in the order they were registered.
    /// </summary>
    protected virtual void RegisterFixedSimulationSystems(SceneSystemGroup systems)
    {
        systems.Add(new BehaviourFixedUpdateSystem(this));
    }


    /// <summary>
    /// Register new presentation (rendering) systems to the scene.
    /// Called before <see cref="Load"/>
    /// The systems are automatically updated after simulation systems (<see cref="RegisterSimulationSystems"/>), in the order they were registered.
    /// </summary>
    protected virtual void RegisterPresentationSystems(SceneSystemGroup systems)
    {
        systems.Add(new MeshRenderSystem(this));
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
        
        _simulationSystems.Dispose();
        _fixedSimulationSystems.Dispose();
        _presentationSystems.Dispose();
        
        GC.SuppressFinalize(this);
    }
}