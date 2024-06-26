using KorpiEngine.Core.EntityModel.Components;

namespace KorpiEngine.Core.EntityModel.Systems.Entity;

/// <summary>
/// Generic system to update Unity-like behaviors on entities.
/// </summary>
public class BehaviourSystem : EntitySystem<BehaviourComponent>
{
    public override SystemUpdateStage[] UpdateStages => [SystemUpdateStage.Update, SystemUpdateStage.PostUpdate, SystemUpdateStage.Render, SystemUpdateStage.FixedUpdate];
    public override bool IsSingleton => true;
    
    private readonly List<BehaviourComponent> _components = [];


    protected override void RegisterComponent(BehaviourComponent c)
    {
        _components.Add(c);
        c.Awake();
    }


    protected override void UnregisterComponent(BehaviourComponent c)
    {
        _components.Remove(c);
        c.OnDestroy();
    }


    public override void Update(SystemUpdateStage stage)
    {
        foreach (BehaviourComponent c in _components)
        {
            switch (stage)
            {
                case SystemUpdateStage.Update:
                    if (!c.HasBeenStarted)
                    {
                        c.Start();
                        c.HasBeenStarted = true;
                    }
                    c.OnUpdate();
                    break;
                case SystemUpdateStage.PostUpdate:
                    c.LateUpdate();
                    break;
                case SystemUpdateStage.Render:
                    c.OnRenderObject();
                    break;
                case SystemUpdateStage.FixedUpdate:
                    c.FixedUpdate();
                    break;
            }
        }
    }
}