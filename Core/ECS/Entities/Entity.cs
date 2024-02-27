using System.Diagnostics.CodeAnalysis;
using KorpiEngine.Core.ECS.Components;

namespace KorpiEngine.Core.ECS.Entities;

public abstract class Entity
{
    public static event Action<Entity, bool>? EnableEvent;
    
    public bool IsEnabled { get; private set; }
    
    private readonly Dictionary<Type, Component> _components;


    protected Entity(bool isEnabled = true)
    {
        _components = new Dictionary<Type, Component>();
        
        if(isEnabled)
            Enable();
    }


    public void Enable()
    {
        if (IsEnabled)
            return;
        
        if (EnableEvent == null)
        {
            throw new NullReferenceException("Tried to enable an Entity but there was no listeners!");
        }
        
        IsEnabled = true;
        EnableEvent.Invoke(this, true);
        OnEnable();
    }
    
    
    internal void Update()
    {
        OnUpdate();
        
        foreach (Component component in _components.Values)
        {
            component.Update();
        }
    }


    internal void FixedUpdate()
    {
        OnFixedUpdate();
        
        foreach (Component component in _components.Values)
        {
            component.FixedUpdate();
        }
    }
    
    
    internal void Draw()
    {
        OnDraw();
        
        foreach (Component component in _components.Values)
        {
            component.Draw();
        }
    }
    
    
    public void Disable()
    {
        if (!IsEnabled)
            return;

        if (EnableEvent == null)
        {
            throw new NullReferenceException("Tried to disable an Entity but there was no listeners!");
        }
        
        IsEnabled = false;
        EnableEvent.Invoke(this, false);
        OnDisable();
    }
    
    
    public void AddComponent<T>(T comp) where T : Component
    {
        _components.Add(typeof(T), comp);
        comp.SetEntity(this);
        comp.Enable();
    }
    
    
    public T AddComponent<T>() where T : Component, new()
    {
        T comp = new();
        _components.Add(typeof(T), comp);
        comp.SetEntity(this);
        comp.Enable();
        return comp;
    }
    
    
    public T GetOrAddComponent<T>() where T : Component, new()
    {
        if (TryGetComponent(out T? component))
            return component;
        
        component = AddComponent<T>();
        return component;
    }
    
    
    public bool TryGetComponent<T>([NotNullWhen(true)] out T? component) where T : Component
    {
        component = GetComponent<T>();
        return component != null;
    }
    
    
    public void RemoveComponent<T>() where T : Component
    {
        if (!TryGetComponent(out T? component))
            return;
        
        component.Disable();
        component.Destroy();
        _components.Remove(typeof(T));
    }


    private T? GetComponent<T>() where T : Component
    {
        if (_components.TryGetValue(typeof(T), out Component? component))
            return (T) component;
        return null;
    }
    
    
    /// <summary>
    /// Called after the Entity has been enabled.
    /// </summary>
    protected virtual void OnEnable() { }
    
    
    /// <summary>
    /// Called when the Entity is updated.
    /// </summary>
    protected virtual void OnUpdate() { }
    
    
    /// <summary>
    /// Called when the Entity is updated.
    /// </summary>
    protected virtual void OnFixedUpdate() { }
    
    
    /// <summary>
    /// Called when the Entity is drawn.
    /// </summary>
    protected virtual void OnDraw() { }
    
    
    /// <summary>
    /// Called after the Entity has been disabled.
    /// </summary>
    protected virtual void OnDisable() { }
}