
# Introduction

Korpi Engine is a code-only 3D game engine written in C#.

The goal of this engine is to provide a viable alternative to other game engines for **programmers**,
and other people who prefer working directly with code.

Korpi Engine does **NOT** aim to:
- be just like Unity or Unreal Engine
- offer a visual scripting system
- be a one-size-fits-all solution for every game project

Korpi Engine **DOES** aim to
- stand out with its 'programmer-first' approach
- offer a clean and simple API
- adhere to the [KISS principle](https://en.m.wikipedia.org/wiki/KISS_principle)
- be a good learning tool for game/engine development
- be modular and extensible, to allow for easy integration of new features and systems.

## Which types of games is Korpi Engine best suited for?

- **Open-world games**: The engine is designed to handle large worlds with minimal additional programmer effort.
- **Procedural generation**: The APIs have been designed to support custom procedurally generated meshes.

An example of such a game could be a Minecraft-like voxel game, or a deep-space exploration game.

## Which types of games is Korpi Engine NOT well-suited for?

- **2D games**: The engine is designed for 3D games, and does not have built-in support for 2D games.
- **Mobile games**: The engine is designed for desktop platforms, and does not have built-in support for mobile platforms.
- **Highly visual games**: The engine does not have a visual scripting system, and is not designed for rapid prototyping of visual effects.
- **Games that require a large/complex asset pipeline**: While the current pipeline supports streaming and dynamic loading/unloading, features like asset compression have not yet been implemented.

## Why the name "Korpi"?

The name "Korpi" is Finnish and means "wilderness" or "forest". It was chosen to reflect the engine's focus on large, open-world games.