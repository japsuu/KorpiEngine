#if TOOLS
using System.Globalization;
using ImGuiNET;
using KorpiEngine.Core.Rendering;

namespace KorpiEngine.Core.UI.DearImGui;

public class DebugStatsWindow : ImGuiWindow
{
    public override string Title => "Debug Statistics";
    protected override ImGuiWindowFlags Flags => ImGuiWindowFlags.AlwaysAutoResize;

    private readonly NumberFormatInfo _largeNumberFormat;

    private bool _shouldCalcMinMaxFps;
    private float _minFps = float.MaxValue;
    private float _maxFps = float.MinValue;

    
    public DebugStatsWindow() : base(true)
    {
        _largeNumberFormat = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        _largeNumberFormat.NumberGroupSeparator = " ";
    }
    

    protected sealed override void DrawContent()
    {
        string renderedTris = Graphics.Device.RenderedTriangles.ToString("#,0", _largeNumberFormat);
        string renderedVerts = Graphics.Device.RenderedVertices.ToString("#,0", _largeNumberFormat);
        string drawCalls = Graphics.Device.DrawCalls.ToString("#,0", _largeNumberFormat);
        string textureSwaps = Graphics.Device.TextureSwaps.ToString("#,0", _largeNumberFormat);
        
        float averageFps = Time.FrameRate;
        double frameTime = Time.DeltaTimeDouble * 1000f;
        if (ImGui.Checkbox("Calculate min/max FPS", ref _shouldCalcMinMaxFps))
        {
            _minFps = float.MaxValue;
            _maxFps = float.MinValue;
        }

        ImGui.Text("Rendering");
        ImGui.Text($"{averageFps:F1} fps ({frameTime:F1} ms/frame)");
        if (_shouldCalcMinMaxFps)
        {
            if (averageFps < _minFps)
                _minFps = averageFps;
            if (averageFps > _maxFps)
                _maxFps = averageFps;
            
            ImGui.Text($"Min: {_minFps:F1} fps");
            ImGui.Text($"Max: {_maxFps:F1} fps");
        }
        ImGui.Text($"Draw Calls = {drawCalls}");
        ImGui.Text($"Triangles = {renderedTris}");
        ImGui.Text($"Vertices = {renderedVerts}");
        ImGui.Text($"Texture Swaps = {textureSwaps}");
    }
}
#endif