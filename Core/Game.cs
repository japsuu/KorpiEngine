using KorpiEngine.Core.Debugging.Profiling;
using KorpiEngine.Core.ECS.Entities;
using KorpiEngine.Core.InputManagement;
using KorpiEngine.Core.Logging;
using KorpiEngine.Core.Rendering.Cameras;
using KorpiEngine.Core.Rendering.Shaders;
using KorpiEngine.Core.SceneManagement;
using KorpiEngine.Core.Threading.Pooling;
using KorpiEngine.Core.UI.ImGui;
using KorpiEngine.Core.Windowing;
using OpenTK.Windowing.Common;

namespace KorpiEngine.Core;

/// <summary>
/// The main class for a game.
/// Contains the game window and handles the game loop.
/// </summary>
public abstract class Game : IDisposable
{
    // Protected:
    protected static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(Game));
    protected readonly KorpiWindow Window;
    
    // Private:
    private readonly EntityManager _entityManager;
    private ImGuiController _imGuiController = null!;
    private double _fixedFrameAccumulator;


    protected Game(WindowingSettings settings)
    {
        Window = new KorpiWindow(settings.GameWindowSettings, settings.NativeWindowSettings);
        _entityManager = new EntityManager();
        
        Window.Load += OnLoad;
        Window.UpdateFrame += OnUpdateFrame;
        Window.RenderFrame += OnRenderFrame;
        Window.Unload += OnUnload;
    }


    /// <summary>
    /// Enters the blocking game loop.
    /// </summary>
    public void Run()
    {
        Window.Run();
    }


    private void OnLoad()
    {
        _imGuiController = new ImGuiController(Window);
        GlobalJobPool.Initialize();
        
        LoadContent();
        
        // Load a scene.
        SceneManager.LoadScene(new EmptyScene());
        
        // Show the window after all resources are loaded.
        Window.CenterWindow();
        Window.IsVisible = true;
    }


    private void OnUpdateFrame(FrameEventArgs args)
    {
        KorpiProfiler.BeginFrame();
        KorpiProfiler.Begin("UpdateLoop");
        
        double deltaTime = args.Time;
        _fixedFrameAccumulator += deltaTime;
        
        using (new ProfileScope("FixedUpdate"))
        {
            while (_fixedFrameAccumulator >= EngineConstants.FIXED_DELTA_TIME)
            {
                InternalFixedUpdate();
                _fixedFrameAccumulator -= EngineConstants.FIXED_DELTA_TIME;
            }
        }
 
        double fixedAlpha = _fixedFrameAccumulator / EngineConstants.FIXED_DELTA_TIME;
        
        if (deltaTime > EngineConstants.MAX_DELTA_TIME)
        {
            Logger.Warn($"Detected large frame hitch ({1f/deltaTime:F2}fps, {deltaTime:F2}s)! Delta time was clamped to {EngineConstants.MAX_DELTA_TIME:F2} seconds.");
            deltaTime = EngineConstants.MAX_DELTA_TIME;
        }
        else if (deltaTime > EngineConstants.DELTA_TIME_SLOW_THRESHOLD)
        {
            Logger.Warn($"Detected frame hitch ({deltaTime:F2}s)!");
            deltaTime = EngineConstants.MAX_DELTA_TIME;
        }
        
        using (new ProfileScope("Update"))
        {
            InternalUpdate(deltaTime, fixedAlpha);
        }
        
        KorpiProfiler.End();
    }


    private void OnRenderFrame(FrameEventArgs args)
    {
        KorpiProfiler.Begin("RenderLoop");

        // We could also multiply the matrices here and then pass which is faster, but having the separate matrices available is used for some advanced effects.
        // IMPORTANT: OpenTK's matrix types are transposed from what OpenGL would expect - rows and columns are reversed.
        // They are then transposed properly when passed to the shader. 
        // This means that we retain the same multiplication order in both OpenTK c# code and GLSL shader code.
        // If you pass the individual matrices to the shader and multiply there, you have to do in the order "model * view * projection".
        // You can think like this: first apply the modelToWorld (aka model) matrix, then apply the worldToView (aka view) matrix, 
        // and finally apply the viewToProjectedSpace (aka projection) matrix.
        MatrixManager.UpdateViewMatrix(Camera.RenderingCamera.ViewMatrix);
        MatrixManager.UpdateProjectionMatrix(Camera.RenderingCamera.ProjectionMatrix);
        
        InternalRender();

        Window.SwapBuffers();
        KorpiProfiler.End();
        KorpiProfiler.EndFrame();
    }


    private void InternalFixedUpdate()
    {
        Time.FixedUpdate();
        
        SceneManager.FixedUpdate();
        _entityManager.FixedUpdate();
        
        FixedUpdate();
        
        // Instantly execute jobs.
        GlobalJobPool.FixedUpdate();
    }


    private void InternalUpdate(double deltaTime, double fixedAlpha)
    {
        Time.Update(deltaTime, fixedAlpha);
        Input.Update(Window.KeyboardState, Window.MouseState);
        
        ImGuiWindowManager.Update();
        _imGuiController.Update();
        
        SceneManager.Update();
        _entityManager.Update();
        
        Update();
        
        // Instantly execute jobs.
        GlobalJobPool.Update();
    }


    private void InternalRender()
    {
        SceneManager.Draw();
        _entityManager.Draw();
        
        Render();

        _imGuiController.Render();
        ImGuiController.CheckGlError("End of frame");
    }


    private void OnUnload()
    {
        UnloadContent();
        
        ImGuiWindowManager.Dispose();
        GlobalJobPool.Shutdown();
    }


    /// <summary>
    /// Called before the window is displayed for the first time.
    /// Load any resources here.
    /// </summary>
    protected virtual void LoadContent() { }
    
    
    /// <summary>
    /// Called every frame.
    /// </summary>
    protected virtual void Update() { }
    
    
    /// <summary>
    /// Called every fixed update frame (see <see cref="EngineConstants.FIXED_DELTA_TIME"/>).
    /// </summary>
    protected virtual void FixedUpdate() { }
    
    
    /// <summary>
    /// Called when the game window is being rendered.
    /// </summary>
    protected virtual void Render() { }


    /// <summary>
    /// Called when the game window is being unloaded.
    /// Dispose of any resources here.
    /// </summary>
    protected virtual void UnloadContent() { }
    
    
    public void Dispose()
    {
        Window.Dispose();
        GC.SuppressFinalize(this);
    }
}