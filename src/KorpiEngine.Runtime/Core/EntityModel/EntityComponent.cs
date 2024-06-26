using KorpiEngine.Core.EntityModel.IDs;
using KorpiEngine.Core.EntityModel.SpatialHierarchy;

namespace KorpiEngine.Core.EntityModel;

public abstract class EntityComponent
{
    /// <summary>
    /// The unique identifier of this component.
    /// </summary>
    public readonly ulong InstanceID;

    public Entity Entity { get; private set; } = null!;
    public Transform Transform => Entity.Transform;
    
    public bool Enabled
    {
        get { return _enabled; }
        set
        {
            if (value != _enabled)
            {
                _enabled = value;
                HierarchyStateChanged();
            }
        }
    }

    public bool EnabledInHierarchy => _enabledInHierarchy;
    
    internal bool HasAwoken;
    internal bool HasStarted;

    private bool _enabled = true;
    private bool _enabledInHierarchy = true;


    internal EntityComponent()
    {
        InstanceID = ComponentID.Generate();
    }
    
    
    internal void Bind(Entity entity)
    {
        Entity = entity;
        
        bool isEnabled = _enabled && entity.EnabledInHierarchy;
        _enabledInHierarchy = isEnabled;
    }
    

    internal void HierarchyStateChanged()
    {
        bool newState = _enabled && Entity.EnabledInHierarchy;
        if (newState == _enabledInHierarchy)
            return;
        
        _enabledInHierarchy = newState;
        if (newState)
            OnEnable();
        else
            OnDisable();
    }

    
    internal bool CanDestroy()
    {
        if (!Entity.IsComponentRequired(this, out Type dependentType))
            return true;
        
        Application.Logger.Error($"Can't remove {GetType().Name} because {dependentType.Name} depends on it");
        return false;
    }
    
    
    internal void InternalAwake()
    {
        if (HasAwoken) return;
        HasAwoken = true;
        OnAwake();

        if (EnabledInHierarchy)
            OnEnable();
    }
    
    
    internal void InternalStart()
    {
        if (HasStarted) return;
        HasStarted = true;
        OnStart();
    }


    #region Overrideable behaviour methods

    // NOTE: Calls to these could be wrapped in ExecuteSafe to catch exceptions and trim the stack trace.
    public virtual void OnAwake() { }
    public virtual void OnEnable() { }
    public virtual void OnDisable() { }
    public virtual void OnStart() { }
    public virtual void OnFixedUpdate() { }
    public virtual void OnUpdate() { }
    public virtual void OnLateUpdate() { }
    public virtual void OnPreRender() { }
    public virtual void OnRenderObject() { }
    public virtual void OnPostRender() { }
    public virtual void OnRenderObjectDepth() { }
    public virtual void OnDrawGizmos() { }
    public virtual void OnDestroy() { }

    #endregion
    
    
    /*internal static void ExecuteSafe(Action action)
    {
        try
        {
            action();
        }
        catch (Exception e)
        {
            Application.Logger.Error($"Error: {e.Message} \n StackTrace: {e.StackTrace}");
        }
    }*/
}