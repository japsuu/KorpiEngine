using KorpiEngine.Core.API;
using KorpiEngine.Core.API.InputManagement;
using KorpiEngine.Core.EntityModel;
using KorpiEngine.Core.Internal.AssetManagement;
using KorpiEngine.Core.Platform;
using KorpiEngine.Core.Rendering.Pipeline;
using KorpiEngine.Core.Rendering.Primitives;

namespace KorpiEngine.Core.Rendering.Cameras;

/// <summary>
/// Camera component that can be attached to an entity to render the scene from its perspective.
///
/// The rendering system currently uses a camera-relative rendering system.
/// The camera-relative rendering process translates Entities and Lights by the negated world space Camera position,
/// before any other geometric transformations affect them.
/// It then sets the world space Camera position to 0,0,0 and modifies all relevant matrices accordingly.
/// </summary>
public sealed class Camera : EntityComponent
{
    private const int RENDER_TEXTURE_MAX_UNUSED_FRAMES = 10;
    
    internal static Camera RenderingCamera { get; private set; } = null!;
    
    public event Action<int, int>? Resized;

    private readonly RenderPipeline _pipeline = new();
    private readonly Dictionary<string, (RenderTexture, long frameCreated)> _cachedRenderTextures = [];
    private Matrix4x4? _oldView;
    private Matrix4x4? _oldProjection;

    /// <summary>
    /// The render priority of this camera.
    /// The Camera with the highest render priority will be rendered last.
    /// </summary>
    public short RenderPriority = 0;
    
    /// <summary>
    /// The render resolution multiplier of this camera.
    /// </summary>
    public float RenderResolution = 1f;

    /// <summary>
    /// The render target texture of this camera.
    /// If set, the camera will render to this texture.
    /// If not set, the camera will render to the screen.
    /// </summary>
    public ResourceRef<RenderTexture> TargetTexture;
    
    /// <summary>
    /// The G-buffer of this camera.
    /// </summary>
    public GBuffer? GBuffer { get; private set; }

    public CameraProjectionType ProjectionType = CameraProjectionType.Perspective;
    public CameraClearType ClearType = CameraClearType.SolidColor;
    public CameraClearFlags ClearFlags = CameraClearFlags.Color | CameraClearFlags.Depth;
    public Color ClearColor = Color.Gray;
    public CameraDebugDrawType DebugDrawType = CameraDebugDrawType.Off;

    /// <summary>
    /// The field of view (FOV degrees, the vertical angle of the camera view).
    /// </summary>
    public float FOVDegrees = 60;
    public float OrthographicSize = 0.5f;
    
    public float NearClipPlane = 0.01f;
    public float FarClipPlane = 1000f;

    public bool ShowGizmos = true;

    /// <summary>
    /// The view matrix of this camera.
    /// Matrix that transforms from world to camera space.
    /// Does not contain the camera position, since we use camera-relative rendering.
    /// </summary>
    public Matrix4x4 ViewMatrix => Matrix4x4.CreateLookToLeftHanded(Vector3.Zero, Transform.Forward, Transform.Up);

    /// <summary>
    /// The projection matrix of this camera.
    /// </summary>
    public Matrix4x4 GetProjectionMatrix(float width, float height) => ProjectionType == CameraProjectionType.Orthographic
        ? System.Numerics.Matrix4x4.CreateOrthographicOffCenterLeftHanded(-OrthographicSize, OrthographicSize, -OrthographicSize, OrthographicSize, NearClipPlane, FarClipPlane).ToDouble()
        : System.Numerics.Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded(FOVDegrees.ToRad(), width / height, NearClipPlane, FarClipPlane).ToDouble();


    internal void Render(int width = -1, int height = -1)
    {
        // Determine render target size
        if (TargetTexture.IsAvailable)
        {
            width = TargetTexture.Res!.Width;
            height = TargetTexture.Res!.Height;
        }
        else if (width == -1 || height == -1)
        {
            width = Graphics.Window.FramebufferSize.X;
            height = Graphics.Window.FramebufferSize.Y;
        }

        width = (int)(width * RenderResolution);
        height = (int)(height * RenderResolution);

        CheckGBuffer();
        
        // Use the current view and projection matrices
        RenderingCamera = this;
        
        Graphics.ViewMatrix = ViewMatrix;
        Graphics.ProjectionMatrix = GetProjectionMatrix(width, height);
        Graphics.OldViewMatrix = _oldView ?? Graphics.ViewMatrix;
        Graphics.OldProjectionMatrix = _oldProjection ?? Graphics.ProjectionMatrix;
        Matrix4x4.Invert(Graphics.ProjectionMatrix, out Graphics.InverseProjectionMatrix);
        
        _pipeline.Prepare(width, height);
        
        // Render all meshes
        if (DebugDrawType == CameraDebugDrawType.Wireframe)
            GeometryPassWireframe();
        else
            GeometryPass();
        
        RenderTexture? result = _pipeline.Render();

        if (result == null)
        {
            EarlyEndRender();

            Application.Logger.Error("RenderPipeline failed to return a RenderTexture!");
            return;
        }
        
        // Draw to Screen
        bool doClear = ClearType == CameraClearType.SolidColor;
        switch (DebugDrawType)
        {
            case CameraDebugDrawType.Off:
                Graphics.Blit(TargetTexture.Res ?? null, result.InternalTextures[0], doClear);
                Graphics.BlitDepth(GBuffer!.Buffer, TargetTexture.Res ?? null);
                break;
            case CameraDebugDrawType.Albedo:
                Graphics.Blit(TargetTexture.Res ?? null, GBuffer!.AlbedoAO, doClear);
                break;
            case CameraDebugDrawType.Normals:
                Graphics.Blit(TargetTexture.Res ?? null, GBuffer!.NormalMetallic, doClear);
                break;
            case CameraDebugDrawType.Position:
                Graphics.Blit(TargetTexture.Res ?? null, GBuffer!.PositionRoughness, doClear);
                break;
            case CameraDebugDrawType.Emission:
                Graphics.Blit(TargetTexture.Res ?? null, GBuffer!.Emission, doClear);
                break;
            case CameraDebugDrawType.Depth:
                Graphics.Blit(TargetTexture.Res ?? null, GBuffer!.Depth!, doClear);
                break;
            case CameraDebugDrawType.Velocity:
                Graphics.Blit(TargetTexture.Res ?? null, GBuffer!.Velocity, doClear);
                break;
            case CameraDebugDrawType.Unlit:
                Graphics.Blit(TargetTexture.Res ?? null, GBuffer!.Unlit, doClear);
                break;
            case CameraDebugDrawType.ObjectID:
            case CameraDebugDrawType.Wireframe: // Hack: Wireframe uses the ObjectID buffer to color the wireframe red
                Graphics.Blit(TargetTexture.Res ?? null, GBuffer!.ObjectIDs, doClear);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        _oldView = Graphics.ViewMatrix;
        _oldProjection = Graphics.ProjectionMatrix;
        
        RenderingCamera = null!;
        Graphics.UseJitter = false;
    }
    
    
    internal void RenderLights() => Entity.Scene.EntityScene.InvokeRenderLighting();
    internal void RenderDepthGeometry() => Entity.Scene.EntityScene.InvokeRenderGeometryDepth();
    
    private void RenderGeometry() => Entity.Scene.EntityScene.InvokeRenderGeometry();


    private void RenderGizmos()
    {
        // if (Graphics.UseJitter)
        //     Graphics.ProjectionMatrix = RenderingCamera.GetProjectionMatrix(width, height); // Cancel out jitter
        Entity.Scene.EntityScene.InvokeDrawDepthGizmos();
        Gizmos.Render(true);
        Gizmos.Clear();
        
        Entity.Scene.EntityScene.InvokeDrawGizmos();
        Gizmos.Render(false);
        Gizmos.Clear();
    }


    private void GeometryPass()
    {
        Entity.Scene.EntityScene.InvokePreRender();
        
        GBuffer!.Begin();
        
        RenderGeometry();
        
        if (ShowGizmos)
            RenderGizmos();
        
        GBuffer.End();
        
        Entity.Scene.EntityScene.InvokePostRender();
    }
    
    
    private void GeometryPassWireframe()
    {
        // Set the wireframe rendering mode
        Graphics.Driver.SetWireframeMode(true);

        // Render all meshes in wireframe mode
        GeometryPass();

        // Reset the wireframe rendering mode
        Graphics.Driver.SetWireframeMode(false);
    }
    
    
    private Vector2 GetRenderTargetSize()
    {
        if (TargetTexture.IsAvailable)
            return new Vector2(TargetTexture.Res!.Width, TargetTexture.Res!.Height);
        
        return new Vector2(Graphics.Window.FramebufferSize.X, Graphics.Window.FramebufferSize.Y);
    }

    
    private void CheckGBuffer()
    {
        // RenderResolution = Math.Clamp(RenderResolution, 0.1f, 4.0f);

        Vector2 renderSize = GetRenderTargetSize() * RenderResolution;
        
        if (GBuffer == null)
        {
            GBuffer = new GBuffer((int)renderSize.X, (int)renderSize.Y);
            Resized?.Invoke(GBuffer.Width, GBuffer.Height);
        }
        else if (GBuffer.Width != (int)renderSize.X || GBuffer.Height != (int)renderSize.Y)
        {
            GBuffer.UnloadGBuffer();
            GBuffer = new GBuffer((int)renderSize.X, (int)renderSize.Y);
            Resized?.Invoke(GBuffer.Width, GBuffer.Height);
        }
    }

    private void EarlyEndRender()
    {
        Graphics.UseJitter = false;
        
        // Clear the screen
        if (ClearType == CameraClearType.SolidColor)
        {
            TargetTexture.Res?.Begin();
            
            ClearColor.Deconstruct(out float r, out float g, out float b, out float a);
            bool clearColor = ClearFlags.HasFlag(CameraClearFlags.Color);
            bool clearDepth = ClearFlags.HasFlag(CameraClearFlags.Depth);
            bool clearStencil = ClearFlags.HasFlag(CameraClearFlags.Stencil);
            Graphics.Clear(r, g, b, a, clearColor, clearDepth, clearStencil);
            
            TargetTexture.Res?.End();
        }
        
        RenderingCamera = null!;
    }


    protected override void OnUpdate()
    {
        if (!Input.GetKeyDown(KeyCode.F1))
            return;
        
        DebugDrawType = DebugDrawType switch
        {
            CameraDebugDrawType.Off => CameraDebugDrawType.Albedo,
            CameraDebugDrawType.Albedo => CameraDebugDrawType.Normals,
            CameraDebugDrawType.Normals => CameraDebugDrawType.Position,
            CameraDebugDrawType.Position => CameraDebugDrawType.Emission,
            CameraDebugDrawType.Emission => CameraDebugDrawType.Depth,
            CameraDebugDrawType.Depth => CameraDebugDrawType.Velocity,
            CameraDebugDrawType.Velocity => CameraDebugDrawType.Unlit,
            CameraDebugDrawType.Unlit => CameraDebugDrawType.ObjectID,
            CameraDebugDrawType.ObjectID => CameraDebugDrawType.Wireframe,
            CameraDebugDrawType.Wireframe => CameraDebugDrawType.Off,
            _ => throw new ArgumentOutOfRangeException()
        };
        Console.WriteLine($"Debug Draw Type: {DebugDrawType}");
    }


    protected override void OnPostUpdate()
    {
        UpdateCachedRT();
    }
    

    protected override void OnDisable()
    {
        GBuffer?.UnloadGBuffer();

        // Clear the Cached RenderTextures
        foreach (var (renderTexture, _) in _cachedRenderTextures.Values)
            renderTexture.Destroy();
        
        _cachedRenderTextures.Clear();
    }

    
    #region RT Cache
    
    public RenderTexture GetCachedRT(string name, int width, int height, TextureImageFormat[] format)
    {
        if (_cachedRenderTextures.ContainsKey(name))
        {
            // Update the frame created
            (RenderTexture, long frameCreated) cached = _cachedRenderTextures[name];
            _cachedRenderTextures[name] = (cached.Item1, Time.TotalFrameCount);
            return cached.Item1;
        }
        RenderTexture rt = new(width, height, 1, false, format);
        rt.Name = name;
        _cachedRenderTextures[name] = (rt, Time.TotalFrameCount);
        return rt;
    }
    

    public void UpdateCachedRT()
    {
        List<(RenderTexture, string)> disposableTextures = [];
        foreach (var (name, (renderTexture, frameCreated)) in _cachedRenderTextures)
            if (Time.TotalFrameCount - frameCreated > RENDER_TEXTURE_MAX_UNUSED_FRAMES)
                disposableTextures.Add((renderTexture, name));

        foreach ((RenderTexture, string) renderTexture in disposableTextures)
        {
            _cachedRenderTextures.Remove(renderTexture.Item2);
            renderTexture.Item1.Destroy();
        }
    }

    #endregion


    #region Public utility methods

    /// <returns>If the provided world position is visible on screen.</returns>
    public bool WorldToScreenPosition(Vector3 worldPosition, out Vector2 screenPos)
    {
        Vector4 clipSpacePosition = new Vector4(worldPosition, 1) * ViewMatrix * GetProjectionMatrix(WindowInfo.ClientWidth, WindowInfo.ClientHeight);

        // Without this, the coordinates are visible even when looking straight away from them.
        if (clipSpacePosition.W <= 0)
        {
            screenPos = Vector2.NegativeInfinity;
            return false;
        }

        Vector3 normalizedDeviceCoordinates = clipSpacePosition.Xyz / clipSpacePosition.W;
        Vector2 screenCoordinates = new(normalizedDeviceCoordinates.X, -normalizedDeviceCoordinates.Y);
        screenCoordinates += Vector2.One;
        screenCoordinates /= 2;
        screenCoordinates.X *= WindowInfo.ClientWidth;
        screenCoordinates.Y *= WindowInfo.ClientHeight;
        screenPos = screenCoordinates;
        return true;
    }


    public Frustum CalculateFrustum()
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

    #endregion
}