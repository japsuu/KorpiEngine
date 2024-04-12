﻿using KorpiEngine.Core.Rendering.Buffers;
using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.Core.Rendering.Textures;

/// <summary>
/// Represents a buffer texture.<br/>
/// The image in this texture (only one image. No mipmapping) is 1-dimensional.
/// The storage for this data comes from a Buffer Object.
/// </summary>
public sealed class TextureBuffer : Texture
{
    public override string Name { get; }
    public override TextureTarget TextureTarget => TextureTarget.TextureBuffer;
    public override bool SupportsMipmaps => false;


    /// <summary>
    /// Creates a buffer texture and uses the given internal format to access a bound buffer, if not specified otherwise.
    /// </summary>
    /// <param name="bufferName"></param>
    /// <param name="internalFormat"></param>
    public TextureBuffer(string bufferName, SizedInternalFormat internalFormat)
        : base(internalFormat, 1)
    {
        Name = bufferName;
    }


    /// <summary>
    /// Binds the given buffer to this texture.<br/>
    /// Applies the internal format specified in the constructor.
    /// </summary>
    /// <param name="buffer">The buffer to bind.</param>
    public void BindBufferToTexture<T>(GLBuffer<T> buffer)
        where T : struct
    {
        BindBufferToTexture(buffer, InternalFormat);
    }


    /// <summary>
    /// Binds the given buffer to this texture using the given internal format.
    /// </summary>
    /// <param name="buffer">The buffer to bind.</param>
    /// <param name="internalFormat">The internal format used when accessing the buffer.</param>
    /// <typeparam name="T">The type of elements in the buffer object.</typeparam>
    public void BindBufferToTexture<T>(GLBuffer<T> buffer, SizedInternalFormat internalFormat)
        where T : struct
    {
        if (!buffer.Initialized) throw new ArgumentException("Can not bind uninitialized buffer to buffer texture.", "buffer");
        GL.BindTexture(TextureTarget.TextureBuffer, Handle);
        GL.TexBuffer(TextureBufferTarget.TextureBuffer, internalFormat, buffer.Handle);
    }
}