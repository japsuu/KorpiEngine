# Korpi Engine

Korpi Engine is an open-source,
MIT-licensed 3D game engine written in pure C# and .NET 8. The engine uses a 64-bit
(double) coordinate system on the CPU and camera-relative rendering on the GPU, to achieve large world support.

> [!WARNING]
> The engine is still under development and is not yet feature-complete!

> [!IMPORTANT]
> Only the `MAIN` branch is reserved for releases. All other branches might contain **broken** features!

## Table of Contents

- [Project goals](#project-goals)
- [Project status](#project-status)
- [Documentation](#documentation)
- [Getting started](#getting-started)
- [Contributing](CONTRIBUTING.md)

## Project goals

The goal of this project is to provide a viable alternative to other game engines for **programmers**,
and other people who prefer working directly with code.
Korpi Engine offers a smooth transition for developers familiar with Unity, by providing a clean and familiar API,
while also adhering to the [KISS principle](https://en.m.wikipedia.org/wiki/KISS_principle).
The engine is designed to be modular and extensible, allowing for easy integration of new features and systems.

## Project status

A non-exhaustive list of currently implemented engine features. Updated every once in a while:

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
    - [ ] Point, spot & directional lights
    - [ ] Post-processing pipeline
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

## Documentation

There is no official documentation available yet.
You can follow the progress of this feature in [issue #12](https://github.com/japsuu/KorpiEngine/issues/12).

## Getting Started

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

> TODO: Guide on Unity -> Korpi transitioning
