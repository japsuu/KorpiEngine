using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.Core.Rendering.Shaders.Variables;

/// <summary>
/// Represents a uniform buffer object (UBO) binding.
/// </summary>
public sealed class UniformBuffer : BufferBinding
{
    internal UniformBuffer(string shaderPropertyName) : base(BufferRangeTarget.UniformBuffer, ProgramInterface.UniformBlock, shaderPropertyName)
    {
    }


    protected override void InitializeVariable()
    {
        base.InitializeVariable();
        
        // Retrieve the default binding point
        if (Active)
            GL.GetActiveUniformBlock(ProgramHandle, Index, ActiveUniformBlockParameter.UniformBlockBinding, out Binding);
    }


    /// <summary>
    /// Assigns a binding point to this uniform block.
    /// </summary>
    /// <param name="binding">The binding point to assign.</param>
    public override void ChangeBinding(int binding)
    {
        base.ChangeBinding(binding);
        if (!Active) return;
        GL.UniformBlockBinding(ProgramHandle, Index, binding);
    }


    /// <summary>
    /// Retrieves the total size of the uniform block.
    /// </summary>
    /// <returns>The total size of the uniform block.</returns>
    public int GetBlockSize()
    {
        int size;
        GL.GetActiveUniformBlock(ProgramHandle, Index, ActiveUniformBlockParameter.UniformBlockDataSize, out size);
        return size;
    }


    /// <summary>
    /// Retrieves the offsets of the uniforms within the block to the start of the block.
    /// </summary>
    /// <param name="offsets">The offsets of the uniforms within the block.</param>
    public void GetBlockOffsets(out int[] offsets)
    {
        GL.GetActiveUniformBlock(ProgramHandle, Index, ActiveUniformBlockParameter.UniformBlockActiveUniforms, out int uniforms);
        int[] indices = new int[uniforms];
        GL.GetActiveUniformBlock(ProgramHandle, Index, ActiveUniformBlockParameter.UniformBlockActiveUniformIndices, indices);
        offsets = new int[uniforms];
        GL.GetActiveUniforms(ProgramHandle, uniforms, indices, ActiveUniformParameter.UniformOffset, offsets);
    }
}