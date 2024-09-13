using System.Text;
using KorpiEngine.Tools;
using KorpiEngine.Utils;

namespace KorpiEngine.AssetManagement;

/// <summary>
/// Base class for "resource types", serving primarily as data containers.
/// Assets can be manually disposed, but are also automatically collected by the GC.
/// </summary>
public abstract class Asset : EngineObject
{
    /// <summary>
    /// The ID of the asset in the asset database.
    /// None, if the asset is a runtime asset.
    /// </summary>
    public UUID ExternalAssetID { get; internal set; } = UUID.Empty;
    
    /// <summary>
    /// Whether the asset has been loaded from an external source.
    /// If true, <see cref="ExternalAssetID"/> will be set.
    /// </summary>
    public bool IsExternal { get; internal set; }


    #region Creation & Destruction

    protected Asset(string name) : base(name)
    {
        
    }

    #endregion
    
    
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append(base.ToString());
        sb.Append("[ExternalAssetID: ");
        sb.Append(ExternalAssetID == UUID.Empty ? "None" : ExternalAssetID.ToString());
        sb.Append(']');
        
        return sb.ToString();
    }
}