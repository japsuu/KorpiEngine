﻿using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.Core.Rendering.Buffers;

/// <summary>
/// Represents a renderbuffer object.
/// </summary>
public class Renderbuffer : GraphicsObject
{
    /// <summary>
    /// Creates a new renderbuffer object.
    /// </summary>
    public Renderbuffer() : base(GL.GenRenderbuffer())
    {
    }

    protected override void Dispose(bool manual)
    {
        if (!manual) return;
        GL.DeleteRenderbuffer(Handle);
    }

    /// <summary>
    /// Initializes the renderbuffer with the given parameters.
    /// </summary>
    /// <param name="storage">Specifies the internal format.</param>
    /// <param name="width">The width of the renderbuffer.</param>
    /// <param name="height">The height of the renderbuffer.</param>
    public void Init(RenderbufferStorage storage, int width, int height)
    {
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, Handle);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, storage, width, height);
    }
}