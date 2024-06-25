using KorpiEngine.Core;
using KorpiEngine.Core.API;
using KorpiEngine.Core.API.InputManagement;
using KorpiEngine.Core.EntityModel;
using KorpiEngine.Core.EntityModel.SpatialHierarchy;

namespace Sandbox;

internal class FreeCameraSystem : EntitySystem<FreeCameraComponent>
{
    public override SystemUpdateStage[] UpdateStages => [SystemUpdateStage.Update];
    public override bool IsSingleton => true;
    
    private FreeCameraComponent? _cameraComponent;


    protected override void RegisterComponent(FreeCameraComponent c)
    {
        if (_cameraComponent != null)
            throw new InvalidOperationException("Only one FreeCameraComponent can be registered at a time.");

        _cameraComponent = c;
    }


    protected override void UnregisterComponent(FreeCameraComponent c)
    {
        _cameraComponent = null;
    }


    public override void Update(SystemUpdateStage stage)
    {
        if (_cameraComponent == null)
            return;

        _cameraComponent.Update();
    }
}

internal class FreeCameraComponent : SpatialEntityComponent
{
    private const float LOOK_SENSITIVITY = 0.2f;

    private const double SLOW_FLY_SPEED = 1.5f;
    private const double FAST_FLY_SPEED = 3.0f;

    private bool _isCursorLocked;


    public void Update()
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
        double pitch = Input.MouseDelta.Y * LOOK_SENSITIVITY; // Reversed since y-coordinates range from bottom to top

        Vector3 eulers = new(pitch, yaw, 0f);

        Transform.Rotate(eulers);
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