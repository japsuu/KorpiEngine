using ImGuiNET;
using KorpiEngine.Core.Logging;

namespace KorpiEngine.Core.UI.DearImGui;

public abstract class ImGuiWindow
{
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

        ImGui.Begin(Title, Flags);

        DrawContent();
        
        ImGui.End();
    }
    
    
    public void Destroy()
    {
        if (_isDestroyed)
            return;
        
        OnDestroy();
        
        ImGuiWindowManager.UnregisterWindow(this);
        _isDestroyed = true;
    }
    
    
    protected virtual void PreUpdate() { }
    protected virtual void OnDestroy() { }

    
    protected abstract void DrawContent();
}