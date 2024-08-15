# <p align="center">Korpi Engine</p>

<div align="center">

[![SonarCloud](https://sonarcloud.io/images/project_badges/sonarcloud-white.svg)](https://sonarcloud.io/summary/new_code?id=japsuu_KorpiEngine)


[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=japsuu_KorpiEngine&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=japsuu_KorpiEngine)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=japsuu_KorpiEngine&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=japsuu_KorpiEngine)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=japsuu_KorpiEngine&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=japsuu_KorpiEngine)
[![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=japsuu_KorpiEngine&metric=sqale_index)](https://sonarcloud.io/summary/new_code?id=japsuu_KorpiEngine)

</div>

An open-source, MIT-licensed 3D game engine written in pure C# and .NET 8. The engine uses a 64-bit
(double) coordinate system on the CPU and camera-relative rendering on the GPU, to achieve large world support.

> [!WARNING]
> The engine is still under development and is not yet feature-complete!

> [!IMPORTANT]
> Only the `MAIN` branch is reserved for releases. All other branches might contain **broken** features!

- [About](#about-the-project)
- [Features & Roadmap](#features--roadmap)
- [Documentation](#documentation)
- [Getting started](#getting-started)
- [Contributing](CONTRIBUTING.md)
- [Acknowledgments](#acknowledgments)

# <p align="center">About The Project</p>

The goal of this project is to provide a viable alternative to other game engines for **programmers**,
and other people who prefer working directly with code.
Korpi Engine offers a smooth transition for developers familiar with Unity, by providing a clean and familiar API,
while also adhering to the [KISS principle](https://en.m.wikipedia.org/wiki/KISS_principle).
The engine is designed to be modular and extensible, allowing for easy integration of new features and systems.

# <p align="center">Features / Roadmap</p>

A non-exhaustive list of currently implemented engine features. Updated every once in a while.

- Runtime
  - Scripting
    - [x] Unity-like C# scripting API
    - [x] Unity-like Coroutines
    - [x] Hybrid Entity/Component/System model
    - [x] Asset/Resource management
  - Graphics
    - [x] Graphics-API agnostic backend
      - [x] OpenGL renderer
      - [ ] DirectX renderer
      - [ ] Vulcan renderer
    - [x] Camera-relative rendering
    - [x] Point, spot & directional lights
    - [x] Post-processing pipeline
  - UI
    - [x] Native Dear ImGUI support
  - Physics
    - [ ] Colliders
    - [ ] Rigidbodies
  - Audio
    - [ ] OpenAL audio backend
  - Networking
    - [x] LiteNetLib transport layer
    - [x] High-level networking API
    - [x] Server/Client architecture
  - Other
    - [x] Input system
    - [x] Scene system
    - [x] Full 64-bit coordinate system support
- Editor layer
  - [ ] Standalone editor

# <p align="center">Documentation</p>

There is no official documentation available yet.
You can follow the progress of this feature in [issue #12](https://github.com/japsuu/KorpiEngine/issues/12).

# <p align="center">Getting Started</p>

1. Download the repository
   - Either download this repository as an archive or clone it:
   - `git clone https://www.github.com/japsuu/KorpiEngine`
2. Build & run the [basic usage example](src/KorpiEngine.Runtime/Sandbox) (optional).
   - This example is updated every once in a while, and teaches you the basics.
   - `cd ./KorpiEngine/src/KorpiEngine.Runtime/Sandbox`
   - `dotnet build ./Sandbox.csproj`
   - The built executable is now located at `./bin/<configuration>/<framework>/Sandbox.exe`
3. Start developing your own game!
	- There are a couple of ways you can create your own projects with the engine:
		- **As a project reference** (recommended): You can add the engine as a project reference to your own solution: https://learn.microsoft.com/en-us/visualstudio/ide/managing-references-in-a-project
        - **As a standalone project**: You can create a new project in to the same solution as the engine. This is not recommended, as it may make updating the engine more difficult.
        - **As a NuGet package**: There is no NuGet package available yet :(. You can follow the progress of this feature in [issue #11](https://github.com/japsuu/KorpiEngine/issues/11).
4. If you encounter any issues, please report them in the [issue tracker](https://github.com/japsuu/KorpiEngine/issues).

# <p align="center">Acknowledgments</p>

Portions of code or ideas are derived from the following sources:
- [OpenTK](https://github.com/opentk/opentk) for providing a managed OpenGL wrapper.
- [Prowl Engine](https://github.com/ProwlEngine/Prowl) for providing inspiration and some code snippets.
- [Unity](https://unity.com/) for providing inspiration and a reference point for the engine's design.
- [LiteNetLib](https://github.com/RevenantX/LiteNetLib) for providing a high-performance C# rUDP networking library.
- 2D game framework [Duality](https://github.com/AdamsLair/duality) for providing a reference point for the asset pipeline.
