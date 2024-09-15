using KorpiEngine.AssetManagement;
using KorpiEngine.Entities;
using KorpiEngine.Input;
using KorpiEngine.Mathematics;
using KorpiEngine.Tools.Gizmos;
using KorpiEngine.UI.DearImGui;
using KorpiEngine.Utils;

namespace KorpiEngine.Rendering;

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
    
    /// <summary>
    /// The camera that is currently rendering.
    /// </summary>
    internal static Camera RenderingCamera { get; private set; } = null!;
    
    /// <summary>
    /// The camera that last rendered a frame.
    /// May change during rendering.
    /// </summary>
    internal static Camera? LastRenderedCamera { get; private set; }
    
    public event Action<int, int>? Resized;

    private readonly RenderPipeline _pipeline = new();
    private readonly Dictionary<string, (RenderTexture, long frameCreated)> _cachedRenderTextures = [];
    private Material _debugMaterial = null!;
    private Matrix4x4? _oldView;
    private Matrix4x4? _oldProjection;

    /// <summary>
    /// The render priority of this camera.
    /// The Camera with the highest render priority will be rendered last.
    /// </summary>
    public short RenderPriority { get; set; } = 0;
    
    /// <summary>
    /// The render resolution multiplier of this camera.
    /// </summary>
    public float RenderResolution { get; set; } = 1f;

    /// <summary>
    /// The render target texture of this camera.
    /// If set, the camera will render to this texture.
    /// If not set, the camera will render to the screen.
    /// </summary>
    public AssetRef<RenderTexture> TargetTexture { get; set; }
    
    /// <summary>
    /// The G-buffer of this camera.
    /// </summary>
    public GBuffer? GBuffer { get; private set; }

    public CameraProjectionType ProjectionType { get; set; } = CameraProjectionType.Perspective;
    public CameraClearType ClearType { get; set; } = CameraClearType.SolidColor;
    public CameraClearFlags ClearFlags { get; set; } = CameraClearFlags.Color | CameraClearFlags.Depth;
    public ColorHDR ClearColor { get; set; } = ColorHDR.Gray;
    public CameraDebugDrawType DebugDrawType { get; set; } = CameraDebugDrawType.OFF;

    /// <summary>
    /// The field of view (FOV degrees, the vertical angle of the camera view).
    /// </summary>
    public float FOVDegrees { get; set; } = 60;
    public float OrthographicSize { get; set; } = 0.5f;
    
    public float NearClipPlane { get; set; } = 0.5f;
    public float FarClipPlane { get; set; } = 5000f;

    public bool ShowGizmos { get; set; } = true;

    /// <summary>
    /// The view matrix of this camera.
    /// Matrix that transforms from world to camera space.
    /// Does not contain the camera position, since we use camera-relative rendering.
    /// </summary>
    public Matrix4x4 ViewMatrix => Matrix4x4.CreateLookAt(Vector3.Zero, Transform.Forward, Transform.Up);

    /// <summary>
    /// The projection matrix of this camera.
    /// </summary>
    public Matrix4x4 GetProjectionMatrix(float width, float height) => ProjectionType == CameraProjectionType.Orthographic
        ? Matrix4x4.CreateOrthographicOffCenter(-OrthographicSize, OrthographicSize, -OrthographicSize, OrthographicSize, NearClipPlane, FarClipPlane)
        : Matrix4x4.CreatePerspectiveFieldOfView(FOVDegrees.ToRadians(), width / height, NearClipPlane, FarClipPlane);


    internal void Render(int width = -1, int height = -1)
    {
        if (Entity.Scene == null)
            throw new InvalidOperationException("Camera must be attached to an Entity in a Scene to render!");
        
        // Determine render target size
        if (TargetTexture.IsAvailable)
        {
            width = TargetTexture.Asset!.Width;
            height = TargetTexture.Asset!.Height;
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
        LastRenderedCamera = this;
        
        Graphics.ViewMatrix = ViewMatrix;
        Graphics.OldViewMatrix = _oldView ?? Graphics.ViewMatrix;
        if (!Matrix4x4.Invert(Graphics.ViewMatrix, out Matrix4x4 inverseViewMatrix))
            throw new InvalidOperationException("Failed to invert the View Matrix!");
        Graphics.InverseViewMatrix = inverseViewMatrix;
        
        Graphics.ProjectionMatrix = GetProjectionMatrix(width, height);
        Graphics.OldProjectionMatrix = _oldProjection ?? Graphics.ProjectionMatrix;
        if (!Matrix4x4.Invert(Graphics.ProjectionMatrix, out Matrix4x4 inverseProjectionMatrix))
            throw new InvalidOperationException("Failed to invert the Projection Matrix!");
        Graphics.InverseProjectionMatrix = inverseProjectionMatrix;
        
        _pipeline.Prepare(width, height);
        
        // Render all meshes
        if (DebugDrawType == CameraDebugDrawType.WIREFRAME)
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
        if (DebugDrawType == CameraDebugDrawType.OFF)
        {
            Graphics.Blit(TargetTexture.Asset ?? null, result.MainTexture, doClear);
            Graphics.BlitDepth(GBuffer!.Buffer, TargetTexture.Asset ?? null);
        }
        else
        {
            _debugMaterial.SetTexture("_GAlbedoAO", GBuffer!.AlbedoAO);
            _debugMaterial.SetTexture("_GNormalMetallic", GBuffer!.NormalMetallic);
            _debugMaterial.SetTexture("_GPositionRoughness", GBuffer!.PositionRoughness);
            _debugMaterial.SetTexture("_GEmission", GBuffer!.Emission);
            _debugMaterial.SetTexture("_GVelocity", GBuffer!.Velocity);
            _debugMaterial.SetTexture("_GObjectID", GBuffer!.ObjectIDs);
            _debugMaterial.SetTexture("_GDepth", GBuffer!.Depth!);
            _debugMaterial.SetTexture("_GUnlit", GBuffer!.Unlit);
            
            _debugMaterial.SetFloat("_CameraNearClip", NearClipPlane);
            _debugMaterial.SetFloat("_CameraFarClip", FarClipPlane);
            
            Graphics.Blit(TargetTexture.Asset ?? null, _debugMaterial, 0, doClear);
        }
        
        _oldView = Graphics.ViewMatrix;
        _oldProjection = Graphics.ProjectionMatrix;
        
        RenderingCamera = null!;
        Graphics.UseJitter = false;
    }
    
    
    internal void RenderLights() => Entity.Scene!.EntityScene.InvokeRenderLighting();
    internal void RenderDepthGeometry() => Entity.Scene!.EntityScene.InvokeRenderGeometryDepth();
    private void RenderGeometry() => Entity.Scene!.EntityScene.InvokeRenderGeometry();


    private void RenderGizmos()
    {
        // if (Graphics.UseJitter)
        //     Graphics.ProjectionMatrix = RenderingCamera.GetProjectionMatrix(width, height); // Cancel out jitter
        Entity.Scene!.EntityScene.InvokeDrawDepthGizmos();
        Gizmos.Render(true);
        Gizmos.Clear();
        
        Entity.Scene.EntityScene.InvokeDrawGizmos();
        Gizmos.Render(false);
        Gizmos.Clear();
    }


    private void GeometryPass()
    {
        Entity.Scene!.EntityScene.InvokePreRender();
        
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
        Graphics.Device.SetWireframeMode(true);

        // Render all meshes in wireframe mode
        GeometryPass();

        // Reset the wireframe rendering mode
        Graphics.Device.SetWireframeMode(false);
    }
    
    
    private Vector2 GetRenderTargetSize()
    {
        if (TargetTexture.IsAvailable)
            return new Vector2(TargetTexture.Asset!.Width, TargetTexture.Asset!.Height);
        
        return new Vector2(Graphics.Window.FramebufferSize.X, Graphics.Window.FramebufferSize.Y);
    }

    
    private void CheckGBuffer()
    {
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
            TargetTexture.Asset?.Begin();
            
            ClearColor.Deconstruct(out float r, out float g, out float b, out float a);
            bool clearColor = ClearFlags.HasFlagFast(CameraClearFlags.Color);
            bool clearDepth = ClearFlags.HasFlagFast(CameraClearFlags.Depth);
            bool clearStencil = ClearFlags.HasFlagFast(CameraClearFlags.Stencil);
            Graphics.Clear(r, g, b, a, clearColor, clearDepth, clearStencil);
            
            TargetTexture.Asset?.End();
        }
        
        RenderingCamera = null!;
    }


    protected override void OnEnable()
    {
        _debugMaterial = new Material(Shader.Find("Assets/Defaults/GBufferDebug.kshader"), "g buffer debug material", false);
    }
    
    
    protected override void OnStart()
    {
#if TOOLS
        ImGuiWindowManager.RegisterWindow(new CameraEditor(this));
#endif
    }


    protected override void OnUpdate()
    {
        if (!Input.Input.GetKeyDown(KeyCode.F1))
            return;

        SetDebugDrawType(DebugDrawType.Next());
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
        
        _debugMaterial.DestroyImmediate();
        _debugMaterial = null!;
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
        Vector4 clipSpacePosition = new Vector4(worldPosition, 1f) * ViewMatrix * GetProjectionMatrix(WindowInfo.ClientWidth, WindowInfo.ClientHeight);

        // Without this, the coordinates are visible even when looking straight away from them.
        if (clipSpacePosition.W <= 0)
        {
            screenPos = Vector2.NegativeInfinity;
            return false;
        }

        Vector3 normalizedDeviceCoordinates = clipSpacePosition.XYZ / clipSpacePosition.W;
        Vector2 screenCoordinates = new(normalizedDeviceCoordinates.X, -normalizedDeviceCoordinates.Y);
        screenCoordinates += Vector2.One;
        screenCoordinates /= 2;
        screenCoordinates *= WindowInfo.ClientSize;
        screenPos = screenCoordinates;
        return true;
    }


    public BoundingFrustum CalculateFrustum()
    {
        Matrix4x4 viewProjection = ViewMatrix * GetProjectionMatrix(WindowInfo.ClientWidth, WindowInfo.ClientHeight);

        return new BoundingFrustum(viewProjection);
    }

    #endregion


    internal void SetDebugDrawType(CameraDebugDrawType newType)
    {
        _debugMaterial.SetKeyword(DebugDrawType.AsShaderKeyword(), false);
        DebugDrawType = newType;
        _debugMaterial.SetKeyword(DebugDrawType.AsShaderKeyword(), true);
        
        Console.WriteLine($"Camera Debug Draw Type: {DebugDrawType.AsShaderKeyword()}");
    }
}