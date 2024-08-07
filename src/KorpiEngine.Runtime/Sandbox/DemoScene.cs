﻿using KorpiEngine.Core;
using KorpiEngine.Core.API;
using KorpiEngine.Core.API.InputManagement;
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
        Entity e;
        Entity m;
        
        e = new Entity("Sphere 1");
        m = CreatePrimitive(PrimitiveType.Sphere, "Sphere model");
        ////m.AddComponent<MeshDebugGizmoDrawer>().DrawNormals = true;
        m.SetParent(e);
        e.Transform.Position = new Vector3(0, 6, 0);
        
        e = new Entity("Sphere 2");
        m = CreatePrimitive(PrimitiveType.Sphere, "Sphere model");
        ////m.AddComponent<MeshDebugGizmoDrawer>().DrawNormals = true;
        m.SetParent(e);
        e.Transform.Position = new Vector3(1, 4, -3);
        e.Transform.Rotation = Quaternion.Euler(0, -45, 45);
        
        e = new Entity("Sphere 3");
        m = CreatePrimitive(PrimitiveType.Sphere, "Sphere model");
        ////m.AddComponent<MeshDebugGizmoDrawer>().DrawNormals = true;
        m.SetParent(e);
        e.Transform.Position = new Vector3(-2, 4, -2);
        
        e = new Entity("Cube 1");
        m = CreatePrimitive(PrimitiveType.Cube, "Cube model");
        ////m.AddComponent<MeshDebugGizmoDrawer>().DrawNormals = true;
        m.SetParent(e);
        e.Transform.Position = new Vector3(0, -1, -2);
        e.Transform.Rotation = Quaternion.Euler(45, 45, 45);
        
        e = new Entity("Cube 2");
        m = CreatePrimitive(PrimitiveType.Cube, "Cube model");
        ////m.AddComponent<MeshDebugGizmoDrawer>().DrawNormals = true;
        m.SetParent(e);
        e.Transform.Position = new Vector3(0, -1, 2);
        
        // ----------------------------------------
        // Creating spheres in random positions that oscillate up and down

        /*for (int i = 0; i < 25; i++)
        {
            // Create a new entity with a name, and add a custom component to make it oscillate
            Entity root = new($"Sphere {i}");
            //root.AddComponent<DemoOscillate>();

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
            Entity root = new($"Cube {i}");

            // Create a cube primitive and add it as a child of the root entity
            Entity model = CreatePrimitive(PrimitiveType.Cube, "Cube model");
            model.SetParent(root);

            // Move the root entity to a random position
            Vector2 randomPos = Random.InUnitCircle * 20;
            root.Transform.Position = new Vector3(randomPos.X, 0, randomPos.Y);
        }*/

        /*// ----------------------------------------
        // Creating a blue point light
        
        Entity blueLightEntity = new("Point Light");
        PointLight blueLight = blueLightEntity.AddComponent<PointLight>();
        blueLight.Color = Color.Blue;
        blueLight.Radius = 10.0f;
        blueLight.Intensity = 3.0f;
        blueLightEntity.Transform.Position = new Vector3(0, 2, 0);

        // ----------------------------------------
        // Creating a red point light
        
        Entity redLightEntity = new("Point Light");
        PointLight redLight = redLightEntity.AddComponent<PointLight>();
        redLight.Color = Color.Red;
        redLight.Radius = 10.0f;
        redLight.Intensity = 3.0f;
        redLightEntity.Transform.Position = new Vector3(-2, 1.5, 1);*/

        /*// ----------------------------------------
        // Creating a quad that moves and rotates

        // Create a primitive quad
        Entity quadEntity = CreatePrimitive(PrimitiveType.Quad, "Blue Quad");

        // Add a custom behavior component to make it move and rotate
        quadEntity.AddComponent<DemoMoveRotate>();*/

        // Get the material of the mesh renderer component (provided by CreatePrimitive), and set the material color to blue
        // Material material = quadEntity.GetComponent<MeshRendererComponent>()!.Material.Res!;
        // material.SetColor(Material.DEFAULT_COLOR_PROPERTY, Color.Blue);
        // material.SetTexture(Material.DEFAULT_SURFACE_TEX_PROPERTY, AssetDatabase.LoadAsset<Texture2D>("Defaults/white_pixel.png")!);
    }


    // ----------------------------------------
    // Creating a camera entity
    
    protected override Camera CreateSceneCamera()
    {
        // We override the CreateSceneCamera method to add our custom camera component to the scene camera entity
        Camera component = base.CreateSceneCamera();
        component.Entity.AddComponent<DemoFreeCam>();
        
        component.Transform.Position = new Vector3(0, 5, -5);
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
    private const float MAX_PITCH = 89.0f;
    private const float MIN_PITCH = -89.0f;

    private const double SLOW_FLY_SPEED = 1.5f;
    private const double FAST_FLY_SPEED = 3.0f;

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