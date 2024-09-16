using System.Reflection;

namespace KorpiEngine.Entities;

public sealed partial class Entity
{
    #region AddComponent API

    public T AddComponent<T>() where T : EntityComponent, new()
    {
        if (IsDestroyed)
            throw new InvalidOperationException($"Entity {InstanceID} has been destroyed.");

        Type type = typeof(T);
        return (AddComponent(type) as T)!;
    }


    private EntityComponent AddComponent(Type type)
    {
        if (!typeof(EntityComponent).IsAssignableFrom(type))
            throw new InvalidOperationException($"The type {type.Name} does not inherit from EntityComponent");

        RequireComponentAttribute? requireComponentAttribute = type.GetCustomAttribute<RequireComponentAttribute>();
        if (requireComponentAttribute != null)
            foreach (Type requiredComponentType in requireComponentAttribute.Types)
            {
                if (!typeof(EntityComponent).IsAssignableFrom(requiredComponentType))
                    continue;

                // If there is already a component on the object
                if (GetComponent(requiredComponentType) != null)
                    continue;

                // Recursive call to attempt to add the new component
                AddComponent(requiredComponentType);
            }

        bool disallowMultiple = type.GetCustomAttribute<DisallowMultipleComponentAttribute>() != null;
        bool hasComponent = GetComponent(type) != null;

        if (disallowMultiple && hasComponent)
            throw new InvalidOperationException(
                $"Can't add the same component multiple times: the component of type {type.Name} does not allow multiple instances");

        if (Activator.CreateInstance(type) is not EntityComponent comp)
            throw new InvalidOperationException($"Failed to create component of type {type.Name}");

        comp.Bind(this);
        _components.Add(comp);
        _componentCache.Add(type, comp);

        if (EnabledInHierarchy)
            comp.InternalAwake();

        RegisterComponentWithSystems(comp);

        return comp;
    }

    #endregion


    #region RemoveComponent API

    public void RemoveAll<T>() where T : EntityComponent //TODO: Accepting interfaces
    {
        if (!_componentCache.TryGetValue(typeof(T), out IReadOnlyCollection<EntityComponent> components))
            return;

        foreach (EntityComponent component in components)
        {
            component.Destroy();

            _components.Remove(component);
        }

        _componentCache.Remove(typeof(T));

        foreach (EntityComponent component in components)
            UnregisterComponentWithSystems(component);
    }


    public void RemoveComponent<T>(T component) where T : EntityComponent //TODO: Accepting interfaces
    {
        Type type = typeof(T);

        RemoveComponent(component, type);
    }


    public void RemoveComponent(EntityComponent component) //TODO: Accepting interfaces
    {
        Type type = component.GetType();

        RemoveComponent(component, type);
    }


    private void RemoveComponent<T>(T component, Type type) where T : EntityComponent
    {
        if (!component.CanBeDestroyed())
            return;

        _components.Remove(component);
        _componentCache.Remove(type, component);

        component.Destroy();

        UnregisterComponentWithSystems(component);
    }

    #endregion


    #region GetComponent API

    public T? GetComponent<T>() where T : EntityComponent => (T?)GetComponent(typeof(T));


    public EntityComponent? GetComponent(Type type)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (type == null)
            return null;

        if (_componentCache.TryGetValue(type, out IReadOnlyCollection<EntityComponent> components))
            return components.First();

        foreach (EntityComponent comp in _components)
            if (comp.GetType().IsAssignableTo(type))
                return comp;

        return null;
    }


    public IEnumerable<EntityComponent> GetAllComponents() => _components;

    public bool TryGetComponent<T>(out T? component) where T : EntityComponent
    {
        component = GetComponent<T>();
        return component != null;
    }


    public IEnumerable<T> GetComponents<T>() where T : EntityComponent
    {
        if (_componentCache.TryGetValue(typeof(T), out IReadOnlyCollection<EntityComponent> components))
        {
            foreach (EntityComponent comp in components)
                if (comp.GetType().IsAssignableTo(typeof(T)))
                    yield return (T)comp;
        }
        else
        {
            foreach (KeyValuePair<Type, IReadOnlyCollection<EntityComponent>> kvp in _componentCache.ToArray())
                if (kvp.Key.GetTypeInfo().IsAssignableTo(typeof(T)))
                    foreach (EntityComponent comp in kvp.Value.ToArray())
                        yield return (T)comp;
        }
    }


    public T? GetComponentInParent<T>(bool includeSelf = true, bool includeInactive = false) where T : EntityComponent =>
        (T?)GetComponentInParent(typeof(T), includeSelf, includeInactive);


    public EntityComponent? GetComponentInParent(Type type, bool includeSelf = true, bool includeInactive = false)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (type == null)
            return null;

        // First check the current Object
        EntityComponent? component;
        if (includeSelf && EnabledInHierarchy)
        {
            component = GetComponent(type);
            if (component != null)
                return component;
        }

        // Now check all parents
        Entity? parent = this;
        while ((parent = parent.Parent) != null)
            if (parent.EnabledInHierarchy || includeInactive)
            {
                component = parent.GetComponent(type);
                if (component != null)
                    return component;
            }

        return null;
    }


    public IEnumerable<T> GetComponentsInParent<T>(bool includeSelf = true, bool includeInactive = false) where T : EntityComponent
    {
        // First check the current Object
        if (includeSelf && EnabledInHierarchy)
            foreach (T component in GetComponents<T>())
                yield return component;

        // Now check all parents
        Entity? parent = this;
        while ((parent = parent.Parent) != null)
            if (parent.EnabledInHierarchy || includeInactive)
                foreach (T component in parent.GetComponents<T>())
                    yield return component;
    }


    public T? GetComponentInChildren<T>(bool includeSelf = true, bool includeInactive = false) where T : EntityComponent =>
        (T?)GetComponentInChildren(typeof(T), includeSelf, includeInactive);


    public EntityComponent? GetComponentInChildren(Type componentType, bool includeSelf = true, bool includeInactive = false)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (componentType == null)
            return null;

        // First check the current Object
        EntityComponent? component;
        if (includeSelf && EnabledInHierarchy)
        {
            component = GetComponent(componentType);
            if (component != null)
                return component;
        }

        // Now check all children
        foreach (Entity child in _childList)
            if (EnabledInHierarchy || includeInactive)
            {
                component = child.GetComponent(componentType) ?? child.GetComponentInChildren(componentType);
                if (component != null)
                    return component;
            }

        return null;
    }


    public IEnumerable<T> GetComponentsInChildren<T>(bool includeSelf = true, bool includeInactive = false) where T : EntityComponent
    {
        // First check the current Object
        if (includeSelf && EnabledInHierarchy)
            foreach (T component in GetComponents<T>())
                yield return component;

        // Now check all children
        foreach (Entity child in _childList)
            if (EnabledInHierarchy || includeInactive)
                foreach (T component in child.GetComponentsInChildren<T>())
                    yield return component;
    }

    #endregion


    internal bool IsComponentRequired(EntityComponent requiredComponent, out Type dependentType)
    {
        Type componentType = requiredComponent.GetType();
        foreach (EntityComponent component in _components)
        {
            RequireComponentAttribute? requireComponentAttribute =
                component.GetType().GetCustomAttribute<RequireComponentAttribute>();
            if (requireComponentAttribute == null)
                continue;

            if (Array.TrueForAll(requireComponentAttribute.Types, type => type != componentType))
                continue;

            dependentType = component.GetType();
            return true;
        }

        dependentType = null!;
        return false;
    }
}