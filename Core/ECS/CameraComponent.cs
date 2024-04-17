using KorpiEngine.Core.Rendering.Cameras;
using OpenTK.Mathematics;

namespace KorpiEngine.Core.ECS;

public struct CameraComponent : INativeComponent    //TODO: Add cached view/projection matrices for performance.
{
    public short RenderPriority;
    
    public RenderTarget RenderTarget;
    
    public float FOVRadians;


    public CameraComponent()
    {
        RenderPriority = 0;
        RenderTarget = RenderTarget.Screen;
        FOVRadians = MathHelper.PiOver2;
    }
}