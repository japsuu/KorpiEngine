using KorpiEngine.Core.API;
using KorpiEngine.Core.API.Rendering;
using KorpiEngine.Core.API.Rendering.Textures;
using KorpiEngine.Core.Internal.AssetManagement;
using KorpiEngine.Core.Rendering.Cameras;

namespace KorpiEngine.Core.ECS;

public struct CameraComponent() : INativeComponent
{
    public AssetRef<RenderTexture> Target;

    public GBuffer? GBuffer;
    
    public short RenderPriority = 0;

    public CameraProjectionType ProjectionType = CameraProjectionType.Perspective;
    
    public CameraRenderTarget RenderTarget = CameraRenderTarget.Screen;
    
    public CameraClearType ClearType = CameraClearType.SolidColor;
    
    public Color ClearColor = Color.Gray;
    
    public float FOVDegrees = 90;
    
    public float NearClip = 0.01f;
    
    public float FarClip = 1000f;

    public float RenderResolution = 1f;
    
    public Matrix4x4? OldView = null;
    
    public Matrix4x4? OldProjection = null;
    
    public DebugDrawType DebugDraw = DebugDrawType.Off;
}