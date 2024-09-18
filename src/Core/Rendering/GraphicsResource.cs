using KorpiEngine.Utils;

namespace KorpiEngine.Rendering;

/// <summary>
/// Represents a graphics (GPU) resource.<br/>
/// Can be derived to inherit the dispose pattern.
/// </summary>
internal abstract class GraphicsResource : SafeDisposable
{
    protected override bool RequiresMainThreadDispose => true;
    
    
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    protected GraphicsResource()
    {
    }
}