
# Entity documentation

## `GetComponent<T>` method

We have two types of components: `KorpiEngine.Core.Scripting.Component`, and the internal `KorpiEngine.Core.ECS.INativeComponent`.

The `KorpiEngine.Core.Scripting.Component` is a C# class that is used to define a component in the scripting environment.
It is a wrapper around the `KorpiEngine.Core.ECS.INativeComponent` interface, and is used to define the component's properties and methods in the scripting environment.
It does not contain any actual data, but is only a wrapper of getters and setters around the native component which actually contains the data.

### The problem

When the user calls `GetComponent<T>` on an entity, they expect to get the scripting component, not the native component.
We cannot just check if the underlying entity contains the provided scripting component type, because the scripting component is just a wrapper and not actually stored in the entity.

### The (_temporary_) solution

To get around this, we delegate a `NativeComponentType` field to the scripting component wrapper, which we can use with reflection to get the type of the native component the wrapper represents.

All user-defined components (scripts) inherit from `Behaviour`, so they set `NativeComponentType` to `BehaviourComponent` type. Likewise, the `Transform` component has `NativeComponentType` of `TransformComponent`.

The simplified implementation of `GetComponent<T>` is approximately as follows:
```csharp
public T? GetComponent<T>() where T : class // We cannot use a Component constraint here, as we need to check if the provided type is a component or a plain C# interface.
{
    if (typeof(T).IsSubclassOf(typeof(Component)))
    {
        // The provided type is a component wrapper, so get the native component type for it.
        // Create a new instance of the provided type using reflection, to basically call T.NativeComponentType.
        object instance = Activator.CreateInstance(typeof(T))!;
        
        Type nativeComponentType = ((Component)instance).NativeComponentType;   // We know 'instance' is a component, so cast it to one to access the NativeComponentType property.
        
        if (!HasNativeComponent(nativeComponentType))
            return null;

        return (T)instance; // We know T is a component, so cast and return instance.
    }

    // The provided type is not a component, so it may be an interface.
    // Check all behaviours if they implement the provided type.
    List<Behaviour>? behaviours = EntityRef.Entity.Get<BehaviourComponent>().Behaviours;
    if (behaviours == null)
        return null;
    
    foreach (Behaviour behaviour in behaviours)
    {
        if (behaviour is T t)
            return t;
    }

    return null;
}
```

However, this has some obvious drawbacks like the use of reflection and the need to create a new instance of the provided type.
This is not performant, and will hopefully be replaced with a better solution in the future.