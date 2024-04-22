﻿namespace KorpiEngine.Core.Rendering;

public abstract class GraphicsFrameBuffer : GraphicsObject
{
    public struct Attachment
    {
        public GraphicsTexture Texture;
        public bool IsDepth;
    }
    
    
    protected GraphicsFrameBuffer(int handle) : base(handle)
    {
    }
}