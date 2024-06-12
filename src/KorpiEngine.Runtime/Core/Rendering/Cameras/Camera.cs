using KorpiEngine.Core.API;
using KorpiEngine.Core.API.Rendering;
using KorpiEngine.Core.API.Rendering.Textures;
using KorpiEngine.Core.ECS;
using KorpiEngine.Core.Internal.AssetManagement;
using KorpiEngine.Core.Platform;
using KorpiEngine.Core.SceneManagement;
using KorpiEngine.Core.Scripting;

namespace KorpiEngine.Core.Rendering.Cameras;

/// <summary>
/// The base class for a camera used to render the scene.
/// </summary>
public sealed class Camera : Component
{
    internal override Type NativeComponentType => typeof(CameraComponent);

    public const float NEAR_CLIP_PLANE = 0.01f;
    public const float FAR_CLIP_PLANE = 1000f;

    internal static Camera? RenderingCamera;

    /// <summary>
    /// Finds the camera with the highest priority, currently rendering to the screen.
    /// Expensive call, because it iterates through all scenes and entities.
    /// </summary>
    public static Camera? MainCamera => CameraFinder.FindMainCamera();

    /// <summary>
    /// The view matrix of this camera.
    /// Matrix that transforms from world to camera space.
    /// </summary>
    public Matrix4x4 ViewMatrix => Matrix4x4.CreateLookToLeftHanded(Transform.Position, Transform.Forward, Transform.Up);
    
#warning TEMPORARY, may cause issues with component serialization
    public event Action<int, int>? Resize;
    
    /// <summary>
    /// The projection matrix of this camera.
    /// </summary>
    public Matrix4x4 GetProjectionMatrix(float width, float height) => ProjectionType == CameraProjectionType.Orthographic ?
        System.Numerics.Matrix4x4.CreateOrthographicLeftHanded(width, height, NEAR_CLIP_PLANE, FAR_CLIP_PLANE).ToDouble() :
        System.Numerics.Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded(FieldOfView.ToRad(), width / height, NEAR_CLIP_PLANE, FAR_CLIP_PLANE).ToDouble();

    /// <summary>
    /// The render priority of this camera.
    /// Camera with the highest render priority will be rendered first.
    /// </summary>
    public short RenderPriority
    {
        get => Entity.GetNativeComponent<CameraComponent>().RenderPriority;
        set => Entity.GetNativeComponent<CameraComponent>().RenderPriority = value;
    }

    public CameraProjectionType ProjectionType
    {
        get => Entity.GetNativeComponent<CameraComponent>().ProjectionType;
        set => Entity.GetNativeComponent<CameraComponent>().ProjectionType = value;
    }

    public Frustum ViewFrustum => CalculateFrustum();

    /// <summary>
    /// The field of view (FOV degrees, the vertical angle of the camera view).
    /// </summary>
    public float FieldOfView
    {
        get => Entity.GetNativeComponent<CameraComponent>().FOVDegrees;
        set => Entity.GetNativeComponent<CameraComponent>().FOVDegrees = value;
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
    
    
    public AssetRef<RenderTexture> Target
    {
        get => Entity.GetNativeComponent<CameraComponent>().Target;
        set => Entity.GetNativeComponent<CameraComponent>().Target = value;
    }
    
    
    public float RenderResolution
    {
        get => Entity.GetNativeComponent<CameraComponent>().RenderResolution;
        set => Entity.GetNativeComponent<CameraComponent>().RenderResolution = value;
    }
    
    
    public GBuffer? GBuffer
    {
        get => Entity.GetNativeComponent<CameraComponent>().GBuffer;
        set => Entity.GetNativeComponent<CameraComponent>().GBuffer = value;
    }
    
    
    public Matrix4x4? OldView
    {
        get => Entity.GetNativeComponent<CameraComponent>().OldView;
        set => Entity.GetNativeComponent<CameraComponent>().OldView = value;
    }
    
    
    public Matrix4x4? OldProjection
    {
        get => Entity.GetNativeComponent<CameraComponent>().OldProjection;
        set => Entity.GetNativeComponent<CameraComponent>().OldProjection = value;
    }
    
    
    public DebugDrawType DebugDraw
    {
        get => Entity.GetNativeComponent<CameraComponent>().DebugDraw;
        set => Entity.GetNativeComponent<CameraComponent>().DebugDraw = value;
    }
    
    
    private bool DoClear => ClearType != CameraClearType.Nothing;


    private Vector2 GetRenderTargetSize()
    {
        if (Target.IsAvailable)
            return new Vector2(Target.Res!.Width, Target.Res!.Height);
        
        return new Vector2(Graphics.KorpiWindow.FramebufferSize.X, Graphics.KorpiWindow.FramebufferSize.Y);
    }
    
    
    private void CheckGBuffer()
    {
        RenderResolution = Math.Clamp(RenderResolution, 0.1f, 4.0f);

        Vector2 size = GetRenderTargetSize() * RenderResolution;
        if (GBuffer == null)
        {
            GBuffer = new GBuffer((int)size.X, (int)size.Y);
            Resize?.Invoke(GBuffer.Width, GBuffer.Height);
        }
        else if (GBuffer.Width != (int)size.X || GBuffer.Height != (int)size.Y)
        {
            GBuffer.UnloadGBuffer();
            GBuffer = new GBuffer((int)size.X, (int)size.Y);
            Resize?.Invoke(GBuffer.Width, GBuffer.Height);
        }
    }
    
    
    private void OpaquePass()
    {
        SceneManager.ForeachComponent((x) => x.Do(x.OnPreRender));
        GBuffer.Begin();                            // Start
        RenderAllOfOrder(RenderingOrder.Opaque);    // Render
        GBuffer.End();                              // End
        SceneManager.ForeachComponent((x) => x.Do(x.OnPostRender));
    }
    
    
    internal void RenderAllOfOrder(RenderingOrder order)
    {
        foreach (var go in SceneManager.AllGameObjects)
            if (go.enabledInHierarchy)
                foreach (var comp in go.GetComponents())
                    if (comp.Enabled && comp.RenderOrder == order)
                        comp.OnRenderObject();
    }


    public void Render(int width, int height)
    {
        if (RenderPipeline.IsAvailable == false)
        {
            Application.Logger.Error($"Camera on {Entity.Name} has no RenderPipeline assigned, Falling back to default.");
            RenderPipeline = Application.AssetProvider.LoadAsset<RenderPipeline>("Defaults/DefaultRenderPipeline.scriptobj");
            if (RenderPipeline.IsAvailable == false)
            {
                Application.Logger.Error($"Camera on {Entity.Name} cannot render, Missing Default Render Pipeline!");
                return;
            }
        }

        var rp = RenderPipeline;
        if (Target.IsAvailable)
        {
            width = Target.Res!.Width;
            height = Target.Res!.Height;
        }
        else if (width == -1 || height == -1)
        {
            width = Graphics.KorpiWindow.FramebufferSize.X;
            height = Graphics.KorpiWindow.FramebufferSize.Y;
        }

        width = (int)(width * RenderResolution);
        height = (int)(height * RenderResolution);

        CheckGBuffer();


        RenderingCamera = this;

        Graphics.ViewMatrix = ViewMatrix;
        Graphics.ProjectionMatrix = RenderingCamera.GetProjectionMatrix(width, height);
        Graphics.OldViewMatrix = OldView ?? Graphics.ViewMatrix;
        Graphics.OldProjectionMatrix = OldProjection ?? Graphics.ProjectionMatrix;

        // Set default jitter to false, this is set to true in a TAA pass
        rp.Res!.Prepare(width, height);

        Matrix4x4.Invert(Graphics.ViewMatrix, out Matrix4x4 inverseView);
        Matrix4x4.Invert(Graphics.ProjectionMatrix, out Matrix4x4 inverseProjection);
        Graphics.InverseViewMatrix = inverseView;
        Graphics.InverseProjectionMatrix = inverseProjection;

        OpaquePass();

        var outputNode = rp.Res!.GetNode<OutputNode>();
        if (outputNode == null)
        {
            EarlyEndRender();

            Application.Logger.Error("RenderPipeline has no OutputNode!");
            return;
        }

        RenderTexture? result = rp.Res!.Render();

        if (result == null)
        {
            EarlyEndRender();

            Application.Logger.Error("RenderPipeline OutputNode failed to return a RenderTexture!");
            return;
        }

        //LightingPass();
        //
        //PostProcessStagePreCombine?.Invoke(gBuffer);
        //
        //if (debugDraw == DebugDraw.Off)
        //    CombinePass();
        //
        //PostProcessStagePostCombine?.Invoke(gBuffer);

        // Draw to Screen
        if (DebugDraw == DebugDrawType.Off)
        {
            Graphics.Blit(Target.Res ?? null, result.InternalTextures[0], DoClear);
            Graphics.BlitDepth(GBuffer.Buffer, Target.Res ?? null);
        }
        else if (DebugDraw == DebugDrawType.Albedo)
            Graphics.Blit(Target.Res ?? null, GBuffer.AlbedoAO, DoClear);
        else if (DebugDraw == DebugDrawType.Normals)
            Graphics.Blit(Target.Res ?? null, GBuffer.NormalMetallic, DoClear);
        else if (DebugDraw == DebugDrawType.Depth)
            Graphics.Blit(Target.Res ?? null, GBuffer.Depth, DoClear);
        else if (DebugDraw == DebugDrawType.Velocity)
            Graphics.Blit(Target.Res ?? null, GBuffer.Velocity, DoClear);
        else if (DebugDraw == DebugDrawType.ObjectID)
            Graphics.Blit(Target.Res ?? null, GBuffer.ObjectIDs, DoClear);

        OldView = Graphics.ViewMatrix;
        OldProjection = Graphics.ProjectionMatrix;

        if (ShowGizmos)
        {
            Target.Res?.Begin();
            if (Graphics.UseJitter)
                Graphics.ProjectionMatrix = RenderingCamera.GetProjectionMatrix(width, height); // Cancel out jitter if there is any
            Gizmos.Render();
            Target.Res?.End();
        }
        
        Gizmos.Clear();

        RenderingCamera = null;
        Graphics.UseJitter = false;
    }
    
    
    private void EarlyEndRender()
    {
        Graphics.UseJitter = false;
        if (ClearType == CameraClearType.SolidColor)
        {
            Target.Res?.Begin();
            Graphics.Clear(ClearColor.R, ClearColor.G, ClearColor.B, ClearColor.A);
            Target.Res?.End();
        }
        RenderingCamera = null;
    }
    
    
    /// <returns>If the provided world position is visible on screen.</returns>
    public bool WorldToScreenPosition(Vector3 worldPosition, out Vector2 screenPos)
    {
        Vector4 clipSpacePosition = new Vector4(worldPosition, 1) * ViewMatrix * GetProjectionMatrix(WindowInfo.ClientWidth, WindowInfo.ClientHeight);
        
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
        Matrix4x4 viewProjection = ViewMatrix * GetProjectionMatrix(WindowInfo.ClientWidth, WindowInfo.ClientHeight);
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
            double length = frustum.Planes[i].Normal.Magnitude;
            frustum.Planes[i].Normal /= length;
            frustum.Planes[i].Distance /= length;
        }
        
        return frustum;
    }
}