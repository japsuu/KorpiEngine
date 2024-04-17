using KorpiEngine.Core.ECS;
using KorpiEngine.Core.Platform;
using KorpiEngine.Core.Scripting;
using OpenTK.Mathematics;

namespace KorpiEngine.Core.Rendering.Cameras;

/// <summary>
/// The base class for a camera used to render the scene.
/// </summary>
public sealed class Camera : Component
{
    internal override Type NativeComponentType => typeof(CameraComponent);

    public const float NEAR_CLIP_PLANE = 0.01f;
    public const float FAR_CLIP_PLANE = 1000f;

    /// <summary>
    /// Finds the camera with the highest priority, currently rendering to the screen.
    /// Expensive call, because it iterates through all scenes and entities.
    /// </summary>
    public static Camera? MainCamera => CameraFinder.FindMainCamera();

    /// <summary>
    /// The view matrix of this camera.
    /// Matrix that transforms from world to camera space.
    /// </summary>
    public Matrix4 ViewMatrix => Transform.Matrix.Inverted();   // Could also use Matrix4.LookAt(Transform.Position, Transform.Position + Transform.Forward, Transform.Up);

    /// <summary>
    /// The projection matrix of this camera.
    /// </summary>
    public Matrix4 ProjectionMatrix => Matrix4.CreatePerspectiveFieldOfView(FovRadians, WindowInfo.ClientAspectRatio, NEAR_CLIP_PLANE, FAR_CLIP_PLANE);

    /// <summary>
    /// The render priority of this camera.
    /// Camera with the highest render priority will be rendered first.
    /// </summary>
    public short RenderPriority
    {
        get => Entity.GetNativeComponent<CameraComponent>().RenderPriority;
        set => Entity.GetNativeComponent<CameraComponent>().RenderPriority = value;
    }

    public Frustum ViewFrustum => CalculateFrustum();

    /// <summary>
    /// The field of view of the camera (radians)
    /// </summary>
    public float FovRadians
    {
        get => Entity.GetNativeComponent<CameraComponent>().FOVRadians;
        set => Entity.GetNativeComponent<CameraComponent>().FOVRadians = value;
    }

    /// <summary>
    /// The field of view (FOV degrees, the vertical angle of the camera view).
    /// </summary>
    public float FovDegrees
    {
        get => MathHelper.RadiansToDegrees(FovRadians);
        set
        {
            float angle = MathHelper.Clamp(value, 1f, 90f);

            // We convert from degrees to radians as soon as the property is set to improve performance.
            FovRadians = MathHelper.DegreesToRadians(angle);
        }
    }
    
    
    public CameraClearType ClearType
    {
        get => Entity.GetNativeComponent<CameraComponent>().ClearType;
        set => Entity.GetNativeComponent<CameraComponent>().ClearType = value;
    }
    
    
    public Color ClearColor
    {
        get => Entity.GetNativeComponent<CameraComponent>().ClearColor;
        set => Entity.GetNativeComponent<CameraComponent>().ClearColor = value;
    }
    
    
    /// <returns>If the provided world position is visible on screen.</returns>
    public bool WorldToScreenPosition(Vector3 worldPosition, out Vector2 screenPos)
    {
        Vector4 clipSpacePosition = new Vector4(worldPosition, 1) * ViewMatrix * ProjectionMatrix;
        
        // Without this the coordinates are visible even when looking straight away from them.
        if (clipSpacePosition.W <= 0)
        {
            screenPos = Vector2.NegativeInfinity;
            return false;
        }
        
        Vector3 normalizedDeviceCoordinates = clipSpacePosition.Xyz / clipSpacePosition.W;
        Vector2 screenCoordinates = new Vector2(normalizedDeviceCoordinates.X, -normalizedDeviceCoordinates.Y);
        screenCoordinates += Vector2.One;
        screenCoordinates /= 2;
        screenCoordinates.X *= WindowInfo.ClientWidth;
        screenCoordinates.Y *= WindowInfo.ClientHeight;
        screenPos = screenCoordinates;
        return true;
    }


    private Frustum CalculateFrustum()
    {
        Matrix4 viewProjection = ViewMatrix * ProjectionMatrix;
        FrustumPlane[] planes = new FrustumPlane[6];

        // Top plane.
        planes[0] = new FrustumPlane
        {
            Normal = new Vector3(
                viewProjection.M14 - viewProjection.M12, viewProjection.M24 - viewProjection.M22, viewProjection.M34 - viewProjection.M32),
            Distance = viewProjection.M44 - viewProjection.M42
        };

        // Bottom plane.
        planes[1] = new FrustumPlane
        {
            Normal = new Vector3(
                viewProjection.M14 + viewProjection.M12, viewProjection.M24 + viewProjection.M22, viewProjection.M34 + viewProjection.M32),
            Distance = viewProjection.M44 + viewProjection.M42
        };

        // Right plane.
        planes[2] = new FrustumPlane
        {
            Normal = new Vector3(
                viewProjection.M14 - viewProjection.M11, viewProjection.M24 - viewProjection.M21, viewProjection.M34 - viewProjection.M31),
            Distance = viewProjection.M44 - viewProjection.M41
        };

        // Left plane.
        planes[3] = new FrustumPlane
        {
            Normal = new Vector3(
                viewProjection.M14 + viewProjection.M11, viewProjection.M24 + viewProjection.M21, viewProjection.M34 + viewProjection.M31),
            Distance = viewProjection.M44 + viewProjection.M41
        };

        // Far plane.
        planes[4] = new FrustumPlane
        {
            Normal = new Vector3(
                viewProjection.M14 - viewProjection.M13, viewProjection.M24 - viewProjection.M23, viewProjection.M34 - viewProjection.M33),
            Distance = viewProjection.M44 - viewProjection.M43
        };

        // Near plane.
        planes[5] = new FrustumPlane
        {
            Normal = new Vector3(viewProjection.M13, viewProjection.M23, viewProjection.M33),
            Distance = viewProjection.M43
        };
        
        // Construct the frustum.
        Frustum frustum = new(planes);

        // Normalize the planes.
        for (int i = 0; i < 6; i++)
        {
            float length = frustum.Planes[i].Normal.Length;
            frustum.Planes[i].Normal /= length;
            frustum.Planes[i].Distance /= length;
        }
        
        return frustum;
    }
}