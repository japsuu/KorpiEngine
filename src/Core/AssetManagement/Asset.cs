using System.Text;
using KorpiEngine.Utils;

namespace KorpiEngine.AssetManagement;

/// <summary>
/// Base class for "resource types", serving primarily as data containers.<br/>
/// </summary>
///
/// <remarks>
/// Runtime assets can be manually destroyed, but are also automatically collected by the GC.<br/><br/>
/// External assets cannot be manually destroyed, but can be unloaded from the AssetManager.
/// </remarks>
public abstract class Asset : EngineObject
{
    /// <summary>
    /// Whether assets can be manually destroyed if they are external.
    /// </summary>
    internal static bool AllowManualExternalDestroy { private get; set; } = false;
    
    /// <summary>
    /// Whether the asset has been loaded from an external source.<br/>
    /// If true, <see cref="ExternalInfo"/> will also be set.<br/>
    /// If <see cref="EngineObject.Destroy"/> is called on an external asset,
    /// the asset will be removed from the asset database.
    /// </summary>
    public bool IsExternal { get; private set; }
    
    /// <summary>
    /// If the asset is external, this will contain the information about the external source.
    /// </summary>
    public ExternalAssetInfo? ExternalInfo { get; private set; }


    #region Creation & Destruction

    protected Asset(string name) : base(name)
    {
        
    }

    #endregion
    
    
    internal void SetExternalInfo(UUID assetID, ushort subID)
    {
        IsExternal = true;
        ExternalInfo = new ExternalAssetInfo(assetID, subID);
    }


    protected sealed override void OnDispose(bool manual)
    {
        if (IsExternal)
        {
            if (!manual)
                throw new InvalidOperationException($"External asset '{this}' disposed by GC. This is an engine bug.");
            
            IsExternal = false;
            ExternalInfo = null;
        }
        
        OnDestroy(manual);
    }


    protected internal override bool AllowDestroy()
    {
        bool allowed = !IsExternal || AllowManualExternalDestroy;
        
        if (!allowed)
            Application.Logger.Warn($"Tried to unexpectedly destroy external asset '{this}' without permission. This is not allowed.");
        
        return allowed;
    }


    /// <summary>
    /// Releases all owned resources.
    /// Guaranteed to be called only once.<br/><br/>
    /// 
    /// Example implementation:
    /// <code>
    /// protected override void OnDestroy(bool manual)
    /// {
    ///     if (manual)
    ///     {
    ///         // Dispose managed resources
    ///     }
    ///     
    ///     // Dispose unmanaged resources
    /// }
    /// </code>
    /// </summary>
    /// <param name="manual">True, if the call is performed explicitly by calling a destroy method.
    /// Managed and unmanaged resources can be disposed.<br/>
    /// 
    /// False, if caused by the GC and therefore from another thread.
    /// Only unmanaged resources can be disposed.</param>
    protected virtual void OnDestroy(bool manual)
    {
        
    }


    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append(base.ToString());
        
        if (ExternalInfo != null)
            sb.Append($" {ExternalInfo.ToString()}");
        else
            sb.Append(" [Runtime Asset]");

        return sb.ToString();
    }
}