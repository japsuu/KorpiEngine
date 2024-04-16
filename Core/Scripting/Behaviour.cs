using System.ComponentModel;
using Arch.Core.Extensions;
using KorpiEngine.Core.ECS;

namespace KorpiEngine.Core.Scripting;

/// <summary>
/// A behaviour component that can be dynamically attached and removed from an <see cref="Entity"/> at runtime, that receives updates.
/// </summary>
public class Behaviour : Component
{
    /// <summary>
    /// Whether this behaviour is enabled.
    /// Only enabled behaviours receive updates.
    /// </summary>
    public bool IsEnabled { get; private set; }
    public bool IsAwaitingDestruction { get; private set; }

    /// <summary>
    /// True if Awake and Start have been called.
    /// </summary>
    internal bool HasBeenInitialized { get; private set; }  //TODO: Use this to call Awake & Start only once


    internal override Type NativeComponentType => typeof(BehaviourComponent);
    
    
    /// <summary>
    /// Do NOT create components directly!
    /// Use the <see cref="Entity.AddComponent{T}"/> method instead.<br/>
    /// If internally called, this constructor will not throw an exception.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
    protected Behaviour()
    {
        //TODO: Warn the user about not manually creating components
        //throw new InvalidOperationException("Do NOT create components directly! Use the Entity.AddComponent<T> method instead.");
    }


    #region PUBLIC API

    public void Enable()
    {
        if (IsEnabled)
            return;
        
        IsEnabled = true;
        OnEnable();
    }
    
    
    public void Disable()
    {
        if (!IsEnabled)
            return;
        
        IsEnabled = false;
        OnDisable();
    }
    
    
    /// <summary>
    /// Destroys this behaviour.
    /// This will set <see cref="IsAwaitingDestruction"/> to true.
    /// The behaviour will be destroyed at the end of this frame.
    /// </summary>
    public void Destroy()
    {
        IsAwaitingDestruction = true;
    }

    #endregion


    #region VIRTUAL METHODS

    /// <summary>
    /// Called instantly after this behaviour is created.
    /// Use this instead of the constructor to initialize the behaviour.
    /// Only ever called once.<br/>
    /// Guaranteed to be called on all currently existing objects before <see cref="OnStart"/> is called.
    /// </summary>
    protected virtual void OnAwake()
    {
    }
    
    
    /// <summary>
    /// Called before the first frame update.
    /// Only ever called once.<br/>
    /// Guaranteed to be called on all currently existing objects before the first <see cref="OnUpdate"/>.
    /// </summary>
    protected virtual void OnStart()
    {
    }
    
    
    /// <summary>
    /// Called every time this behaviour is enabled.
    /// </summary>
    protected virtual void OnEnable()
    {
    }
    
    
    /// <summary>
    /// Called every time this behaviour is disabled.
    /// </summary>
    protected virtual void OnDisable()
    {
    }
    
    
    /// <summary>
    /// Called every frame.
    /// </summary>
    protected virtual void OnUpdate()
    {
    }
    
    
    /// <summary>
    /// Called every fixed update frame (see <see cref="EngineConstants.FIXED_DELTA_TIME"/>).
    /// </summary>
    protected virtual void OnFixedUpdate()
    {
    }
    
    
    /// <summary>
    /// Called every frame after <see cref="OnUpdate"/>.
    /// </summary>
    protected virtual void OnLateUpdate()
    {
    }
    
    
    /// <summary>
    /// Called before the behaviour is destroyed.
    /// </summary>
    protected virtual void OnDestroy()
    {
    }

    #endregion


    #region INTERNAL API

    internal void InternalAwake()
    {
        OnAwake();
        Enable();
    }
    
    
    internal void InternalStart()
    {
        OnStart();
        HasBeenInitialized = true;
    }
    
    
    internal void InternalUpdate()
    {
        if (IsEnabled)
            OnUpdate();
    }
    
    
    internal void InternalFixedUpdate()
    {
        if (IsEnabled)
            OnFixedUpdate();
    }
    
    
    internal void InternalLateUpdate()
    {
        if (IsEnabled)
            OnLateUpdate();
    }
    
    
    internal void InternalDestroy()
    {
        Disable();
        OnDestroy();
    }


    /// <summary>
    /// Internal method to initialize the component and attach it to the given entity.
    /// </summary>
    /// <param name="entity">The entity this component is attached to.</param>
    internal override void Bind(Entity entity)
    {
        base.Bind(entity);
        BindToEntity(Entity.EntityRef.Entity);
    }


    /// <summary>
    /// Attaches this component to the given Arch entity.
    /// </summary>
    private void BindToEntity(Arch.Core.Entity entity)
    {
        ref BehaviourComponent c = ref entity.AddOrGet<BehaviourComponent>();
        c.Behaviours ??= new List<Behaviour>();
        c.Behaviours.Add(this);
    }

    #endregion
}