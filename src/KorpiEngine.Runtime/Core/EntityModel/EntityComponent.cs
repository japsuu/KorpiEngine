using System.Collections;
using System.Reflection;
using KorpiEngine.Core.EntityModel.Coroutines;
using KorpiEngine.Core.EntityModel.IDs;
using KorpiEngine.Core.EntityModel.SpatialHierarchy;
using KorpiEngine.Core.Rendering.Cameras;

namespace KorpiEngine.Core.EntityModel;

public abstract class EntityComponent
{
    /// <summary>
    /// The unique identifier of this component.
    /// </summary>
    public readonly ulong InstanceID = ComponentID.Generate();

    public Entity Entity { get; private set; } = null!;
    public Transform Transform => Entity.Transform;

    /// <summary>
    /// True if the entity is enabled explicitly, false otherwise.
    /// This value is unaffected by the entity's parent hierarchy.
    /// </summary>
    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (value == _enabled)
                return;
            
            _enabled = value;
            HierarchyStateChanged();
        }
    }

    /// <summary>
    /// True if the entity is enabled and all of its parents are enabled, false otherwise.
    /// </summary>
    public bool EnabledInHierarchy => _enabledInHierarchy;
    
    public virtual ComponentRenderOrder RenderOrder => ComponentRenderOrder.None;

    internal bool HasAwoken;
    internal bool HasStarted;

    private bool _enabled = true;
    private bool _enabledInHierarchy = true;
    private readonly Dictionary<string, Coroutine> _coroutines = new();
    private readonly Dictionary<string, Coroutine> _endOfFrameCoroutines = new();


    #region Creation and destruction

    internal void Bind(Entity entity)
    {
        Entity = entity;

        bool isEnabled = _enabled && entity.EnabledInHierarchy;
        _enabledInHierarchy = isEnabled;
    }


    private void Dispose()
    {
        Entity = null!;
        _coroutines.Clear();
        _endOfFrameCoroutines.Clear();
    }

    #endregion


    #region Internal calls
    
    internal void InternalAwake()
    {
        if (HasAwoken)
            return;
        HasAwoken = true;
        OnAwake();

        if (EnabledInHierarchy)
            OnEnable();
    }


    internal void InternalStart()
    {
        if (HasStarted)
            return;
        HasStarted = true;
        OnStart();
    }


    internal void Update(EntityUpdateStage stage)
    {
        switch (stage)
        {
            case EntityUpdateStage.PreUpdate:
                UpdateCoroutines();
                OnPreUpdate();
                break;
            case EntityUpdateStage.Update:
                OnUpdate();
                break;
            case EntityUpdateStage.PostUpdate:
                OnPostUpdate();
                UpdateEndOfFrameCoroutines();
                break;
            case EntityUpdateStage.PreFixedUpdate:
                OnPreFixedUpdate();
                break;
            case EntityUpdateStage.FixedUpdate:
                OnFixedUpdate();
                break;
            case EntityUpdateStage.PostFixedUpdate:
                OnPostFixedUpdate();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(stage), stage, null);
        }
    }
    
    
    internal void PreRender() => OnPreRender();
    internal void RenderObject() => OnRenderObject();
    internal void PostRender() => OnPostRender();
    internal void RenderObjectDepth() => OnRenderObjectDepth();
    internal void DrawGizmos() => OnDrawGizmos();


    internal void Destroy()
    {
        Enabled = false;
        
        // OnDestroy is only called for components that have previously been active
        if (HasStarted)
            OnDestroy();
        
        Dispose();
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


    internal bool CanBeDestroyed()
    {
        if (!Entity.IsComponentRequired(this, out Type dependentType))
            return true;

        Application.Logger.Error($"Can't remove {GetType().Name} because {dependentType.Name} depends on it");
        return false;
    }

    #endregion


    #region Coroutines


    private void UpdateCoroutines()
    {
        Dictionary<string, Coroutine> tempList = new(_coroutines);
        _coroutines.Clear();
        foreach (KeyValuePair<string, Coroutine> coroutine in tempList)
        {
            coroutine.Value.Run();
            if (coroutine.Value.IsDone)
            {
                if (coroutine.Value.Enumerator.Current is WaitForEndOfFrame)
                    _endOfFrameCoroutines.Add(coroutine.Key, coroutine.Value);
                else
                    _coroutines.Add(coroutine.Key, coroutine.Value);
            }
        }
    }


    private void UpdateEndOfFrameCoroutines()
    {
        Dictionary<string, Coroutine> tempList = new(_endOfFrameCoroutines);
        _endOfFrameCoroutines.Clear();
        foreach (KeyValuePair<string, Coroutine> coroutine in tempList)
        {
            coroutine.Value.Run();
            if (coroutine.Value.IsDone)
            {
                if (coroutine.Value.Enumerator.Current is WaitForEndOfFrame)
                    _endOfFrameCoroutines.Add(coroutine.Key, coroutine.Value);
                else
                    _coroutines.Add(coroutine.Key, coroutine.Value);
            }
        }
    }

    public Coroutine StartCoroutine(string methodName)
    {
        methodName = methodName.Trim();
        MethodInfo? method = GetType().GetMethod(
            methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        if (method == null)
            throw new InvalidOperationException($"Coroutine '{methodName}' couldn't be started, the method doesn't exist!");

        object? invoke = method.Invoke(this, null);

        if (invoke is not IEnumerator enumerator)
            throw new InvalidOperationException($"Coroutine '{methodName}' couldn't be started, the method doesn't return an IEnumerator!");

        Coroutine coroutine = new(enumerator);

        if (coroutine.Enumerator.Current is WaitForEndOfFrame)
            _endOfFrameCoroutines.Add(methodName, coroutine);
        else
            _coroutines.Add(methodName, coroutine);

        return coroutine;
    }


    public void StopAllCoroutines()
    {
        _coroutines.Clear();
        _endOfFrameCoroutines.Clear();
    }


    public void StopCoroutine(string methodName)
    {
        methodName = methodName.Trim();
        _coroutines.Remove(methodName);
        _endOfFrameCoroutines.Remove(methodName);
    }

    #endregion


    #region Overrideable behaviour methods

    // NOTE: Calls to these could be wrapped in ExecuteSafe to catch exceptions and trim the stack trace.
    protected virtual void OnAwake() { }
    protected virtual void OnEnable() { }
    protected virtual void OnDisable() { }
    protected virtual void OnStart() { }
    protected virtual void OnPreFixedUpdate() { }
    protected virtual void OnFixedUpdate() { }
    protected virtual void OnPostFixedUpdate() { }
    protected virtual void OnPreUpdate() { }
    protected virtual void OnUpdate() { }
    protected virtual void OnPostUpdate() { }
    protected virtual void OnPreRender() { }
    protected virtual void OnRenderObject() { }
    protected virtual void OnPostRender() { }
    protected virtual void OnRenderObjectDepth() { }
    protected virtual void OnDrawGizmos() { }
    protected virtual void OnDestroy() { }


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

    #endregion
}