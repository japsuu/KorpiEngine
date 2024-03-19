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
            SetPosition(Position + Forward * _cameraFlySpeed * Time.DeltaTime); // Forward
        }

        if (Input.KeyboardState.IsKeyDown(Keys.S))
        {
            SetPosition(Position - Forward * _cameraFlySpeed * Time.DeltaTime); // Backward
        }

        if (Input.KeyboardState.IsKeyDown(Keys.A))
        {
            SetPosition(Position - Right * _cameraFlySpeed * Time.DeltaTime); // Left
        }

        if (Input.KeyboardState.IsKeyDown(Keys.D))
        {
            SetPosition(Position + Right * _cameraFlySpeed * Time.DeltaTime); // Right
        }

        if (Input.KeyboardState.IsKeyDown(Keys.Space))
        {
            SetPosition(Position + Up * _cameraFlySpeed * Time.DeltaTime); // Up
        }

        if (Input.KeyboardState.IsKeyDown(Keys.LeftShift))
        {
            SetPosition(Position - Up * _cameraFlySpeed * Time.DeltaTime); // Down
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

    
    protected override Vector3 CalculateForwardVector(float pitch, float yaw)
    {
        // First, the front matrix is calculated using some basic trigonometry.
        float x = MathF.Cos(pitch) * MathF.Cos(yaw);
        float y = MathF.Sin(pitch);
        float z = MathF.Cos(pitch) * MathF.Sin(yaw);

        // We need to make sure the vectors are all normalized, as otherwise we would get some funky results.
        return Vector3.Normalize(new Vector3(x, y, z));
    }


    protected override Vector3 CalculateRightVector(Vector3 forward)
    {
        // Calculate both the right and the up vector using cross product.
        // We are calculating the right from the "global" up.
        return Vector3.Normalize(Vector3.Cross(forward, Vector3.UnitY));
    }


    protected override Vector3 CalculateUpVector(Vector3 right, Vector3 forward)
    {
        return Vector3.Normalize(Vector3.Cross(right, forward));
    }
}