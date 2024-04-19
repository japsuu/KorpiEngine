using System.Diagnostics;
using KorpiEngine.Core.Debugging.OpenGL;
using KorpiEngine.Core.Rendering.Exceptions;
using KorpiEngine.Core.Rendering.Textures;
using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.Core.Rendering.Buffers;

/// <summary>
/// Represents a framebuffer object.
/// </summary>
public class Framebuffer : GraphicsObject
{
    /// <summary>
    /// Creates a new framebuffer object.
    /// </summary>
    public Framebuffer() : base(GL.GenFramebuffer())
    {
    }


    protected override void Dispose(bool manual)
    {
        if (!manual) return;
        GL.DeleteFramebuffer(Handle);
    }


    /// <summary>
    /// Binds this framebuffer.
    /// </summary>
    /// <param name="target">The framebuffer target to bind to.</param>
    public void Bind(FramebufferTarget target)
    {
        GL.BindFramebuffer(target, Handle);
    }


    /// <summary>
    /// Unbind this framebuffer, i.e. bind the default framebuffer.
    /// </summary>
    /// <param name="target">The framebuffer target to bind to.</param>
    public static void Unbind(FramebufferTarget target)
    {
        GL.BindFramebuffer(target, 0);
    }


    /// <summary>
    /// Attaches the given texture level to the an attachment point.
    /// </summary>
    /// <remarks>
    /// If texture is a three-dimensional, cube map array, cube map, one- or two-dimensional array, or two-dimensional multisample array texture
    /// the specified texture level is an array of images and the framebuffer attachment is considered to be layered.
    /// </remarks>
    /// <param name="target">The framebuffer target to bind to.</param>
    /// <param name="attachment">The attachment point to attach to.</param>
    /// <param name="texture">The texture to attach.</param>
    /// <param name="level">The level of the texture to attach.</param>
    public void Attach(FramebufferTarget target, FramebufferAttachment attachment, Texture texture, int level = 0)
    {
        texture.AssertLevel(level);
        AssertActive(target);
        GL.FramebufferTexture(target, attachment, texture.Handle, level);
        CheckState(target);
    }


    /// <summary>
    /// Attaches a single layer of the given texture level to an attachment point.
    /// </summary>
    /// <remarks>
    /// Note that for cube maps and cube map arrays the <paramref name="layer"/> parameter actually indexes the layer-faces.<br/>
    /// Thus for cube maps the layer parameter equals the face to be bound.<br/>
    /// For cube map arrays the layer parameter can be calculated as 6 * arrayLayer + face, which is done automatically when using
    /// the corresponding overload <see cref="Attach(FramebufferTarget, FramebufferAttachment, TextureCubemapArray, int, int, int)"/>.
    /// </remarks>
    /// <param name="target">The framebuffer target to bind to.</param>
    /// <param name="attachment">The attachment point to attach to.</param>
    /// <param name="texture">The texture to attach.</param>
    /// <param name="layer">The layer of the texture to attach.</param>
    /// <param name="level">The level of the texture to attach.</param>
    public void Attach(FramebufferTarget target, FramebufferAttachment attachment, LayeredTexture texture, int layer, int level = 0)
    {
        texture.AssertLevel(level);
        AssertActive(target);
        GL.FramebufferTextureLayer(target, attachment, texture.Handle, level, layer);
        CheckState(target);
    }


    /// <summary>
    /// Attaches a single face of the given cube map texture level to the an attachment point.
    /// </summary>
    /// <param name="target">The framebuffer target to bind to.</param>
    /// <param name="attachment">The attachment point to attach to.</param>
    /// <param name="texture">The texture to attach.</param>
    /// <param name="face">The cube map face of the texture to attach.</param>
    /// <param name="level">The level of the texture to attach.</param>
    public void Attach(FramebufferTarget target, FramebufferAttachment attachment, TextureCubemap texture, int face, int level = 0)
    {
        Attach(target, attachment, (LayeredTexture)texture, face, level);
    }


    /// <summary>
    /// Attaches a single face of the given cube map array texture level to an attachment point.
    /// </summary>
    /// <param name="target">The framebuffer target to bind to.</param>
    /// <param name="attachment">The attachment point to attach to.</param>
    /// <param name="texture">The texture to attach.</param>
    /// <param name="arrayLayer">The layer of the texture to attach.</param>
    /// <param name="face">The cube map face of the texture to attach.</param>
    /// <param name="level">The level of the texture to attach.</param>
    public void Attach(FramebufferTarget target, FramebufferAttachment attachment, TextureCubemapArray texture, int arrayLayer, int face, int level = 0)
    {
        Attach(target, attachment, (LayeredTexture)texture, 6 * arrayLayer + face, level);
    }


    /// <summary>
    /// Attaches the render buffer to the given attachment point.
    /// </summary>
    /// <param name="target">The framebuffer target to bind to.</param>
    /// <param name="attachment">The attachment point to attach to.</param>
    /// <param name="renderbuffer">Render buffer to attach.</param>
    public void Attach(FramebufferTarget target, FramebufferAttachment attachment, Renderbuffer renderbuffer)
    {
        AssertActive(target);
        GL.FramebufferRenderbuffer(target, attachment, RenderbufferTarget.Renderbuffer, renderbuffer.Handle);
        CheckState(target);
    }


    /// <summary>
    /// Detaches the currently attached texture from the given attachment point.
    /// </summary>
    /// <param name="attachment">The attachment point to detach from.</param>
    /// <param name="target">The framebuffer target to bind to.</param>
    public void DetachTexture(FramebufferTarget target, FramebufferAttachment attachment)
    {
        AssertActive(target);
        GL.FramebufferTexture(target, attachment, 0, 0);
        CheckState(target);
    }


    /// <summary>
    /// Detaches the currently attached render buffer from the given attachment point.
    /// </summary>
    /// <param name="target">The framebuffer target to bind to.</param>
    /// <param name="attachment">The attachment point to detach from.</param>
    public void DetachRenderbuffer(FramebufferTarget target, FramebufferAttachment attachment)
    {
        AssertActive(target);
        GL.FramebufferRenderbuffer(target, attachment, RenderbufferTarget.Renderbuffer, 0);
        CheckState(target);
    }


    /// <summary>
    /// Check if the current framebuffer status is "framebuffer complete", throws on error.
    /// </summary>
    /// <param name="target">The framebuffer target to bind to.</param>
    [Conditional("DEBUG")]
    public void CheckState(FramebufferTarget target)
    {
        GLAssertUtility.Assert("Error on framebuffer attach/detach");
        GLAssertUtility.Assert(GL.CheckFramebufferStatus(target), FramebufferErrorCode.FramebufferComplete, "Framebuffer is not framebuffer complete.");
    }


    /// <summary>
    /// Throws an <see cref="ObjectNotBoundException"/> if this framebuffer is not the currently active one.
    /// </summary>
    /// <param name="target">The framebuffer target to bind to.</param>
    [Conditional("DEBUG")]
    public void AssertActive(FramebufferTarget target)
    {
        GetPName binding = target switch
        {
            FramebufferTarget.ReadFramebuffer => GetPName.ReadFramebufferBinding,
            FramebufferTarget.DrawFramebuffer => GetPName.DrawFramebufferBinding,
            FramebufferTarget.Framebuffer => GetPName.FramebufferBinding,
            _ => throw new ArgumentOutOfRangeException()
        };

        GL.GetInteger(binding, out int activeHandle);
        if (activeHandle != Handle) throw new ObjectNotBoundException("Can not access an unbound framebuffer. Call Framebuffer.Bind() first.");
    }
}