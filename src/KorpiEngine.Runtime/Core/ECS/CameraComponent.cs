using KorpiEngine.Core.Rendering.Cameras;

namespace KorpiEngine.Core.ECS;

public struct CameraComponent : INativeComponent    //TODO: Add cached view/projection matrices for performance.
{
    public short RenderPriority;

    public CameraProjectionType ProjectionType;
    
    public CameraRenderTarget RenderTarget;
    
    public CameraClearType ClearType;
    
    public Color ClearColor;
    
    public float FOVDegrees;


    public CameraComponent()
    {
        RenderPriority = 0;
        ProjectionType = CameraProjectionType.Perspective;
        RenderTarget = CameraRenderTarget.Screen;
        ClearType = CameraClearType.SolidColor;
        ClearColor = Color.White;
        FOVDegrees = 60;
    }
}