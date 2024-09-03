namespace KorpiEngine.Rendering.Primitives;

public struct RasterizerState
{
    public bool EnableDepthTest = true;
    public bool EnableDepthWrite = true;
    public DepthMode DepthMode = DepthMode.LessOrEqual;
    
    public bool EnableBlend = true;
    public BlendType BlendSrc = BlendType.SrcAlpha;
    public BlendType BlendDst = BlendType.OneMinusSrcAlpha;
    public BlendMode BlendMode = BlendMode.Add;

    public bool EnableCulling = true;
    public PolyFace FaceCulling = PolyFace.Back;

    public WindingOrder WindingOrder = WindingOrder.CCW;


    public RasterizerState()
    {
    }
}