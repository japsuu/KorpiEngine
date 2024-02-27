using KorpiEngine.Core.Rendering.Shaders.Variables;
using OpenTK.Mathematics;

namespace KorpiEngine.Core.Rendering.Shaders.ShaderPrograms;

public class MVPShaderProgram : ShaderProgram
{
    public Uniform<Matrix4> ModelMat { get; protected set; } = null!;
    public Uniform<Matrix4> ViewMat { get; protected set; } = null!;
    public Uniform<Matrix4> ProjMat { get; protected set; } = null!;


    protected MVPShaderProgram()
    {
        MatrixManager.ProjectionMatrixChanged += UpdateProjectionMatrix;
        MatrixManager.ViewMatrixChanged += UpdateViewMatrix;
    }
    
    
    protected virtual void UpdateProjectionMatrix(Matrix4 projectionMatrix)
    {
        Use();
        ProjMat.Set(projectionMatrix);
    }


    protected virtual void UpdateViewMatrix(Matrix4 viewMatrix)
    {
        Use();
        ViewMat.Set(viewMatrix);
    }


    protected override void Dispose(bool disposing)
    {
        MatrixManager.ProjectionMatrixChanged -= UpdateProjectionMatrix;
        MatrixManager.ViewMatrixChanged -= UpdateViewMatrix;
        
        base.Dispose(disposing);
    }
}