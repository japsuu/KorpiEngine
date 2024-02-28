using System.Diagnostics.CodeAnalysis;
using Arch.Core;
using KorpiEngine.Core.GameObjects.Components;
using KorpiEngine.Core.SceneManagement;

namespace KorpiEngine.Core.GameObjects;

/// <summary>
/// High-level container for user-defined components in the game world.
/// </summary>
public sealed class GameObject
{
    /// <summary>
    /// True if this GameObject is enabled.
    /// </summary>
    public bool IsEnabled { get; private set; }

    internal readonly Entity WorldEntity;
    
    private readonly Dictionary<Type, List<Component>> _componentTypes;


    internal GameObject(Entity worldEntity, bool isEnabled)
    {
        WorldEntity = worldEntity;
        _componentTypes = new Dictionary<Type, List<Component>>();
        
        if(isEnabled)
            Enable();
    }


    /// <summary>
    /// Enables the GameObject, allowing any (enabled) attached components to update.
    /// </summary>
    public void Enable()
    {
        if (IsEnabled)
            return;
        
        SceneManager.CurrentScene.GameObjectManager.RegisterUpdates(this);
        IsEnabled = true;
    }
    
    
    internal void Update()
    {
        foreach (List<Component> components in _componentTypes.Values)
        {
            foreach (Component component in components)
            {
                component.InternalUpdate();
            }
        }
    }


    internal void FixedUpdate()
    {
        foreach (List<Component> components in _componentTypes.Values)
        {
            foreach (Component component in components)
            {
                component.InternalFixedUpdate();
            }
        }
    }
    
    
    /// <summary>
    /// Disables the GameObject, preventing any components from updating.
    /// </summary>
    public void Disable()
    {
        if (!IsEnabled)
            return;

        SceneManager.CurrentScene.GameObjectManager.UnregisterUpdates(this);
        IsEnabled = false;
    }
    
    
    public T AddComponent<T>() where T : Component, new()
    {
        if (!_componentTypes.TryGetValue(typeof(T), out List<Component>? components))
        {
            components = new List<Component>();
            _componentTypes.Add(typeof(T), components);
        }
        
        T comp = new();
        comp.SetGameObject(this);
        
        components.Add(comp);
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
    
    
    public bool TryGetComponents<T>([NotNullWhen(true)] out List<T>? components) where T : Component
    {
        if (_componentTypes.TryGetValue(typeof(T), out List<Component>? comps))
        {
            components = comps.Cast<T>().ToList();
            return true;
        }
        
        components = null;
        return false;
    }
    
    
    internal void RemoveComponent(Component component)
    {
        _componentTypes.Remove(component.GetType());
    }


    private T? GetComponent<T>() where T : Component
    {
        if (_componentTypes.TryGetValue(typeof(T), out List<Component>? components) && components.Count > 0)
            return (T)components[0];
        return null;
    }
}