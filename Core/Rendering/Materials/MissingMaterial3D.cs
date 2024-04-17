namespace KorpiEngine.Core.Rendering.Materials;

public class MissingMaterial3D : BaseMaterial3D
{
    protected override void SetMaterialPropertyDefaults()
    {
        base.SetMaterialPropertyDefaults();
        
        Color = Color.Magenta;
    }
}