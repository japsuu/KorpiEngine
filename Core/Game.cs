using KorpiEngine.Core.Debugging.Profiling;
using KorpiEngine.Core.InputManagement;
using KorpiEngine.Core.Logging;
using KorpiEngine.Core.Rendering;
using KorpiEngine.Core.Rendering.Cameras;
using KorpiEngine.Core.Rendering.GraphicsDrivers;
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
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(Game));
    private readonly KorpiWindow _window;
    
    // Private:
    private readonly ImGuiController _imGuiController;
    private double _fixedFrameAccumulator;


    protected Game(WindowingSettings settings)
    {
        InitializeLog4Net();

        Graphics.Initialize(new GLGraphicsDriver());
        _window = new KorpiWindow(settings.GameWindowSettings, settings.NativeWindowSettings);
        _imGuiController = new ImGuiController(_window);
        
        _window.Load += OnLoad;
        _window.UpdateFrame += OnUpdateFrame;
        _window.RenderFrame += OnRenderFrame;
        _window.Unload += OnUnload;
    }


    private static void InitializeLog4Net()
    {
        // Add support for additional encodings (code pages), required by Log4Net.
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        
        // Initialize the Log4Net configuration.
        LogFactory.Initialize("log4net.config");
    }


    /// <summary>
    /// Enters the blocking game loop.
    /// </summary>
    public void Run()
    {
        _window.Run();
    }


    private void OnLoad()
    {
        SceneManager.Initialize();
        GlobalJobPool.Initialize();
        
        // Queue window visibility after all internal resources are loaded.
        _window.CenterWindow();
        _window.IsVisible = true;
        
        OnLoadContent();
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
        MatrixManager.UpdateViewMatrix(Camera.MainCamera.ViewMatrix);
        MatrixManager.UpdateProjectionMatrix(Camera.MainCamera.ProjectionMatrix);
        
        InternalRender();
        
        KorpiProfiler.End();
        KorpiProfiler.EndFrame();
    }


    private void InternalFixedUpdate()
    {
        Time.FixedUpdate();
        
        SceneManager.FixedUpdate();
        
        OnFixedUpdate();
        
        // Instantly execute jobs.
        GlobalJobPool.FixedUpdate();
    }


    private void InternalUpdate(double deltaTime, double fixedAlpha)
    {
        Time.Update(deltaTime, fixedAlpha);
        Input.Update(_window.KeyboardState, _window.MouseState);
        
        ImGuiWindowManager.Update();
        _imGuiController.Update();
        
        SceneManager.Update();
        
        OnUpdate();
        
        // Instantly execute jobs.
        GlobalJobPool.Update();
    }


    private void InternalRender()
    {
        SceneManager.Draw();
        
        OnRender();

        _imGuiController.Render();
        ImGuiController.CheckGlError("End of frame");
    }


    private void OnUnload()
    {
        OnUnloadContent();
        
        SceneManager.UnloadAllScenes();
        GlobalJobPool.Shutdown();
        Graphics.Shutdown();
    }


    /// <summary>
    /// Called before the window is displayed for the first time.
    /// Load any resources here.
    /// </summary>
    protected virtual void OnLoadContent() { }
    
    
    /// <summary>
    /// Called every frame.
    /// </summary>
    protected virtual void OnUpdate() { }
    
    
    /// <summary>
    /// Called every fixed update frame (see <see cref="EngineConstants.FIXED_DELTA_TIME"/>).
    /// </summary>
    protected virtual void OnFixedUpdate() { }
    
    
    /// <summary>
    /// Called when the game window is being rendered.
    /// </summary>
    protected virtual void OnRender() { }


    /// <summary>
    /// Called when the game window is being unloaded.
    /// Dispose of any resources here.
    /// </summary>
    protected virtual void OnUnloadContent() { }
    
    
    public void Dispose()
    {
        ImGuiWindowManager.Dispose();
        _imGuiController.Dispose();
        _window.Dispose();
        GC.SuppressFinalize(this);
    }
}