using KorpiEngine.Mathematics;
using KorpiEngine.Rendering;

namespace KorpiEngine.Utils;

/// <summary>
/// Contains information about the display the application is running on.
/// </summary>
public static class DisplayInfo
{
    /// <summary>
    /// The current display resolution.
    /// </summary>
    public static Int2 Resolution { get; private set; }
    
    
    internal static void Update(DisplayState state)
    {
        Resolution = state.Resolution;
    }
}