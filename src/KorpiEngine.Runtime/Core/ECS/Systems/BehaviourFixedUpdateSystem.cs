using Arch.Core;
using KorpiEngine.Core.SceneManagement;
using KorpiEngine.Core.Scripting;

namespace KorpiEngine.Core.ECS.Systems;

/// <summary>
/// The default system for calling FixedUpdate on <see cref="Behaviour"/>s.
/// </summary>
internal class BehaviourFixedUpdateSystem : NativeSystem
{
    private readonly QueryDescription _desc = new QueryDescription().WithAll<BehaviourComponent>();
    
    public BehaviourFixedUpdateSystem(Scene scene) : base(scene) { }


    protected override void SystemUpdate(in double deltaTime)
    {
        // Call FixedUpdate on all behaviours
        World.Query(in _desc, (ref BehaviourComponent behaviours) => {
            foreach (Behaviour behaviour in behaviours.Behaviours!)
            {
                if (behaviour.IsEnabled)
                    behaviour.InternalFixedUpdate();
            }
        });
    }
}