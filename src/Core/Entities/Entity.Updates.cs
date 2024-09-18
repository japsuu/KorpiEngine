namespace KorpiEngine.Entities;

public sealed partial class Entity
{
    internal void EnsureComponentInitialization()
    {
        foreach (EntityComponent component in _components)
        {
            if (!component.HasAwoken)
                component.InternalAwake();

            if (component.HasStarted)
                continue;

            if (component.EnabledInHierarchy)
                component.InternalStart();
        }
    }
    
    
    internal void Update(EntityUpdateStage stage)
    {
        UpdateComponentsRecursive(stage);
        UpdateSystemsRecursive(stage);
    }


    /// <summary>
    /// Propagates system updates downwards in the hierarchy.
    /// </summary>
    private void UpdateSystemsRecursive(EntityUpdateStage stage)
    {
        _systemBuckets.Update(stage);

        if (!HasChildren)
            return;

        foreach (Entity child in _childList)
            child.UpdateSystemsRecursive(stage);
    }


    /// <summary>
    /// Propagates component updates downwards in the hierarchy.
    /// </summary>
    private void UpdateComponentsRecursive(EntityUpdateStage stage)
    {
        foreach (EntityComponent component in _components)
            component.Update(stage);

        if (!HasChildren)
            return;

        foreach (Entity child in _childList)
            child.UpdateComponentsRecursive(stage);
    }
}