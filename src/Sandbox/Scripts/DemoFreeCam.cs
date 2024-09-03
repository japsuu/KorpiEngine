using KorpiEngine;
using KorpiEngine.EntityModel;
using KorpiEngine.InputManagement;
using KorpiEngine.UI;

namespace Sandbox;

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
        if (Input.GetMouseDown(MouseButton.Right) && !GUI.WantCaptureMouse)
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
        _pitch = MathOps.Clamp(_pitch, MIN_PITCH, MAX_PITCH);

        // Apply the new rotation
        Transform.Rotation = Quaternion.CreateFromYawPitchRoll(-(float)_yaw.ToRadians(), -(float)_pitch.ToRadians(), 0f);
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