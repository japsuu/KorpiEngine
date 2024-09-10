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
    
    
    protected override Camera CreateSceneCamera()
    {
        // We override the CreateSceneCamera method to add our custom camera component to the scene camera entity
        Camera component = base.CreateSceneCamera();
        component.Entity.AddComponent<DemoFreeCam>();
        
        component.Transform.Position = new Vector3(0, 5, 5);
        return component;
    }
}