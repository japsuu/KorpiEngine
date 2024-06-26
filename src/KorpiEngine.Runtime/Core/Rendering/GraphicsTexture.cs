﻿using KorpiEngine.Core.Rendering.Primitives;

namespace KorpiEngine.Core.Rendering;

internal abstract class GraphicsTexture : GraphicsObject
{
    public abstract TextureType Type { get; protected set; }
    
    
    protected GraphicsTexture(int handle) : base(handle)
    {
    }
}