using KorpiEngine.Core.ECS;
using KorpiEngine.Core.Scripting;

namespace KorpiEngine.Core.Rendering;

public class DirectionalLight : Component
{
    internal override Type NativeComponentType => typeof(DirectionalLightComponent);
}