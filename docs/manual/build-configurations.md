
# Build Configurations

The KorpiEngine solution has three default build configurations: `Debug`, `Release`, and `Production`.

Each build configuration has a different use case and is optimized for different scenarios.

## Default Build Configurations

The `Debug` build configuration is used for development and debugging purposes.
It is the slowest build configuration with all optimizations disabled,
but it has the most debugging information available.

The `Release` build configuration is used for testing and performance profiling.
It is faster than the `Debug` build configuration with some optimizations enabled,
but still has most of the debugging information available.

The `Production` build configuration is used for shipping the final product.
It is the fastest build configuration with all optimizations enabled,
and all tooling and debugging information stripped.

<br/>

## Default Preprocessor Defines

The following preprocessor defines are available in KorpiEngine projects,
in addition to the default ones (e.g. `DEBUG`, `RELEASE`, `PRODUCTION`, `NETCOREAPP`, `NET`, etc.) provided by the .NET SDK.

| Preprocessor define | Debug | Release | Production | Description                                         |
|---------------------|:-----:|:-------:|:----------:|-----------------------------------------------------|
| `KORPI_PROFILE`     |   ✔   |    ✔    |            | Profiling functionality is included with the build. |
| `KORPI_TOOLS`       |   ✔   |    ✔    |            | Tooling functionality is included with the build.   |
| `KORPI_OPTIMIZED`   |       |    ✔    |     ✔      | Optimized code compilation is enabled.              |
