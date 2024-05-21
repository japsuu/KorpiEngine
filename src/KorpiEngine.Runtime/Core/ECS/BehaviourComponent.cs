using KorpiEngine.Core.Scripting;

namespace KorpiEngine.Core.ECS;

/// <summary>
/// Contains a list of attached <see cref="Behaviour"/>s.
/// </summary>
public struct BehaviourComponent : INativeComponent
{
    public List<Behaviour>? Behaviours;
}