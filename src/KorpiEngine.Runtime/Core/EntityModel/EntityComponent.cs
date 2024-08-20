using System.Collections;
using System.Reflection;
using KorpiEngine.Core.EntityModel.Coroutines;
using KorpiEngine.Core.EntityModel.IDs;
using KorpiEngine.Core.EntityModel.SpatialHierarchy;
using KorpiEngine.Core.Rendering.Cameras;

namespace KorpiEngine.Core.EntityModel;

public abstract class EntityComponent
{
    public event Action? Destroying;
    
    /// <summary>
    /// The unique identifier of this component.
    /// </summary>
    public readonly int InstanceID = ComponentID.Generate();

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
    private readonly List<Coroutine> _coroutines = [];


    #region Creation and destruction

    internal void Bind(Entity entity)
    {
        Entity = entity;

        bool isEnabled = _enabled && entity.EnabledInHierarchy;
        _enabledInHierarchy = isEnabled;
    }


    private void Cleanup()
    {
        Entity = null!;
        _coroutines.Clear();
    }

    #endregion


    #region Internal calls
    
    internal void InternalAwake()
    {
        if (HasAwoken)
            return;
        HasAwoken = true;
        ExecuteSafe(OnAwake);

        if (EnabledInHierarchy)
            ExecuteSafe(OnEnable);
    }


    internal void InternalStart()
    {
        if (HasStarted)
            return;
        HasStarted = true;
        ExecuteSafe(OnStart);
    }


    internal void Update(EntityUpdateStage stage)
    {
        switch (stage)
        {
            case EntityUpdateStage.PreUpdate:
                UpdateCoroutines(CoroutineUpdateStage.Update);
                ExecuteSafe(OnPreUpdate);
                break;
            case EntityUpdateStage.Update:
                ExecuteSafe(OnUpdate);
                break;
            case EntityUpdateStage.PostUpdate:
                ExecuteSafe(OnPostUpdate);
                break;
            case EntityUpdateStage.PreFixedUpdate:
                ExecuteSafe(OnPreFixedUpdate);
                break;
            case EntityUpdateStage.FixedUpdate:
                ExecuteSafe(OnFixedUpdate);
                break;
            case EntityUpdateStage.PostFixedUpdate:
                ExecuteSafe(OnPostFixedUpdate);
                break;
            case EntityUpdateStage.PostRender:
                UpdateCoroutines(CoroutineUpdateStage.EndOfFrame);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(stage), stage, null);
        }
    }
    
    
    internal void PreRender() => ExecuteSafe(OnPreRender);
    internal void RenderObject() => ExecuteSafe(OnRenderObject);
    internal void PostRender() => ExecuteSafe(OnPostRender);
    internal void RenderObjectDepth() => ExecuteSafe(OnRenderDepth);
    internal void DrawGizmos() => ExecuteSafe(OnDrawGizmos);
    internal void DrawDepthGizmos() => ExecuteSafe(OnDrawDepthGizmos);
    internal void DrawGUI() => ExecuteSafe(OnDrawGUI);


    internal void Destroy()
    {
        Destroying?.Invoke();
        Enabled = false;
        
        ExecuteSafe(OnDestroy);
        
        Cleanup();
    }
    

    internal void HierarchyStateChanged()
    {
        bool newState = _enabled && Entity.EnabledInHierarchy;
        if (newState == _enabledInHierarchy)
            return;

        _enabledInHierarchy = newState;
        if (newState)
            ExecuteSafe(OnEnable);
        else
            ExecuteSafe(OnDisable);
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


    private void UpdateCoroutines(CoroutineUpdateStage stage)
    {
        for (int i = 0; i < _coroutines.Count; i++)
        {
            Coroutine coroutine = _coroutines[i];
            
            coroutine.Run(stage);
            
            // Check if the coroutine is finished.
            if (!coroutine.IsDone)
                continue;
            
            _coroutines.RemoveAt(i);
            i--;
        }
    }
    
    
    public Coroutine StartCoroutine(IEnumerator routine)
    {
        Coroutine coroutine = new(routine);
        _coroutines.Add(coroutine);
        return coroutine;
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

        return StartCoroutine(enumerator);
    }


    public void StopAllCoroutines()
    {
        _coroutines.Clear();
    }


    public void StopCoroutine(Coroutine coroutine)
    {
        _coroutines.Remove(coroutine);
    }

    #endregion


    #region Overrideable behaviour methods

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
    protected virtual void OnRenderDepth() { }
    /// <summary>
    /// Called when Gizmos are drawn.
    /// These gizmos ignore depth and are drawn on top of all scene geometry.
    /// Called even when the component is disabled.
    /// </summary>
    protected virtual void OnDrawGizmos() { }
    /// <summary>
    /// Called when Gizmos are drawn.
    /// These gizmos respect depth and may be occluded by scene geometry.
    /// Called even when the component is disabled.
    /// </summary>
    protected virtual void OnDrawDepthGizmos() { }
    protected virtual void OnDrawGUI() { }
    protected virtual void OnDestroy() { }


    internal static void ExecuteSafe(Action action)
    {
        try
        {
            action();
        }
        catch (Exception e)
        {
            Application.Logger.Error("Caught exception:\n", e);
        }
    }

    #endregion
}