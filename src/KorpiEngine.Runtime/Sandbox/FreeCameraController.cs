using KorpiEngine.Core;
using KorpiEngine.Core.API;
using KorpiEngine.Core.InputManagement;
using KorpiEngine.Core.Scripting;

namespace Sandbox;

internal class FreeCameraController : Behaviour
{
    private const float LOOK_SENSITIVITY = 0.2f;
    
    private double _cameraFlySpeed = 1.5f;


    protected override void OnUpdate()
    {
        UpdatePosition();
        
        UpdateRotation();

        if (Input.IsKeyDown(KeyCode.LeftShift))
            UpdateFlySpeed();
    }


    private void UpdatePosition()
    {
        if (Input.IsKeyDown(KeyCode.W))
            Transform.Position += Transform.Forward * _cameraFlySpeed * Time.DeltaTime; // Forward

        if (Input.IsKeyDown(KeyCode.S))
            Transform.Position += Transform.Backward * _cameraFlySpeed * Time.DeltaTime; // Backward

        if (Input.IsKeyDown(KeyCode.A))
            Transform.Position += Transform.Left * _cameraFlySpeed * Time.DeltaTime; // Left

        if (Input.IsKeyDown(KeyCode.D))
            Transform.Position += Transform.Right * _cameraFlySpeed * Time.DeltaTime; // Right

        if (Input.IsKeyDown(KeyCode.E))
            Transform.Position += Transform.Up * _cameraFlySpeed * Time.DeltaTime; // Up

        if (Input.IsKeyDown(KeyCode.Q))
            Transform.Position += Transform.Down * _cameraFlySpeed * Time.DeltaTime; // Down
    }


    private void UpdateFlySpeed()
    {
        _cameraFlySpeed = Input.IsKeyDown(KeyCode.LeftShift) ? 3.0f : 1.5f;
    }


    private void UpdateRotation()
    {
        // Calculate the offset of the mouse position
        double yaw = Input.MouseDelta.X * LOOK_SENSITIVITY;
        double pitch = Input.MouseDelta.Y * LOOK_SENSITIVITY; // Reversed since y-coordinates range from bottom to top
        
        Vector3 eulers = new Vector3(pitch, yaw, 0f);
        
        Transform.Rotate(eulers);
    }
}