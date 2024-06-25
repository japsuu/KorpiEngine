using KorpiEngine.Core;
using KorpiEngine.Core.API;
using KorpiEngine.Core.API.InputManagement;
using KorpiEngine.Core.EntityModel.Components;

namespace Sandbox;

internal class FreeCameraComponent : BehaviourComponent
{
    private const float LOOK_SENSITIVITY = 0.2f;

    private const double SLOW_FLY_SPEED = 1.5f;
    private const double FAST_FLY_SPEED = 3.0f;

    private bool _isCursorLocked;


    public override void Update()
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
            ParentTransform.Position += ParentTransform.Forward * flySpeed * Time.DeltaTime;

        if (Input.GetKey(KeyCode.S)) // Backward
            ParentTransform.Position += ParentTransform.Backward * flySpeed * Time.DeltaTime;

        if (Input.GetKey(KeyCode.A)) // Left
            ParentTransform.Position += ParentTransform.Left * flySpeed * Time.DeltaTime;

        if (Input.GetKey(KeyCode.D)) // Right
            ParentTransform.Position += ParentTransform.Right * flySpeed * Time.DeltaTime;

        if (Input.GetKey(KeyCode.E)) // Up
            ParentTransform.Position += ParentTransform.Up * flySpeed * Time.DeltaTime;

        if (Input.GetKey(KeyCode.Q)) // Down
            ParentTransform.Position += ParentTransform.Down * flySpeed * Time.DeltaTime;
    }


    private void UpdateRotation()
    {
        // Calculate the offset of the mouse position
        double yaw = Input.MouseDelta.X * LOOK_SENSITIVITY;
        double pitch = Input.MouseDelta.Y * LOOK_SENSITIVITY;

        Vector3 eulers = new(pitch, yaw, 0f);

        ParentTransform.Rotate(eulers);
    }


    private void StartLooking()
    {
        _isCursorLocked = true;
        Cursor.LockState = CursorLockState.Locked;
    }


    /// <summary>
    /// Disable free looking.
    /// </summary>
    private void StopLooking()
    {
        _isCursorLocked = false;
        Cursor.LockState = CursorLockState.None;
    }
}