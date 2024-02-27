using System.Reflection;
using KorpiEngine.Core.Logging;
using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.Core.Rendering.Shaders.Variables;

/// <summary>
/// Represents a vertex attribute.
/// </summary>
public sealed class VertexAttrib : ProgramVariable
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(VertexAttrib));

    /// <summary>
    /// The vertex attributes location within the shader.
    /// </summary>
    public int Index { get; private set; }

    /// <summary>
    /// The number components to read.
    /// </summary>
    public int Components { get; private set; }

    /// <summary>
    /// The type of each component.
    /// </summary>
    public VertexAttribPointerType Type { get; private set; }

    /// <summary>
    /// Specifies whether the components should be normalized.
    /// </summary>
    public bool Normalized { get; private set; }


    internal VertexAttrib()
    {
    }


    internal override void Initialize(ShaderPrograms.ShaderProgram shaderProgram, PropertyInfo property)
    {
        base.Initialize(shaderProgram, property);
        VertexAttribAttribute attribute = property.GetCustomAttributes<VertexAttribAttribute>(false).FirstOrDefault() ?? new VertexAttribAttribute();
        Components = attribute.Components;
        Type = attribute.Type;
        Normalized = attribute.Normalized;
        if (attribute.Index > 0) BindAttribLocation(attribute.Index);
    }


    public void BindAttribLocation(int index)
    {
        Index = index;
        GL.BindAttribLocation(ProgramHandle, index, Name);
    }


    internal override void OnLink()
    {
        Index = GL.GetAttribLocation(ProgramHandle, Name);
        Active = Index > -1;
        if (!Active) Logger.WarnFormat("Vertex attribute not found or not active: {0}", Name);
    }
}