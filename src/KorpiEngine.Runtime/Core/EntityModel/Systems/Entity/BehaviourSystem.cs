using KorpiEngine.Core.EntityModel.Components;

namespace KorpiEngine.Core.EntityModel.Systems.Entity;

/// <summary>
/// Generic system to update Unity-like behaviors on entities.
/// </summary>
public class BehaviourSystem : EntitySystem<BehaviourComponent>
{
    public override EntityUpdateStage[] UpdateStages => [EntityUpdateStage.Update, EntityUpdateStage.PostUpdate, EntityUpdateStage.Render, EntityUpdateStage.FixedUpdate];
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


    public override void Update(EntityUpdateStage stage)
    {
        foreach (BehaviourComponent c in _components)
        {
            switch (stage)
            {
                case EntityUpdateStage.Update:
                    if (!c.HasBeenStarted)
                    {
                        c.Start();
                        c.HasBeenStarted = true;
                    }
                    c.OnUpdate();
                    break;
                case EntityUpdateStage.PostUpdate:
                    c.LateUpdate();
                    break;
                case EntityUpdateStage.Render:
                    c.OnRenderObject();
                    break;
                case EntityUpdateStage.FixedUpdate:
                    c.FixedUpdate();
                    break;
            }
        }
    }
}