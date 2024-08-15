using KorpiEngine.Core.API;
using KorpiEngine.Core.EntityModel;
using KorpiEngine.Core.EntityModel.Components;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Rendering.Cameras;
using KorpiEngine.Core.SceneManagement;

namespace Sandbox.Scenes;

/// <summary>
/// This scene is a simplified example of basic primitive shape rendering.
/// </summary>
internal class PrimitiveExampleScene : Scene
{
    protected override void OnLoad()
    {
        Entity e;
        Entity m;
        
        e = new Entity("Sphere 1");
        m = CreatePrimitive(PrimitiveType.Sphere, "Sphere model");
        m.AddComponent<MeshDebugGizmoDrawer>().DrawNormals = true;
        m.SetParent(e);
        e.Transform.Position = new Vector3(0, 6, 0);
        
        e = new Entity("Sphere 2");
        m = CreatePrimitive(PrimitiveType.Sphere, "Sphere model");
        m.AddComponent<MeshDebugGizmoDrawer>().DrawNormals = true;
        m.SetParent(e);
        e.Transform.Position = new Vector3(1, 4, -3);
        e.Transform.Rotation = Quaternion.Euler(0, -45, 45);
        
        e = new Entity("Sphere 3");
        m = CreatePrimitive(PrimitiveType.Sphere, "Sphere model");
        m.AddComponent<MeshDebugGizmoDrawer>().DrawNormals = true;
        m.SetParent(e);
        e.Transform.Position = new Vector3(-2, 4, -2);
        
        e = new Entity("Cube 1");
        m = CreatePrimitive(PrimitiveType.Cube, "Cube model");
        m.AddComponent<MeshDebugGizmoDrawer>().DrawNormals = true;
        m.SetParent(e);
        e.Transform.Position = new Vector3(0, -1, -2);
        e.Transform.Rotation = Quaternion.Euler(45, 45, 45);
        
        e = new Entity("Cube 2");
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
        
        component.Transform.Position = new Vector3(0, 5, -5);
        return component;
    }
}