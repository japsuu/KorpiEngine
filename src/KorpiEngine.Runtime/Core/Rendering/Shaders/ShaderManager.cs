using KorpiEngine.Core.Rendering.OpenGL;
using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.Core.Rendering.Shaders;

internal static class ShaderManager
{
    public static GraphicsProgram StandardShader3D { get; private set; } = Graphics.Driver.CompileProgram(
        new List<ShaderSourceDescriptor>
        {
            new(ShaderType.VertexShader, "3d/unlit.vert"),
            new(ShaderType.FragmentShader, "3d/unlit.frag")
        }
    );
}