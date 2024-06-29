# Korpi Engine

Korpi Engine is an open-source, MIT-licensed, code-only 3D game engine written in pure C# and .NET 8.

> The engine is still under development and is not yet feature-complete.

> ⚠️ This is the active `DEVELOPMENT` branch, which might contain **broken** features! ⚠️

## Table of Contents

- [Project goals](#project-goals)
- [Project status](#projestatus)
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
	- [x] Unity-like Corourines
	- [ ] Toggleable full 64-bit coordinate system support
	- [ ] Scene system
	- [ ] OpenAL audio backend
- [ ] Editor layer

## Documentation

> TODO: Add a link to the DocFX site here.

## Getting Started

[Basic usage example](src/KorpiEngine.Runtime/Sandbox/CustomScene.cs) is included in the `Sandbox` directory, inside the `KorpiEngine.Runtime` project. This example is updated every once in a while, and teaches you the basics.

> TODO: Guide on Unity -> Korpi transitioning
