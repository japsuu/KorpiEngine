using KorpiEngine.Core.ECS;

namespace KorpiEngine.Core.Scripting.Components;

public class SkinnedMeshRenderer : Component
{
    internal override Type NativeComponentType => typeof(SkinnedMeshRendererComponent);
}