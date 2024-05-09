using KorpiEngine.Core;
using KorpiEngine.Core.API;
using KorpiEngine.Core.API.InputManagement;
using KorpiEngine.Core.Scripting;

namespace Sandbox;

internal class FreeCameraController : Behaviour
{
    private const float LOOK_SENSITIVITY = 0.2f;
    
    private const double SLOW_FLY_SPEED = 1.5f;
    private const double FAST_FLY_SPEED = 3.0f;
    
    private bool isCursorLocked = false;


    protected override void OnUpdate()
    {
        UpdateCursorLock();
        
        UpdatePosition();
        
        if (isCursorLocked)
            UpdateRotation();
    }


    private void UpdateCursorLock()
    {
        if (Input.GetMouseDown(MouseButton.Right))
        {
            StartLooking();
        }
        else if (Input.GetMouseUp(MouseButton.Right))
        {
            StopLooking();
        }
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
        double pitch = Input.MouseDelta.Y * LOOK_SENSITIVITY; // Reversed since y-coordinates range from bottom to top
        
        Vector3 eulers = new Vector3(pitch, yaw, 0f);
        
        Transform.Rotate(eulers);
    }


    protected override void OnDisable()
    {
        StopLooking();
    }


    private void StartLooking()
    {
        isCursorLocked = true;
        Cursor.LockState = CursorLockState.Locked;
    }

    /// <summary>
    /// Disable free looking.
    /// </summary>
    private void StopLooking()
    {
        isCursorLocked = false;
        Cursor.LockState = CursorLockState.None;
    }
}