using KorpiEngine.Entities;
using KorpiEngine.Mathematics;
using KorpiEngine.Rendering;

namespace Sandbox.Scenes.PrimitiveExample;

/// <summary>
/// This scene is a simplified example of basic primitive shape rendering.
/// </summary>
internal class PrimitiveExampleScene : ExampleScene
{
    protected override string HelpTitle => "Primitive Example Scene";
    protected override string HelpText =>
        "This scene is a simplified example of basic primitive shape rendering.\n" +
        "Use the WASD keys to move the camera, and the mouse to look around.\n";


    protected override void OnLoad()
    {
        base.OnLoad();
        
        // Create a camera entity with our custom free camera component.
        Entity cameraEntity = CreateEntity("Scene Camera");
        cameraEntity.AddComponent<Camera>();
        cameraEntity.AddComponent<DemoFreeCam>();
        cameraEntity.Transform.Position = new Vector3(0, 5, 5);
        
        // Create a directional light to illuminate the scene.
        Entity dlEntity = CreateEntity("Directional Light");
        DirectionalLight dlComp = dlEntity.AddComponent<DirectionalLight>();
        dlComp.Transform.LocalEulerAngles = new Vector3(130, 45, 0);
        
        // Create an ambient light to provide some base illumination.
        Entity alEntity = CreateEntity("Ambient Light");
        AmbientLight alComp = alEntity.AddComponent<AmbientLight>();
        alComp.SkyIntensity = 0.4f;
        alComp.GroundIntensity = 0.1f;
        
        Entity e;
        Entity m;
        
        e = CreateEntity("Sphere 1");
        m = CreatePrimitive(PrimitiveType.Sphere, "Sphere model");
        m.AddComponent<MeshDebugGizmoDrawer>().DrawNormals = true;
        m.SetParent(e);
        e.Transform.Position = new Vector3(0, 6, 0);
        
        e = CreateEntity("Sphere 2");
        m = CreatePrimitive(PrimitiveType.Sphere, "Sphere model");
        m.AddComponent<MeshDebugGizmoDrawer>().DrawNormals = true;
        m.SetParent(e);
        e.Transform.Position = new Vector3(1, 4, -3);
        e.Transform.Rotation = Quaternion.CreateFromEulerAnglesDegrees(0, -45, 45);
        
        e = CreateEntity("Sphere 3");
        m = CreatePrimitive(PrimitiveType.Sphere, "Sphere model");
        m.AddComponent<MeshDebugGizmoDrawer>().DrawNormals = true;
        m.SetParent(e);
        e.Transform.Position = new Vector3(-2, 4, -2);
        
        e = CreateEntity("Cube 1");
        m = CreatePrimitive(PrimitiveType.Cube, "Cube model");
        m.AddComponent<MeshDebugGizmoDrawer>().DrawNormals = true;
        m.SetParent(e);
        e.Transform.Position = new Vector3(0, -1, -2);
        e.Transform.Rotation = Quaternion.CreateFromEulerAnglesDegrees(45, 45, 45);
        
        e = CreateEntity("Cube 2");
        m = CreatePrimitive(PrimitiveType.Cube, "Cube model");
        m.AddComponent<MeshDebugGizmoDrawer>().DrawNormals = true;
        m.SetParent(e);
        e.Transform.Position = new Vector3(0, -1, 2);
    }
}