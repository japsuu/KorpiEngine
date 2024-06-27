using KorpiEngine.Core;
using KorpiEngine.Core.API;
using KorpiEngine.Core.API.AssetManagement;
using KorpiEngine.Core.API.InputManagement;
using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.API.Rendering.Textures;
using KorpiEngine.Core.EntityModel;
using KorpiEngine.Core.EntityModel.Components;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Rendering.Cameras;
using KorpiEngine.Core.SceneManagement;
using Random = KorpiEngine.Core.API.Random;

namespace Sandbox;

internal class DemoScene : Scene
{
    protected override void OnLoad()
    {
        // ----------------------------------------
        // Creating spheres in random positions that oscillate up and down

        for (int i = 0; i < 20; i++)
        {
            // Create a new entity with a name, and add a custom component to make it oscillate
            Entity root = new($"Sphere {i}");
            root.AddComponent<DemoOscillate>();

            // Create a sphere primitive and add it as a child of the root entity
            Entity model = CreatePrimitive(PrimitiveType.Sphere, "Sphere model");
            model.SetParent(root);

            // Move the root entity to a random position
            root.Transform.Position = Random.InUnitSphere * 20;
        }

        // ----------------------------------------
        // Creating a quad that moves and rotates

        // Create a primitive quad
        Entity quadEntity = CreatePrimitive(PrimitiveType.Quad, "Blue Quad");

        // Add a custom behavior component to make it move and rotate
        quadEntity.AddComponent<DemoMoveRotate>();

        // Get the material of the mesh renderer component (provided by CreatePrimitive), and set the material color to blue
        Material material = quadEntity.GetComponent<MeshRendererComponent>()!.Material!;
        material.SetColor(Material.DEFAULT_COLOR_PROPERTY, Color.Blue);
        material.SetTexture(Material.DEFAULT_SURFACE_TEX_PROPERTY, AssetDatabase.LoadAsset<Texture2D>("Defaults/white_pixel.png")!);
    }


    // ----------------------------------------
    // Creating a camera entity
    
    protected override CameraComponent CreateSceneCamera()
    {
        // We override the CreateSceneCamera method to add our custom camera component to the scene camera entity
        CameraComponent component = base.CreateSceneCamera();
        component.Entity.AddComponent<DemoFreeCam>();
        return component;
    }
}

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

internal class DemoMoveRotate : EntityComponent
{
    protected override void OnUpdate()
    {
        // Rotate the entity
        const float rotSpeedY = 15f;
        const float rotSpeedZ = 30f;
        Vector3 newEulerAngles = new(0, rotSpeedY * Time.DeltaTime, rotSpeedZ * Time.DeltaTime);
        Transform.Rotate(newEulerAngles);

        // Move the entity
        const float moveSpeed = 0.5f;
        Transform.Translate(new Vector3(1f, 0f, 0f) * moveSpeed * Time.DeltaTime);
    }
}

internal class DemoFreeCam : EntityComponent
{
    private const float LOOK_SENSITIVITY = 0.2f;

    private const double SLOW_FLY_SPEED = 1.5f;
    private const double FAST_FLY_SPEED = 3.0f;

    private bool _isCursorLocked;


    protected override void OnUpdate()
    {
        UpdateCursorLock();

        UpdatePosition();

        if (_isCursorLocked)
            UpdateRotation();
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
        double flySpeed = Input.GetKey(KeyCode.LeftShift) ? FAST_FLY_SPEED : SLOW_FLY_SPEED;

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
        double yaw = Input.MouseDelta.X * LOOK_SENSITIVITY;
        double pitch = Input.MouseDelta.Y * LOOK_SENSITIVITY;

        Vector3 eulers = new(pitch, yaw, 0f);

        Transform.Rotate(eulers);
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