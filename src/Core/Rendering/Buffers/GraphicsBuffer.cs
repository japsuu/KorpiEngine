﻿namespace KorpiEngine.Rendering;

public abstract class GraphicsBuffer(int handle) : GraphicsObject(handle)
{
    internal abstract int SizeInBytes { get; }
}