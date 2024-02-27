using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.Core.Rendering.Shaders.Variables;

/// <summary>
/// Represents a shader storage buffer object (SSBO) binding.
/// </summary>
public sealed class ShaderStorage : BufferBinding
{
    internal ShaderStorage()
        : base(BufferRangeTarget.ShaderStorageBuffer, ProgramInterface.ShaderStorageBlock)
    {
    }


    internal override void OnLink()
    {
        base.OnLink();

        //TODO: find out if the current binding point can be queried, like it can be for uniform blocks
        // set the binding point to the blocks index
        if (Active) ChangeBinding(Index);
    }


    /// <summary>
    /// Assigns a binding point to this shader storage block.
    /// </summary>
    /// <param name="binding"></param>
    public override void ChangeBinding(int binding)
    {
        base.ChangeBinding(binding);
        GL.ShaderStorageBlockBinding(ProgramHandle, Index, binding);
    }
}