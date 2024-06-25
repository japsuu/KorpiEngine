using KorpiEngine.Core;
using KorpiEngine.Core.API;
using KorpiEngine.Core.EntityModel;
using KorpiEngine.Core.EntityModel.Components;
using KorpiEngine.Core.EntityModel.Systems.Entity;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.SceneManagement;
using Random = KorpiEngine.Core.API.Random;

namespace Sandbox;

internal class CustomScene : Scene
{
    private Entity _blueBoxEntity = null!;  // Automatically moves and rotates
    private readonly List<Entity> _balls = [];


    protected override void Load()
    {
        // Create a bunch of ball entities in random positions
        for (int i = 0; i < 20; i++)
        {
            Entity e = CreatePrimitive(PrimitiveType.Sphere, "Ball Entity");
            e.RootSpatialComponent!.Transform.Position = Random.InUnitSphere * 20;
            _balls.Add(e);
        }
        
        // Create a blue quad entity
        _blueBoxEntity = CreatePrimitive(PrimitiveType.Quad, "Blue Quad");
        _blueBoxEntity.RootSpatialComponent!.Transform.Position = new Vector3(0, 3, 0);
        /*Material blueMaterial = _blueBoxEntity.GetComponent<MeshRenderer>()!.Material!;
        blueMaterial.SetColor(Material.DEFAULT_COLOR_PROPERTY, Color.Blue);
        blueMaterial.SetTexture(Material.DEFAULT_SURFACE_TEX_PROPERTY, AssetDatabase.LoadAsset<Texture2D>("Defaults/white_pixel.png")!);*/
        _blueBoxEntity.AddComponent<BlueBoxBehaviourComponent>("bbox");
        _blueBoxEntity.AddSystem<BehaviourSystem>();

        // Setup an FPS camera controller
        SceneCamera.AddComponent<FreeCameraComponent>();
        SceneCamera.AddSystem<BehaviourSystem>();
    }
}

internal class BlueBoxBehaviourComponent : BehaviourComponent
{
    public override void Update()
    {
        // Rotate the entity
        const float rotSpeedY = 15f;
        const float rotSpeedZ = 30f;
        Vector3 newEulerAngles = new Vector3(0, rotSpeedY * Time.DeltaTime, rotSpeedZ * Time.DeltaTime);
        Transform.Rotate(newEulerAngles);
        
        // Move the entity
        const float moveSpeed = 0.5f;
        Transform.Translate(new Vector3(1f, 0f, 0f) * moveSpeed * Time.DeltaTime);
    }
}