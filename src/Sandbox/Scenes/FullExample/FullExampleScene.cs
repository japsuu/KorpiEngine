using KorpiEngine.Entities;
using KorpiEngine.Mathematics;
using KorpiEngine.Rendering;
using Random = KorpiEngine.Mathematics.SharedRandom;

namespace Sandbox.Scenes.FullExample;

/// <summary>
/// This scene demonstrates a variety of features, including
/// entity creation, component addition, input handling.
/// </summary>
internal class FullExampleScene : ExampleScene
{
    protected override string HelpTitle => "Full Example Scene";
    protected override string HelpText =>
        "This scene demonstrates a variety of features, including\n" +
        "entity creation, component addition, input handling.\n" +
        "Use the WASD keys to move the camera, and the mouse to look around.";


    protected override void OnLoad()
    {
        base.OnLoad();

        // Create a camera entity with our custom free camera component.
        Entity cameraEntity = CreateEntity("Scene Camera");
        cameraEntity.AddComponent<Camera>();
        cameraEntity.AddComponent<DemoFreeCam>();
        cameraEntity.Transform.Position = new Vector3(0f, 5f, -5f);
        
        // Create a directional light to illuminate the scene.
        Entity dlEntity = CreateEntity("Directional Light");
        DirectionalLight dlComp = dlEntity.AddComponent<DirectionalLight>();
        dlComp.Transform.LocalEulerAngles = new Vector3(130, 45, 0);
        
        // Create an ambient light to provide some base illumination.
        Entity alEntity = CreateEntity("Ambient Light");
        AmbientLight alComp = alEntity.AddComponent<AmbientLight>();
        alComp.SkyIntensity = 0.4f;
        alComp.GroundIntensity = 0.1f;

        // ----------------------------------------
        // Creating spheres in random positions that oscillate up and down

        for (int i = 0; i < 25; i++)
        {
            // Create a new entity with a name, and add a custom component to make it oscillate
            Entity root = CreateEntity($"Sphere {i}");
            if (i % 2 == 0)
                root.AddComponent<DemoOscillate>();

            // Create a sphere primitive and add it as a child of the root entity
            Entity model = CreatePrimitive(PrimitiveType.Sphere, "Sphere model");
            model.SetParent(root);

            // Move the root entity to a random position
            Vector2 randomPos = Random.InUnitCircle * 20;
            root.Transform.Position = new Vector3(randomPos.X, 0, randomPos.Y);
        }

        for (int i = 0; i < 25; i++)
        {
            // Create a new entity with a name
            Entity root = CreateEntity($"Cube {i}");

            // Create a cube primitive and add it as a child of the root entity
            Entity model = CreatePrimitive(PrimitiveType.Cube, "Cube model");
            model.SetParent(root);
            
            // Get the material of the mesh renderer component (provided by CreatePrimitive),
            // and randomize the color
            model.GetComponent<MeshRenderer>()!.MainColor = Random.ColorHDRFullAlpha;

            // Move the root entity to a random position
            Vector2 randomPos = Random.InUnitCircle * 20;
            root.Transform.Position = new Vector3(randomPos.X, 0, randomPos.Y);
            root.Transform.Rotation = Random.Rotation;
        }
    }
}