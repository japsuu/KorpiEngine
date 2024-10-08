﻿using KorpiEngine.Rendering;
using KorpiEngine.Utils;
using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.OpenGL;

internal sealed class GLFrameBuffer : GraphicsFrameBuffer
{
    private static readonly DrawBuffersEnum[] Buffers =
    [
        DrawBuffersEnum.ColorAttachment0,
        DrawBuffersEnum.ColorAttachment1,
        DrawBuffersEnum.ColorAttachment2,
        DrawBuffersEnum.ColorAttachment3,
        DrawBuffersEnum.ColorAttachment4,
        DrawBuffersEnum.ColorAttachment5,
        DrawBuffersEnum.ColorAttachment6,
        DrawBuffersEnum.ColorAttachment7,
        DrawBuffersEnum.ColorAttachment8,
        DrawBuffersEnum.ColorAttachment9,
        DrawBuffersEnum.ColorAttachment10,
        DrawBuffersEnum.ColorAttachment11,
        DrawBuffersEnum.ColorAttachment12,
        DrawBuffersEnum.ColorAttachment13,
        DrawBuffersEnum.ColorAttachment14,
        DrawBuffersEnum.ColorAttachment15
    ];


    public GLFrameBuffer(IList<Attachment> attachments) : base(GL.GenFramebuffer())
    {
        int texCount = attachments.Count;
        if (texCount < 1 || texCount > GraphicsInfo.MaxFramebufferColorAttachments)
            throw new ArgumentOutOfRangeException(nameof(attachments),
                $"[FrameBuffer] Invalid number of textures! [0-{GraphicsInfo.MaxFramebufferColorAttachments}]");

        // Generate FBO
        if (Handle <= 0)
            throw new GLException("[FrameBuffer] Failed to generate new FrameBuffer.");

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);

        // Generate textures
        for (int i = 0; i < texCount; i++)
        {
            if (!attachments[i].IsDepth)
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0 + i, (attachments[i].Texture as GLTexture)!.Target, (attachments[i].Texture as GLTexture)!.Handle, 0);
            else
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, (attachments[i].Texture as GLTexture)!.Handle, 0);
        }
        GL.DrawBuffers(texCount, Buffers);

        if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
            throw new GLException("RenderTexture: [ID {fboId}] RenderTexture object creation failed.");

        // Unbind FBO.
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }


    protected override void DisposeResources()
    {
        GL.DeleteFramebuffer(Handle);
    }
}