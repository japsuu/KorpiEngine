using System.Text;
using KorpiEngine.Utils;

namespace KorpiEngine.AssetManagement;

/// <summary>
/// If an asset is loaded from an external source,
/// this class contains information about the source.
/// </summary>
public sealed class ExternalAssetInfo(UUID assetID, ushort subID)
{
    /// <summary>
    /// The ID of the asset in the asset database.<br/>
    /// None, if the asset is a runtime asset.
    /// </summary>
    public readonly UUID AssetID = assetID;
    
    /// <summary>
    /// The ID of the asset in the asset database.<br/>
    /// None, if the asset is a runtime asset.
    /// </summary>
    public readonly ushort SubID = subID;
    
    /// <summary>
    /// Whether this asset is the main asset of the external source.
    /// </summary>
    public bool IsMainAsset => SubID == 0;

    
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append("[AssetID: ");
        sb.Append(AssetID);
        
        if (IsMainAsset)
            sb.Append(", Main Asset");
        else
        {
            sb.Append(", SubID: ");
            sb.Append(SubID);
        }

        sb.Append(']');
        return sb.ToString();
    }
}