using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.Core.Rendering.Shaders.ShaderPrograms;

internal static class ShaderManager
{
    public static ShaderProgram StandardShader3D { get; private set; } = ShaderProgramFactory.Create(
        EngineConstants.INTERNAL_SHADER_BASE_PATH,
        new List<ShaderSourceDescriptor>
        {
            new(ShaderType.VertexShader, "3d/unlit.vert"),
            new(ShaderType.FragmentShader, "3d/unlit.frag")
        }
    );
}