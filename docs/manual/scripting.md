
# Scripting

Korpi Engine uses a Unity-like C# scripting API for game logic.
The scripting API is designed to be quite similar to Unity's API,
to make it easier for developers to transition from Unity to KorpiEngine.

> [!NOTE]
> Most of the API has been annotated with XML comments, so you can see the documentation in your IDE.

<br/>

## Entity/Component/System model

<br/>

### Entities

Entities are the basic building blocks of the game world. They are similar to GameObjects in Unity.

You can create entities by calling `scene.CreateEntity()`, or by creating an `Entity` object.

> [!WARNING]
> If you create an `Entity` object with `new Entity()`, you need to also spawn it into a scene by calling `entity.Spawn(scene)`.

They:
- have a `Transform` component that defines their position, rotation and scale.
- can have multiple components attached to them.
- can be parented to other entities.
- can have a name for debugging purposes.
- can be dynamically created and destroyed at runtime.
- cannot be inherited from.

<br/>

### Components

Components are reusable pieces that can be attached to entities to give them functionality.

Similarly to Unity, they can be added to Entities by calling `AddComponent<T>()` on an entity, queried with `GetComponent<T>()`, and removed with `RemoveComponent<T>()`.

They:
- inherit from `EntityComponent`.
- can be added and removed at runtime.
- have the usual expected lifecycle methods (`OnAwake`, `OnStart`, `OnUpdate`, `OnDestroy`, ...).

<br/>

### Systems

> [!NOTE]
> The systems API is still a work in progress. It can be used, but it's not as polished as the rest of the API.

`EntitySystem` is a base class for systems that operate on the components of a single entity.

`SceneSystem` is a base class for systems that operate on all components of a given type in a scene.
