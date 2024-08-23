using KorpiEngine.Core.Logging;
using KorpiEngine.Core.Rendering.Exceptions;
using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.Core.Debugging.OpenGL;

public class GLAssertUtility
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(GLAssertUtility));


    public static void Assert(string errorMessage)
    {
        Assert(GL.GetError(), ErrorCode.NoError, errorMessage);
    }


    public static void Assert(ErrorCode desiredErrorCode, string errorMessage)
    {
        Assert(GL.GetError(), desiredErrorCode, errorMessage);
    }


    public static void Assert<T>(T value, T desiredValue, string errorMessage)
    {
        if (desiredValue != null && desiredValue.Equals(value)) return;
        Logger.Error($"Assert failed: {value}\n{errorMessage}");
        throw new OpenGLException($"ErrorCode: {value}\n{errorMessage}");
    }
}