using ImGuiNET;
using KorpiEngine.Core.API;
using KorpiEngine.Core.API.InputManagement;
using KorpiEngine.Core.API.Rendering.Materials;
using KorpiEngine.Core.API.Rendering.Shaders;
using KorpiEngine.Core.EntityModel;
using KorpiEngine.Core.Internal.AssetManagement;
using KorpiEngine.Core.Platform;
using KorpiEngine.Core.Rendering.Pipeline;
using KorpiEngine.Core.Rendering.Primitives;
using KorpiEngine.Core.UI.DearImGui;

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
    public ResourceRef<RenderTexture> TargetTexture { get; set; }
    
    /// <summary>
    /// The G-buffer of this camera.
    /// </summary>
    public GBuffer? GBuffer { get; private set; }

    public CameraProjectionType ProjectionType { get; set; } = CameraProjectionType.Perspective;
    public CameraClearType ClearType { get; set; } = CameraClearType.SolidColor;
    public CameraClearFlags ClearFlags { get; set; } = CameraClearFlags.Color | CameraClearFlags.Depth;
    public Color ClearColor { get; set; } = Color.Gray;
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
            Graphics.Blit(TargetTexture.Res ?? null, result.InternalTextures[0], doClear);
            Graphics.BlitDepth(GBuffer!.Buffer, TargetTexture.Res ?? null);
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
            
            Graphics.Blit(TargetTexture.Res ?? null, _debugMaterial, 0, doClear);
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
        Graphics.Device.SetWireframeMode(true);

        // Render all meshes in wireframe mode
        GeometryPass();

        // Reset the wireframe rendering mode
        Graphics.Device.SetWireframeMode(false);
    }
    
    
    private Vector2 GetRenderTargetSize()
    {
        if (TargetTexture.IsAvailable)
            return new Vector2(TargetTexture.Res!.Width, TargetTexture.Res!.Height);
        
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


    protected override void OnEnable()
    {
        _debugMaterial = new Material(Shader.Find("Defaults/GBufferDebug.kshader"), "g buffer debug material", false);
    }
    
    
    protected override void OnStart()
    {
        ImGuiWindowManager.RegisterWindow(new CameraEditor(this));
    }


    protected override void OnUpdate()
    {
        if (!Input.GetKeyDown(KeyCode.F1))
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


    internal void SetDebugDrawType(CameraDebugDrawType newType)
    {
        _debugMaterial.SetKeyword(DebugDrawType.AsShaderKeyword(), false);
        DebugDrawType = newType;
        _debugMaterial.SetKeyword(DebugDrawType.AsShaderKeyword(), true);
        
        Console.WriteLine($"Camera Debug Draw Type: {DebugDrawType.AsShaderKeyword()}");
    }
}

internal class CameraEditor(Camera target) : EntityComponentEditor(target)
{
    protected override void DrawEditor()
    {
        ImGui.Text("Camera Settings");

        int renderPriority = target.RenderPriority;
        if (ImGui.DragInt("Render Priority", ref renderPriority, 1, short.MinValue, short.MaxValue))
            target.RenderPriority = (short)renderPriority;

        float renderResolution = target.RenderResolution;
        if (ImGui.DragFloat("Render Resolution", ref renderResolution, 0.1f, 0.1f, 10f))
            target.RenderResolution = renderResolution;

        DrawProjectionSettings();

        DrawClearSettings();

        DrawDebugSettings();
    }


    private void DrawProjectionSettings()
    {
        ImGui.Spacing();
        ImGui.Text("Projection Settings");
        if (ImGui.BeginCombo("Projection Type", target.ProjectionType.ToString()))
        {
            foreach (CameraProjectionType type in Enum.GetValues<CameraProjectionType>())
            {
                if (ImGui.Selectable(type.ToString(), target.ProjectionType == type))
                    target.ProjectionType = type;
            }
            ImGui.EndCombo();
        }

        float fovDegrees = target.FOVDegrees;
        if (ImGui.DragFloat("FOV Degrees", ref fovDegrees, 1f, 1f, 179f))
            target.FOVDegrees = fovDegrees;

        float orthographicSize = target.OrthographicSize;
        if (ImGui.DragFloat("Orthographic Size", ref orthographicSize, 0.1f, 0.1f, 100f))
            target.OrthographicSize = orthographicSize;

        float nearClipPlane = target.NearClipPlane;
        if (ImGui.DragFloat("Near Clip Plane", ref nearClipPlane, 0.01f, 0.01f, 100f))
            target.NearClipPlane = nearClipPlane;

        float farClipPlane = target.FarClipPlane;
        if (ImGui.DragFloat("Far Clip Plane", ref farClipPlane, 1f, 1f, 10000f))
            target.FarClipPlane = farClipPlane;
    }


    private void DrawClearSettings()
    {
        ImGui.Spacing();
        ImGui.Text("Clear Settings");
        if (ImGui.BeginCombo("Clear Type", target.ClearType.ToString()))
        {
            foreach (CameraClearType type in Enum.GetValues<CameraClearType>())
            {
                if (ImGui.Selectable(type.ToString(), target.ClearType == type))
                    target.ClearType = type;
            }
            ImGui.EndCombo();
        }

        ImGui.Text("Clear Flags");
        int clearFlags = (int)target.ClearFlags;
        if (ImGui.CheckboxFlags("Color", ref clearFlags, (int)CameraClearFlags.Color))
            target.ClearFlags = (CameraClearFlags)clearFlags;
        if (ImGui.CheckboxFlags("Depth", ref clearFlags, (int)CameraClearFlags.Depth))
            target.ClearFlags = (CameraClearFlags)clearFlags;
        if (ImGui.CheckboxFlags("Stencil", ref clearFlags, (int)CameraClearFlags.Stencil))
            target.ClearFlags = (CameraClearFlags)clearFlags;

        System.Numerics.Vector4 clearColor = new(target.ClearColor.R, target.ClearColor.G, target.ClearColor.B, target.ClearColor.A);
        if (ImGui.ColorEdit4("Clear Color", ref clearColor))
            target.ClearColor = new Color(clearColor);
    }


    private void DrawDebugSettings()
    {
        ImGui.Spacing();
        ImGui.Text("Debug Settings");
        
        if (ImGui.BeginCombo("Debug Draw Type", target.DebugDrawType.ToString()))
        {
            foreach (CameraDebugDrawType type in Enum.GetValues<CameraDebugDrawType>())
            {
                if (ImGui.Selectable(type.ToString(), target.DebugDrawType == type))
                    target.SetDebugDrawType(type);
            }
            ImGui.EndCombo();
        }
        
        bool showGizmos = target.ShowGizmos;
        if (ImGui.Checkbox("Show Gizmos", ref showGizmos))
            target.ShowGizmos = showGizmos;
    }
}