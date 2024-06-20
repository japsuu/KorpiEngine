using KorpiEngine.Core.EntityModel.IDs;

namespace KorpiEngine.Core.EntityModel;

/// <summary>
/// A collection of system buckets grouped by update stage.
/// </summary>
internal class SystemBucketCollection
{
    private readonly Dictionary<SystemUpdateStage, SystemBucket> _buckets = [];
    
    
    public void AddSystem(EntitySystemID id, IEntitySystem system)
    {
        SystemUpdateStage[] stages = system.UpdateStages;
        
        foreach (SystemUpdateStage stage in stages)
        {
            if (!_buckets.ContainsKey(stage))
                _buckets.Add(stage, new SystemBucket());

            _buckets[stage].AddSystem(id, system);
        }
    }


    public bool RemoveSystem(EntitySystemID id)
    {
        bool removed = false;
        
        foreach (SystemBucket bucket in _buckets.Values)
            removed |= bucket.RemoveSystem(id);
        
        return removed;
    }
    
    
    public void Update(SystemUpdateStage stage)
    {
        if (!_buckets.TryGetValue(stage, out SystemBucket? bucket))
            return;
        
        bucket.Update(stage);
    }
    
    
    public void Clear()
    {
        _buckets.Clear();
    }
}

/// <summary>
/// A collection of systems that are updated at the same time.
/// </summary>
internal class SystemBucket
{
    private readonly Dictionary<EntitySystemID, IEntitySystem> _systems = [];
    
    
    public void AddSystem(EntitySystemID id, IEntitySystem system)
    {
        _systems.Add(id, system);
    }
    
    
    public bool RemoveSystem(EntitySystemID id)
    {
        return _systems.Remove(id);
    }
    
    
    public void Update(SystemUpdateStage stage)
    {
        foreach (IEntitySystem system in _systems.Values)
            system.Update(stage);
    }
}