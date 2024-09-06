namespace KorpiEngine.Entities;

/// <summary>
/// A collection of system buckets grouped by update stage.
/// </summary>
internal class SystemBucketCollection
{
    private readonly Dictionary<EntityUpdateStage, SystemBucket> _buckets = [];
    
    
    public void AddSystem(ulong id, IEntitySystem system)
    {
        EntityUpdateStage[] stages = system.UpdateStages;
        
        foreach (EntityUpdateStage stage in stages)
        {
            if (!_buckets.ContainsKey(stage))
                _buckets.Add(stage, new SystemBucket());

            _buckets[stage].AddSystem(id, system);
        }
    }


    public bool RemoveSystem(ulong id)
    {
        bool removed = false;
        
        foreach (SystemBucket bucket in _buckets.Values)
            removed |= bucket.RemoveSystem(id);
        
        return removed;
    }
    
    
    public void Update(EntityUpdateStage stage)
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
    private readonly Dictionary<ulong, IEntitySystem> _systems = [];
    
    
    public void AddSystem(ulong id, IEntitySystem system)
    {
        _systems.Add(id, system);
    }
    
    
    public bool RemoveSystem(ulong id)
    {
        return _systems.Remove(id);
    }
    
    
    public void Update(EntityUpdateStage stage)
    {
        foreach (IEntitySystem system in _systems.Values)
            system.Update(stage);
    }
}