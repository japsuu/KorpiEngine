using KorpiEngine.Entities;
using KorpiEngine.Mathematics;
using KorpiEngine.Rendering;

namespace Sandbox.Scenes.SponzaExample;

/// <summary>
/// This scene demonstrates how to send a web request to load a 3D-model,
/// and spawn it into the scene.
/// </summary>
internal class SponzaExampleScene : ExampleScene
{
    protected override string HelpTitle => "Sponza Example Scene";
    protected override string HelpText =>
        "This scene demonstrates how to send a web request to load a 3D-model, and spawn it into the scene.\n" +
        "Use the WASD keys to move the camera, and the mouse to look around.\n" +
        "Check the console to see the progress of the model loading.";


    protected override void OnLoad()
    {
        base.OnLoad();
        
        // Create a camera entity with our custom free camera component.
        Entity cameraEntity = CreateEntity("Scene Camera");
        cameraEntity.AddComponent<Camera>();
        cameraEntity.AddComponent<DemoFreeCam>();
        cameraEntity.Transform.Position = new Vector3(0f, 1f, 0f);
        cameraEntity.Transform.Rotate(new Vector3(0f, 90f, 0f));
        
        // Create a directional light to illuminate the scene.
        Entity dlEntity = CreateEntity("Directional Light");
        DirectionalLight directionalLight = dlEntity.AddComponent<DirectionalLight>();
        directionalLight.Transform.Forward = new Vector3(-0.225f, -0.965f, -0.135f);
        directionalLight.Color = new ColorHDR(1f, 0.9f, 0.7f, 1f);
        
        // Create an ambient light to provide some base illumination.
        Entity alEntity = CreateEntity("Ambient Light");
        AmbientLight ambientLight = alEntity.AddComponent<AmbientLight>();
        ambientLight.SkyIntensity = 0.4f;
        ambientLight.GroundIntensity = 0.2f;

        // Create an entity that loads the Sponza model.
        Entity sponzaLoader = CreateEntity("Sponza Loader");
        sponzaLoader.AddComponent<SponzaLoader>();
    }
}