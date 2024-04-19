using KorpiEngine.Core.Logging;
using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.Core.Rendering.Shaders.Variables;

/// <summary>
/// Represents a shader uniform.
/// </summary>
/// <typeparam name="T">The type of the uniform.</typeparam>
public class Uniform<T> : MaterialProperty where T : struct
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(Uniform<T>));

    /// <summary>
    /// The location of the uniform within the shader shaderProgram.
    /// </summary>
    public int Location { get; private set; }

    /// <summary>
    /// Action used to set the uniform.<br/>
    /// Inputs are the uniforms location and the value to set.
    /// </summary>
    private readonly Action<int, T> _setter;

    /// <summary>
    /// The current value of the shader uniform.
    /// </summary>
    public T Value { get; set; }


    public Uniform(string shaderPropertyName) : this(UniformSetter.Get<T>(), shaderPropertyName)
    {
    }


    protected Uniform(Action<int, T> setter, string shaderPropertyName) : base(shaderPropertyName)
    {
        _setter = setter ?? throw new ArgumentNullException(nameof(setter));
    }


    protected override void InitializeVariable()
    {
        Location = GL.GetUniformLocation(ProgramHandle, ShaderPropertyName);
        Active = Location > -1;
        if (!Active)
            Logger.WarnFormat("Uniform not found or not active: {0}", ShaderPropertyName);
    }


    protected override void BindProperty()
    {
        _setter(Location, Value);
    }
}