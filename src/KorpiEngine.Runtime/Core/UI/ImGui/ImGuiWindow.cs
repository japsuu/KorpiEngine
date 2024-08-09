using ImGuiNET;
using KorpiEngine.Core.Logging;

namespace KorpiEngine.Core.UI.ImGui;

public abstract class ImGuiWindow : IDisposable
{
    protected static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(ImGuiWindow));

    protected virtual ImGuiWindowFlags Flags { get; } = ImGuiWindowFlags.None;
    
    public abstract string Title { get; }
    
    public bool IsVisible { get; private set; } = true;
    
    private bool _isDestroyed;


    protected ImGuiWindow(bool autoRegister)
    {
        if (autoRegister)
            ImGuiWindowManager.RegisterWindow(this);
    }


    public void ToggleVisibility()
    {
        IsVisible = !IsVisible;
    }

    
    public void Update()
    {
        // Only update if the window is visible.
        if (!IsVisible || _isDestroyed)
            return;
        
        PreUpdate();

        ImGuiNET.ImGui.Begin(Title, Flags);

        DrawContent();
        
        ImGuiNET.ImGui.End();
    }
    
    
    public void Destroy()
    {
        if (_isDestroyed)
            return;
        
        ImGuiWindowManager.UnregisterWindow(this);
        _isDestroyed = true;
    }
    
    
    protected virtual void PreUpdate() { }
    protected virtual void OnDispose() { }

    
    protected abstract void DrawContent();


    public void Dispose()
    {
        OnDispose();
        Destroy();
        GC.SuppressFinalize(this);
    }
}