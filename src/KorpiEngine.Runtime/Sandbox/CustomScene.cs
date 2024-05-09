using KorpiEngine.Core;
using KorpiEngine.Core.API;
using KorpiEngine.Core.API.InputManagement;
using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Rendering.Cameras;
using KorpiEngine.Core.SceneManagement;
using KorpiEngine.Core.Scripting;
using KorpiEngine.Core.Scripting.Components;
using Random = KorpiEngine.Core.API.Random;

namespace Sandbox;

internal class CustomScene : Scene
{
    private Entity _blueBoxEntity = null!;  // Automatically moves and rotates
    private TestPlayer _player = null!;
    private readonly List<Entity> _balls = [];


    protected override void Load()
    {
        // Create a bunch of ball entities in random positions
        for (int i = 0; i < 20; i++)
        {
            Entity e = CreatePrimitive(PrimitiveType.Sphere, "Ball Entity");
            e.Transform.Position = Random.InUnitSphere * 20;
            _balls.Add(e);
        }
        
        // Create a blue quad entity
        _blueBoxEntity = CreatePrimitive(PrimitiveType.Quad, "Blue Quad");
        _blueBoxEntity.Transform.Position = new Vector3(0, 3, 0);
        Material blueMaterial = _blueBoxEntity.GetComponent<MeshRenderer>()!.Material!;
        blueMaterial.SetColor(Material.DEFAULT_COLOR_PROPERTY, Color.Blue);
        
        // Create a player entity
        _player = Instantiate<TestPlayer>("Player");
        _player.Transform.Position = new Vector3(0, 0, 0);

        // Setup an FPS camera controller
        Entity mainCamera = Camera.MainCamera!.Entity;
        mainCamera.AddComponent<FreeCameraController>();
    }


    protected override void Update()
    {
        UpdateBlueBox();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            _player.Entity.GetComponent<IDamageable>()!.TakeDamage(10);
        }
    }


    private void UpdateBlueBox()
    {
        // Rotate the entity
        const float rotSpeedY = 15f;
        const float rotSpeedZ = 30f;
        Vector3 newEulerAngles = new Vector3(0, rotSpeedY * Time.DeltaTime, rotSpeedZ * Time.DeltaTime);
        _blueBoxEntity.Transform.Rotate(newEulerAngles);
        
        // Move the entity
        const float moveSpeed = 0.5f;
        _blueBoxEntity.Transform.Translate(new Vector3(1f, 0f, 0f) * moveSpeed * Time.DeltaTime);
    }
}