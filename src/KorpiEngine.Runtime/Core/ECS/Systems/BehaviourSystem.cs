using Arch.Core;
using Arch.Core.Extensions;
using KorpiEngine.Core.SceneManagement;
using KorpiEngine.Core.Scripting;
using Entity = Arch.Core.Entity;

namespace KorpiEngine.Core.ECS.Systems;

/// <summary>
/// The default system for updating and managing <see cref="Behaviour"/>s.
/// </summary>
internal class BehaviourSystem : NativeSystem
{
    // Even though this system may seem inefficient, keep in mind that the user will most likely
    // not create thousands (or even hundreds) of entities with behaviours.
    // If they really need to, they might as well use the ECS API directly.
    private readonly QueryDescription _desc = new QueryDescription().WithAll<BehaviourComponent>();
    
    public BehaviourSystem(Scene scene) : base(scene) { }


    protected override void SystemEarlyUpdate(in double deltaTime)
    {
        // Call awake on objects that have just been created or enabled for the first time
        World.Query(in _desc, (ref BehaviourComponent behaviours) => {
            foreach (Behaviour behaviour in behaviours.Behaviours!)
            {
                if (!behaviour.HasBeenInitialized)
                    behaviour.InternalAwake();
            }
        });
        
        // Call start on objects that have just been created or enabled for the first time
        World.Query(in _desc, (ref BehaviourComponent behaviours) => {
            foreach (Behaviour behaviour in behaviours.Behaviours!)
            {
                if (!behaviour.HasBeenInitialized)
                    behaviour.InternalStart();  // The behaviour will set HasBeenInitialized to true
            }
        });
    }


    protected override void SystemUpdate(in double deltaTime)
    {
        // Call Update on all behaviours
        World.Query(in _desc, (ref BehaviourComponent behaviours) => {
            foreach (Behaviour behaviour in behaviours.Behaviours!)
            {
                if (behaviour.IsEnabled)
                    behaviour.InternalUpdate();
            }
        });
    }


    protected override void SystemLateUpdate(in double deltaTime)
    {
        // Call LateUpdate on all behaviours
        World.Query(in _desc, (ref BehaviourComponent behaviours) => {
            foreach (Behaviour behaviour in behaviours.Behaviours!)
            {
                if (behaviour.IsEnabled)
                    behaviour.InternalLateUpdate();
            }
        });
        
        // Check if any behaviours have been destroyed
        World.Query(in _desc, (Entity e, ref BehaviourComponent behaviours) => {
            for (int i = behaviours.Behaviours!.Count - 1; i >= 0; i--)
            {
                if (!behaviours.Behaviours[i].IsAwaitingDestruction)
                    continue;
                
                behaviours.Behaviours[i].InternalDestroy();
                behaviours.Behaviours.RemoveAt(i);
            }
            
            // If all the behaviours have been destroyed, also remove the component
            if (behaviours.Behaviours.Count <= 0)
                e.Remove<BehaviourComponent>();
        });
    }


    public override void Dispose()
    {
        base.Dispose();
        
        // Destroy all behaviours
        World.Query(in _desc, (ref BehaviourComponent behaviours) => {
            foreach (Behaviour behaviour in behaviours.Behaviours!)
                behaviour.InternalDestroy();
        });
    }
}