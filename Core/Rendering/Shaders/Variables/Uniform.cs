using KorpiEngine.Core.Logging;
using KorpiEngine.Core.Rendering.Shaders.ShaderPrograms;
using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.Core.Rendering.Shaders.Variables;

/// <summary>
/// Represents a uniform.
/// </summary>
/// <typeparam name="T">The type of the uniform.</typeparam>
public class Uniform<T> : ProgramVariable where T : struct
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
    /// The current value of the uniform.
    /// </summary>
    private T _value;

    /// <summary>
    /// Gets or sets the current value of the shader uniform.
    /// </summary>
    public T Value
    {
        get => _value;
        set => Set(value);
    }


    internal Uniform()
        : this(UniformSetter.Get<T>())
    {
    }


    public Uniform(Action<int, T> setter)
    {
        _setter = setter ?? throw new ArgumentNullException(nameof(setter));
    }


    internal override void OnLink()
    {
        Location = GL.GetUniformLocation(ProgramHandle, Name);
        Active = Location > -1;
        if (!Active)
            Logger.WarnFormat("Uniform not found or not active: {0}", Name);
    }


    /// <summary>
    /// Sets the given value to the shaderProgram uniform.<br/>
    /// Must be called on an active shaderProgram, i.e. after <see cref="ShaderProgram"/>.<see cref="ShaderProgram.Use()"/>.
    /// </summary>
    /// <param name="value">The value to set.</param>
    public void Set(T value)
    {
        ShaderProgram.AssertActive();
        _value = value;
        if (Active)
            _setter(Location, value);
    }
}