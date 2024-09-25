
# Profiling

KorpiEngine uses the `Tracy` profiler to measure the performance of the engine.
_Tracy is a real-time, nanosecond resolution, remote telemetry, hybrid frame and sampling profiler for games and other applications_.

> [!NOTE]
> Profiling is enabled by default for `Debug` and `Release` builds, but **NOT** for `Production` builds.

<br/>

## Usage

1. Download the latest Tracy profiler from [here](https://github.com/wolfpld/tracy/releases) and extract the downloaded archive.
2. Run your game/application with a build configuration that has profiling enabled.
3. Run the `tracy-profiler` executable, located in the extracted archive.
4. The profiler should discover the running game/application instance. _Double-click_ on the instance to connect to it.

<br/>

## Custom Profiling

To profile a specific part of your code,
you can use the <xref:KorpiEngine.Tools.IProfiler> provided by `Application.Profiler`.
The `IProfiler` provides a simplified interface to start and stop profiling a specific block of code,
and plotting data in the profiler.

<br/>

### CPU Profiling

```csharp
IProfilerZone zone = Application.Profiler.BeginZone("MyZone");

// Code to profile
// ...
if (condition)
    zone.EmitText("Something happened!");
// ...

zone.Dispose();
```

You can also use a `using` statement to automatically dispose the zone.

```csharp
using (IProfilerZone zone = Application.Profiler.BeginZone("MyZone"))
{
    // Code to profile
    // ...
}
```

Profiling with an attribute is also supported (profiling code is injected around the method at build-time, using `AspectInjector`).
This approach is useful when you want to quickly profile a method without modifying the code,
but it's not as flexible.

```csharp

[Profile]
public void MyMethod()
{
    // Code to profile
    // ...
}
``` 

<br/>

### GPU Profiling

GPU profiling is not yet supported.

<br/>

## Advanced

You can also use the `Tracy` API directly to profile your code.

We use [clibequilibrium/Tracy-CSharp](https://github.com/clibequilibrium/Tracy-CSharp) bindings
to interact with the `Tracy` profiler.

> [!TIP]
> Use Tracy's [PDF manual](https://github.com/wolfpld/tracy/releases) to learn more about the Tracy API.
