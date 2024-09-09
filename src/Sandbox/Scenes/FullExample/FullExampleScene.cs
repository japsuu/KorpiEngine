using KorpiEngine;
using KorpiEngine.Entities;
using KorpiEngine.Mathematics;
using KorpiEngine.Rendering;
using KorpiEngine.SceneManagement;
using Random = KorpiEngine.Mathematics.SharedRandom;

namespace Sandbox.Scenes.FullExample;

/// <summary>
/// This scene demonstrates a variety of features, including
/// entity creation, component addition, input handling.
/// </summary>
internal class FullExampleScene : Scene
{
    protected override void OnLoad()
    {
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


    // ----------------------------------------
    // Creating a camera entity
    
    protected override Camera CreateSceneCamera()
    {
        // We override the CreateSceneCamera method to add our custom camera component to the scene camera entity
        Camera component = base.CreateSceneCamera();
        component.Entity.AddComponent<DemoFreeCam>();
        component.Entity.Transform.Position = new Vector3(0f, 5f, -5f);
        
        return component;
    }
}