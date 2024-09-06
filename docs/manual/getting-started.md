
# Getting Started

This guide will help you get started with the KorpiEngine. If you encounter any issues, please report them in the [issue tracker](https://github.com/japsuu/KorpiEngine/issues).

<br/>

## Run the example project

There's an example project included in the engine solution, that will teach you the basics of the engine.
You can find it in the [src/Sandbox](https://www.github.com/japsuu/KorpiEngine/src/Sandbox/Sandbox.csproj) directory.
The example has multiple scenes, each showcasing different features of the engine.

1. Download the repository:
    - Either download the repository as an archive or clone it:
    - `git clone https://www.github.com/japsuu/KorpiEngine`
2. Build & run the [example project](https://www.github.com/japsuu/KorpiEngine/src/Sandbox/Sandbox.csproj):
   - With an **IDE**:
      - Open the solution file `./KorpiEngine.sln` in your favourite IDE.
      - Set the `Sandbox` project as the startup project.
      - Build and run the project.
   - With the **TERMINAL**:
      - `cd ./KorpiEngine/src/Sandbox`
      - `dotnet build ./Sandbox.csproj`
      - The built executable is now located at `./bin/<configuration>/<framework>/Sandbox.exe`

<br/>

## Start developing your own game

Download the repository, or skip this step if you already have it (see step 1 above).

There are two main ways you can create your own projects with the engine:

<br/>

### New solution with the engine as a dependency

> [!NOTE]
> Recommended for most users.

**Option 1:** Use the NuGet package (not available yet)
- You can follow the progress of this feature in [issue #11](https://github.com/japsuu/KorpiEngine/issues/11).

**Option 2:** Reference engine DLL
- You can build the engine as a DLL and reference it from your project:
   - Create a new solution with a new project for your game.
   - Open the engine solution and build the projects you need.
   - Reference the built engine DLLs from your game project.
   - You should now have access to the engine API in your game project.

<br/>

### New project in the engine solution

Arguably the easier way to get started is to create a new project in the same solution as the engine.
This way you can reference the engine from your project and build them both at the same time.
If you need to modify the engine, you can do so directly from the same solution.
This may make updating the engine more difficult.

**Option 1:** New project
- Create a new project into the engine solution, and [reference the engine](https://learn.microsoft.com/en-us/visualstudio/ide/managing-references-in-a-project).
- You can now build and run your project alongside the engine.

**Option 2:** Example project
- Use the included [example project](https://www.github.com/japsuu/KorpiEngine/src/Sandbox/Sandbox.csproj) as a starting point for your own project. This project already has the engine referenced.
