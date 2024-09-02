
# Math

In the Korpi Engine when working with angles, you can use either degrees or radians.
Most methods provide both versions, so you can use the one you prefer.

You can convert between radians/degrees by calling
```csharp
float radians = degrees.ToRadians();
float degrees = radians.ToDegrees();
```