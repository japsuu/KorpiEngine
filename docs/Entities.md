
# Entity documentation

## `GetComponent<T>` method

We have two types of components: `KorpiEngine.Core.Scripting.Component`, and the internal `KorpiEngine.Core.ECS.INativeComponent`.

The `KorpiEngine.Core.Scripting.Component` is a C# class that is used to define a component in the scripting environment.
It is a wrapper around the `KorpiEngine.Core.ECS.INativeComponent` interface, and is used to define the component's properties and methods in the scripting environment.
It does not contain any actual data, but is only a wrapper of getters and setters around the native component which actually contains the data.

### The problem

When the user calls `GetComponent<T>` on an entity, they expect to get the scripting component, not the native component.
We cannot just check if the underlying entity contains the provided scripting component type, because the scripting component is just a wrapper and not actually stored in the entity.

### The solution

To get around this, we delegate the `GetComponent<T>` method to the scripting component wrapper, which then checks if the underlying entity contains the native component.

Because the scripting components are not actually stored in the entity, we need to create a new instance of the scripting component wrapper every time `GetComponent<T>` is called.
