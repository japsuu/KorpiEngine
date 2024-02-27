using KorpiEngine.Core.Logging;
using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.Core.Rendering.Shaders.Variables;

/// <summary>
/// Represents a fragment shader output.<br/>
/// TODO: implement methods to bind output to a specific attachment
/// see glBindFragDataLocation, glDrawBuffers and http://stackoverflow.com/questions/1733838/fragment-shaders-output-variables
/// </summary>
public sealed class FragData : ProgramVariable
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(FragData));

    /// <summary>
    /// The location of the output.
    /// </summary>
    public int Location { get; private set; }


    internal FragData()
    {
    }


    internal override void OnLink()
    {
        //TODO: find out what GL.GetFragDataIndex(); does
        Location = GL.GetFragDataLocation(ProgramHandle, Name);
        if (Location == -1) Logger.WarnFormat("Output variable not found or not active: {0}", Name);
    }
}