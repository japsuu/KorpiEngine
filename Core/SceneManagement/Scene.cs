using Arch.Core;
using Arch.Core.Extensions;
using KorpiEngine.Core.ECS;
using KorpiEngine.Core.ECS.Systems;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Rendering.Materials;
using KorpiEngine.Core.Scripting;
using OpenTK.Mathematics;
using Entity = KorpiEngine.Core.Scripting.Entity;

namespace KorpiEngine.Core.SceneManagement;

/// <summary>
/// An in-game scene, that can be loaded and unloaded and receives updates.
/// Can create <see cref="Entity"/>s and register systems to process them.
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
    
    /// <summary>
    /// The ESC world for this scene.
    /// </summary>
    internal readonly World World;


    protected Scene()
    {
        World = World.Create();
        _simulationSystems = new SceneSystemGroup("SimulationSystems");
        _fixedSimulationSystems = new SceneSystemGroup("FixedSimulationSystems");
        _presentationSystems = new SceneSystemGroup("PresentationSystems");
    }


    public Entity CreatePrimitive(PrimitiveType primitiveType, string name)
    {
        Entity e = CreateEntity(name);
        ref MeshRendererComponent c = ref e.AddNativeComponent<MeshRendererComponent>();
        c.Mesh = Mesh.CreatePrimitive(primitiveType);
        c.Material = new StandardMaterial3D();
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
        ref CameraComponent cameraComponent = ref cameraEntity.AddNativeComponent<CameraComponent>();
        cameraComponent.FOVRadians = MathHelper.DegreesToRadians(90f);
        cameraComponent.RenderPriority = 0;
    }
    
    
    /*protected void DestroyEntity(Arch.Core.Entity entity)
    {
        World.Destroy(entity);
    }*/


    private Entity CreateEntity(string name)
    {
        UUID uuid = new();
        string nameString = string.IsNullOrWhiteSpace(name) ? "Entity" : name;
        Arch.Core.Entity entity = World.Create(new IdComponent(uuid), new NameComponent(nameString), new TransformComponent());
        return new Entity(entity.Reference(), this);
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
    
    
    internal void InternalDraw()
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
        World.Dispose();
        //World.Destroy(World);
        GC.SuppressFinalize(this);
    }
}