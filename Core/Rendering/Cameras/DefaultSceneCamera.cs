using KorpiEngine.Core.InputManagement;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KorpiEngine.Core.Rendering.Cameras;

public class DefaultSceneCamera : Camera
{
    private const float LOOK_SENSITIVITY = 0.2f;
    
    private float _cameraFlySpeed = 1.5f;


    protected override void OnUpdate()
    {
        UpdatePosition();
        
        UpdateRotation();

        if (Input.KeyboardState.IsKeyDown(Keys.LeftShift))
            UpdateFlySpeed();
    }


    private void UpdatePosition()
    {
        if (Input.KeyboardState.IsKeyDown(Keys.W))
        {
            SetPosition(Transform.Position + Transform.Forward * _cameraFlySpeed * Time.DeltaTime); // Forward
        }

        if (Input.KeyboardState.IsKeyDown(Keys.S))
        {
            SetPosition(Transform.Position - Transform.Forward * _cameraFlySpeed * Time.DeltaTime); // Backward
        }

        if (Input.KeyboardState.IsKeyDown(Keys.A))
        {
            SetPosition(Transform.Position - Transform.Right * _cameraFlySpeed * Time.DeltaTime); // Left
        }

        if (Input.KeyboardState.IsKeyDown(Keys.D))
        {
            SetPosition(Transform.Position + Transform.Right * _cameraFlySpeed * Time.DeltaTime); // Right
        }

        if (Input.KeyboardState.IsKeyDown(Keys.Space))
        {
            SetPosition(Transform.Position + Transform.Up * _cameraFlySpeed * Time.DeltaTime); // Up
        }

        if (Input.KeyboardState.IsKeyDown(Keys.LeftShift))
        {
            SetPosition(Transform.Position - Transform.Up * _cameraFlySpeed * Time.DeltaTime); // Down
        }
    }


    private void UpdateFlySpeed()
    {
        switch (_cameraFlySpeed)
        {
            // Changing the fly speed should be accurate at the lower end, but fast when at the upper end.
            case <= 1f:
                _cameraFlySpeed += Input.MouseState.ScrollDelta.Y * 0.05f;
                break;
            case <= 5f:
            {
                _cameraFlySpeed += Input.MouseState.ScrollDelta.Y * 0.5f;

                if (_cameraFlySpeed < 1f)
                {
                    _cameraFlySpeed = 0.95f;
                }

                break;
            }
            case <= 10f:
            {
                _cameraFlySpeed += Input.MouseState.ScrollDelta.Y * 1f;

                if (_cameraFlySpeed < 5f)
                {
                    _cameraFlySpeed = 4.5f;
                }

                break;
            }
            default:
            {
                _cameraFlySpeed += Input.MouseState.ScrollDelta.Y * 5f;

                if (_cameraFlySpeed < 10f)
                {
                    _cameraFlySpeed = 9f;
                }

                break;
            }
        }

        _cameraFlySpeed = MathHelper.Clamp(_cameraFlySpeed, 0.05f, 50f);
    }


    private void UpdateRotation()
    {
        // Calculate the offset of the mouse position
        float deltaX = Input.MouseState.X - Input.MouseState.PreviousX;
        float deltaY = Input.MouseState.Y - Input.MouseState.PreviousY;

        YawDegrees += deltaX * LOOK_SENSITIVITY;
        PitchDegrees -= deltaY * LOOK_SENSITIVITY; // Reversed since y-coordinates range from bottom to top
    }
}