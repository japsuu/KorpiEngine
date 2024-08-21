using System.Collections;
using KorpiEngine.Core;
using KorpiEngine.Core.API;
using KorpiEngine.Core.API.AssetManagement;
using KorpiEngine.Core.API.InputManagement;
using KorpiEngine.Core.EntityModel;
using KorpiEngine.Core.EntityModel.Components;
using KorpiEngine.Core.Internal.AssetManagement;
using KorpiEngine.Core.Internal.AssetManagement.Importers;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Rendering.Cameras;
using KorpiEngine.Core.SceneManagement;
using KorpiEngine.Core.UI;
using KorpiEngine.Core.UI.DearImGui;
using KorpiEngine.Networking;
using Random = KorpiEngine.Core.API.Random;

namespace Sandbox.Scenes;

/// <summary>
/// This scene demonstrates a variety of features, including
/// entity creation, component addition, input handling.
/// </summary>
internal class FullExampleScene : Scene
{
    protected override void OnLoad()
    {
        // Create an entity to load the Sponza model
        Entity sponzaLoader = CreateEntity("Sponza Loader");
        sponzaLoader.AddComponent<SponzaLoader>();
        
        
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
            model.GetComponent<MeshRenderer>()!.MainColor = Random.ColorFullAlpha;

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
        
        component.Transform.Position = new Vector3(0, 1, 0);
        return component;
    }
}

internal class SponzaLoader : EntityComponent
{
    private const string SPONZA_WEB_URL = "https://github.com/jimmiebergmann/Sponza/raw/master";
    private static readonly string[] SponzaAssets =
    [
        "sponza.obj",
        "sponza.mtl",
        "textures/background.tga",
        "textures/background_ddn.tga",
        "textures/chain_texture.tga",
        "textures/chain_texture_ddn.tga",
        "textures/lion.tga",
        "textures/lion2_ddn.tga",
        "textures/lion_ddn.tga",
        "textures/spnza_bricks_a_ddn.tga",
        "textures/spnza_bricks_a_diff.tga",
        "textures/sponza_arch_ddn.tga",
        "textures/sponza_arch_diff.tga",
        "textures/sponza_ceiling_a_ddn.tga",
        "textures/sponza_ceiling_a_diff.tga",
        "textures/sponza_column_a_ddn.tga",
        "textures/sponza_column_a_diff.tga",
        "textures/sponza_column_b_ddn.tga",
        "textures/sponza_column_b_diff.tga",
        "textures/sponza_column_c_ddn.tga",
        "textures/sponza_column_c_diff.tga",
        "textures/sponza_curtain_blue_diff.tga",
        "textures/sponza_curtain_ddn.tga",
        "textures/sponza_curtain_diff.tga",
        "textures/sponza_curtain_green_diff.tga",
        "textures/sponza_details_ddn.tga",
        "textures/sponza_details_diff.tga",
        "textures/sponza_fabric_blue_diff.tga",
        "textures/sponza_fabric_ddn.tga",
        "textures/sponza_fabric_diff.tga",
        "textures/sponza_fabric_green_diff.tga",
        "textures/sponza_flagpole_ddn.tga",
        "textures/sponza_flagpole_diff.tga",
        "textures/sponza_floor_a_ddn.tga",
        "textures/sponza_floor_a_diff.tga",
        "textures/sponza_roof_ddn.tga",
        "textures/sponza_roof_diff.tga",
        "textures/sponza_thorn_ddn.tga",
        "textures/sponza_thorn_diff.tga",
        "textures/vase_ddn.tga",
        "textures/vase_dif.tga",
        "textures/vase_hanging.tga",
        "textures/vase_hanging_ddn.tga",
        "textures/vase_plant.tga",
        "textures/vase_round.tga",
        "textures/vase_round_ddn.tga"
    ];
    
    
    protected override void OnStart()
    {
        StartCoroutine(nameof(LoadSponzaWeb));
    }
    
    
    private IEnumerator LoadSponzaWeb()
    {
        // Create a web request to load the Sponza model and all its assets,
        // and save them to disk next to the executable in "WebAssets/sponza" subfolder.
        WebAssetLoadOperation operation = new("sponza", SPONZA_WEB_URL, false, SponzaAssets);
        
        yield return operation.SendWebRequest();
        
        // Load the Sponza model from disk
        LoadSponzaDisk(operation.SavePaths[0]);
    }
    
    
    private void LoadSponzaDisk(string path)
    {
        // Create a custom importer to scale the model down
        ModelImporter importer = (ModelImporter)AssetDatabase.GetImporter(".obj");
        importer.UnitScale = 0.1f;
        
        // Load the Sponza model from disk
        Entity asset = AssetDatabase.LoadAssetFile<Entity>(path, importer);
        
        asset.Spawn(Entity.Scene!);
        ImGuiWindowManager.RegisterWindow(new EntityEditor(asset));
    }
}

/// <summary>
/// This component makes the entity oscillate up and down.
/// </summary>
internal class DemoOscillate : EntityComponent
{
    private const float OSCILLATION_SPEED = 1f;
    private const float OSCILLATION_HEIGHT = 2f;

    private double _oscillationOffset;

    
    protected override void OnStart()
    {
        // Generate a random offset in the 0-1 range to make the oscillation unique for each entity
        _oscillationOffset = Random.Range(0f, 1f);
    }


    protected override void OnUpdate()
    {
        // Oscillate the entity up and down
        double time = Time.TotalTime + _oscillationOffset;
        double height = Math.Sin(time * OSCILLATION_SPEED) * OSCILLATION_HEIGHT;
        Transform.Position = new Vector3(Transform.Position.X, height, Transform.Position.Z);
    }
}

/// <summary>
/// This component allows the camera to be moved/rotated by the user.
/// </summary>
internal class DemoFreeCam : EntityComponent
{
    private const float LOOK_SENSITIVITY = 0.2f;
    private const float MAX_PITCH = 89.0f;
    private const float MIN_PITCH = -89.0f;

    private float _slowFlySpeed = 1.5f;
    private float _fastFlySpeed = 3.0f;

    private double _pitch;
    private double _yaw;
    private bool _isCursorLocked;
    
    
    protected override void OnStart()
    {
        // Set the initial pitch and yaw angles based on the current rotation
        Vector3 currentEulerAngles = Transform.EulerAngles;
        _pitch = currentEulerAngles.X;
        _yaw = currentEulerAngles.Y;
    }


    protected override void OnUpdate()
    {
        UpdateCursorLock();

        UpdatePosition();

        if (_isCursorLocked)
            UpdateRotation();
    }


    protected override void OnDrawGUI()
    {
        GUI.Begin("Free Camera Controls");

        GUI.Text("WASD - Move");
        GUI.Text("QE - Up/Down");
        GUI.Text("Right Mouse - Look");
        GUI.Text("Shift - Fast Mode");
        
        GUI.FloatSlider("Slow Speed", ref _slowFlySpeed, 0.1f, 10f);
        GUI.FloatSlider("Fast Speed", ref _fastFlySpeed, 1f, 50f);

        GUI.End();
    }


    private void UpdateCursorLock()
    {
        if (Input.GetMouseDown(MouseButton.Right))
            StartLooking();
        else if (Input.GetMouseUp(MouseButton.Right))
            StopLooking();
    }


    private void UpdatePosition()
    {
        float flySpeed = Input.GetKey(KeyCode.LeftShift) ? _fastFlySpeed : _slowFlySpeed;

        if (Input.GetKey(KeyCode.W)) // Forward
            Transform.Position += Transform.Forward * flySpeed * Time.DeltaTime;

        if (Input.GetKey(KeyCode.S)) // Backward
            Transform.Position += Transform.Backward * flySpeed * Time.DeltaTime;

        if (Input.GetKey(KeyCode.A)) // Left
            Transform.Position += Transform.Left * flySpeed * Time.DeltaTime;

        if (Input.GetKey(KeyCode.D)) // Right
            Transform.Position += Transform.Right * flySpeed * Time.DeltaTime;

        if (Input.GetKey(KeyCode.E)) // Up
            Transform.Position += Transform.Up * flySpeed * Time.DeltaTime;

        if (Input.GetKey(KeyCode.Q)) // Down
            Transform.Position += Transform.Down * flySpeed * Time.DeltaTime;
    }


    private void UpdateRotation()
    {
        // Calculate the offset of the mouse position
        _yaw += Input.MouseDelta.X * LOOK_SENSITIVITY;

        // Calculate new pitch and clamp it
        _pitch += Input.MouseDelta.Y * LOOK_SENSITIVITY;
        _pitch = Mathd.Clamp(_pitch, MIN_PITCH, MAX_PITCH);

        // Apply the new rotation
        Transform.Rotation = Quaternion.Euler((float)_pitch, (float)_yaw, 0f);
    }


    private void StartLooking()
    {
        _isCursorLocked = true;
        Cursor.LockState = CursorLockState.Locked;
    }


    private void StopLooking()
    {
        _isCursorLocked = false;
        Cursor.LockState = CursorLockState.None;
    }
}