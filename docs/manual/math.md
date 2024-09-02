
# Math

Korpi Engine uses a modified version of [vimaec/Math3D](https://github.com/vimaec/Math3D) for most of its math operations.

Their API documentation can be found [here](https://vimaec.github.io/Math3D/).

## Angles

In the Korpi Engine when working with angles, you can use either degrees or radians.
Most methods provide both versions, so you can use the one you prefer.

You can convert between radians/degrees by calling
```csharp
float radians = degrees.ToRadians();
float degrees = radians.ToDegrees();
```

Internally, the engine uses radians for all calculations, so if you need to squeeze out every bit of performance, you should use radians.