using KorpiEngine.Core.Logging;
using KorpiEngine.Core.Rendering.Exceptions;
using KorpiEngine.Core.Rendering.Shaders;
using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.Core.Rendering.OpenGL;

public class GLGraphicsProgram : GraphicsProgram
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(GraphicsProgram));

    public static GLGraphicsProgram? CurrentProgram { get; private set; }

    /// <summary>
    /// Initializes a new shaderProgram object.
    /// </summary>
    internal GLGraphicsProgram() : base(GL.CreateProgram())
    {
        
    }


    protected override void Dispose(bool manual)
    {
        if (!manual)
            return;
        
        if (CurrentProgram != null && CurrentProgram.Handle == Handle)
            CurrentProgram = null;
        
        GL.DeleteProgram(Handle);
    }


    /// <summary>
    /// Activate the shaderProgram.
    /// </summary>
    internal void Use()
    {
        if (CurrentProgram != null && CurrentProgram.Handle == Handle)
            return;
        
        GL.UseProgram(Handle);
        CurrentProgram = this;
    }


    /// <summary>
    /// Attach shader object.
    /// </summary>
    /// <param name="glShader">Specifies the shader object to attach.</param>
    internal void AttachShader(GLGraphicsShader glShader)
    {
        GL.AttachShader(Handle, glShader.Handle);
    }


    /// <summary>
    /// Detach shader object.
    /// </summary>
    /// <param name="glShader">Specifies the shader object to detach.</param>
    internal void DetachShader(GLGraphicsShader glShader)
    {
        GL.DetachShader(Handle, glShader.Handle);
    }


    /// <summary>
    /// Link the shaderProgram.
    /// </summary>
    internal void Link()
    {
        Logger.Debug($"Linking shaderProgram '{Handle}'...");
        GL.LinkProgram(Handle);
        CheckLinkStatus();
    }


    /// <summary>
    /// Asserts that no link error occured.
    /// </summary>
    private void CheckLinkStatus()
    {
        // Check link status
        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int linkStatus);
        Logger.DebugFormat("Link status: {0}", linkStatus);

        // Check shaderProgram info log
        string? info = GL.GetProgramInfoLog(Handle);
        if (!string.IsNullOrEmpty(info))
            Logger.InfoFormat("Link  info:\n{0}", info);

        // Log message and throw exception on link error
        if (linkStatus == 1)
            return;
        
        string msg = $"Error linking shaderProgram '{Handle}'";
        Logger.Error(msg);
        throw new ProgramLinkException(msg, info);
    }
}