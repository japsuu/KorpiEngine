namespace KorpiEngine.Core.GameObjects.Components;

/// <summary>
/// A component attached to a <see cref="GameObject"/>.
/// </summary>
public abstract class Component
{
    /// <summary>
    /// True if this component is enabled.
    /// </summary>
    public bool IsEnabled { get; private set; }
    
    /// <summary>
    /// True if this component is receiving updates.
    /// </summary>
    public bool IsActiveAndEnabled => IsEnabled && GameObject.IsEnabled;
    
    /// <summary>
    /// The <see cref="GameObject"/> this component is attached to.
    /// </summary>
    public GameObject GameObject { get; private set; } = null!;


    internal Component() { }
    
    
    internal void SetGameObject(GameObject gameObject)
    {
        GameObject = gameObject;
    }


    public void Enable()
    {
        IsEnabled = true;
        OnEnable();
    }
    
    
    internal void InternalUpdate()
    {
        Update();
    }


    internal void InternalFixedUpdate()
    {
        FixedUpdate();
    }
    
    
    public void Disable()
    {
        IsEnabled = false;
        OnDisable();
    }
    
    
    public void Destroy()
    {
        Disable();
        OnDestroy();
        GameObject.RemoveComponent(this);
    }


    protected virtual void OnEnable() { }
    
    protected virtual void Update() { }
    
    protected virtual void FixedUpdate() { }
    
    protected virtual void OnDisable() { }
    
    protected virtual void OnDestroy() { }
}