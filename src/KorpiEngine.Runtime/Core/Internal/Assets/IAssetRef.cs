﻿namespace KorpiEngine.Core.Internal.Assets;

public interface IAssetRef
{
    Guid AssetID { get; set; }
    ushort FileID { get; set; }
    string Name { get; }
    bool IsAvailable { get; }
    bool IsRuntimeResource { get; }
    bool IsLoaded { get; }
    bool IsExplicitNull { get; }
    Type InstanceType { get; }

    object? GetInstance();

    void SetInstance(object? obj);
}