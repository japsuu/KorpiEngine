using KorpiEngine;
using KorpiEngine.EntityModel;
using KorpiEngine.EntityModel.Components;
using KorpiEngine.Rendering;
using KorpiEngine.Rendering.Cameras;
using KorpiEngine.Rendering.Lighting;
using KorpiEngine.SceneManagement;
using Random = KorpiEngine.SharedRandom;

namespace Sandbox.Scenes.SponzaExample;

/// <summary>
/// This scene demonstrates how to send a web request to load a 3D-model,
/// and spawn it into the scene.
/// </summary>
internal class SponzaExampleScene : Scene
{
    protected override void OnLoad()
    {
        // Create an entity that loads the Sponza model.
        Entity sponzaLoader = CreateEntity("Sponza Loader");
        sponzaLoader.AddComponent<SponzaLoader>();
    }


    // We override the CreateSceneCamera method to add our custom camera component to the scene camera entity.
    protected override Camera CreateSceneCamera()
    {
        Camera component = base.CreateSceneCamera();
        component.Entity.AddComponent<DemoFreeCam>();
        component.Entity.Transform.Position = new Vector3(0f, 1f, 0f);
        component.Entity.Transform.Rotate(new Vector3(0f, 90f, 0f));
        
        return component;
    }


    // We override the CreateLights method to customize the default scene lighting.
    protected override void CreateLights()
    {
        Entity dlEntity = CreateEntity("Directional Light");
        DirectionalLight directionalLight = dlEntity.AddComponent<DirectionalLight>();
        directionalLight.Transform.Forward = new Vector3(-0.225f, -0.965f, -0.135f);
        directionalLight.Color = new ColorHDR(1f, 0.9f, 0.7f, 1f);
        
        Entity alEntity = CreateEntity("Ambient Light");
        AmbientLight ambientLight = alEntity.AddComponent<AmbientLight>();
        ambientLight.SkyIntensity = 0.4f;
        ambientLight.GroundIntensity = 0.2f;
    }
}