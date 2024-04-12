using System.Reflection;
using KorpiEngine.Core.Logging;
using KorpiEngine.Core.Rendering.Shaders.ShaderPrograms;
using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.Core.Rendering.Shaders.Variables;

/// <summary>
/// Represents a fragment shader output.<br/>
/// TODO: implement methods to bind output to a specific attachment
/// see glBindFragDataLocation, glDrawBuffers and http://stackoverflow.com/questions/1733838/fragment-shaders-output-variables
/// </summary>
public sealed class FragData : MaterialProperty
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(FragData));

    /// <summary>
    /// The location of the output.
    /// </summary>
    public int Location { get; private set; }


    internal FragData()
    {
    }


    protected override void InitializeVariable(ShaderProgram shaderProgram, PropertyInfo property)
    {
        //TODO: find out what GL.GetFragDataIndex(); does
        Location = GL.GetFragDataLocation(ProgramHandle, Name);
        if (Location == -1)
            Logger.WarnFormat("Output variable not found or not active: {0}", Name);
    }


    protected override void BindProperty()
    {
        
    }
}