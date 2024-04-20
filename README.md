# Korpi Engine

Korpi Engine is an open-source, MIT-licensed 3D game engine written in pure C# in .NET 6. The engine is internally based around an ECS architecture.

> The engine is still under development and is not yet feature-complete.

> ⚠️ Development happens on the `dev` branch [(link)](https://github.com/japsuu/KorpiEngine/tree/dev). **This branch is reserved for releases** ⚠️

## Table of Contents

- [Project goals](#project-goals)
- [Project status](#project-status)
- [Documentation](#documentation)
- [Getting started](#getting-started)
- [Contributing](CONTRIBUTING.md)

## Project goals

The goal of this project is to provide a viable alternative to the Unity game engine for **programmers**, and other people who prefer working directly with code.
Korpi Engine offers a smooth transition for developers familiar with Unity, by providing a clean and familiar API, while also adhering to the [KISS principle](https://en.m.wikipedia.org/wiki/KISS_principle).
The engine is designed to be modular and extensible, allowing for easy integration of new features and systems.

## Project status

A non-exhaustive list of currently implemented engine features. Updated every once in a while:

- [ ] Runtime
	- [x] Unity-like scripting API
	- [x] C# Scripting
	- [x] Entity & Component structure
	- [x] Graphics-API agnostic backend
		- [x] OpenGL renderer
		- [ ] DirectX renderer
		- [ ] Vulcan renderer
		- [ ] Point, spot & directional lights
		- [ ] Post-processing pipeline
	- [x] Native Dear ImGUI support
	- [ ] Physics
		- [ ] Colliders
		- [ ] Rigidbodies
	- [ ] Unity-like Corourines
	- [ ] Toggleable full 64-bit coordinate system support
	- [ ] Scene system
	- [ ] OpenAL audio backend
- [ ] Editor layer

## Documentation

> TODO: Add a link to the DocFX site here.

## Getting Started

[Basic usage example](src/KorpiEngine.Runtime/Sandbox/CustomScene.cs) is included in the `Sandbox` directory, inside the `KorpiEngine.Runtime` project. This example is updated every once in a while, and teaches you the basics.

> TODO: Guide on Unity -> Korpi transitioning
