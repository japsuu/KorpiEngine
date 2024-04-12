using System.Reflection;
using KorpiEngine.Core.Logging;
using KorpiEngine.Core.Rendering.Buffers;
using KorpiEngine.Core.Rendering.Shaders.ShaderPrograms;
using OpenTK.Graphics.OpenGL4;

namespace KorpiEngine.Core.Rendering.Shaders.Variables;

/// <summary>
/// Represents a shader buffer binding point identified by its resource index.
/// </summary>
public abstract class BufferBinding : MaterialProperty
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(BufferBinding));

    /// <summary>
    /// The target to use when binding to this point.
    /// </summary>
    public readonly BufferRangeTarget BindingTarget;

    /// <summary>
    /// The resource index of this binding point.
    /// </summary>
    public int Index { get; private set; }

    /// <summary>
    /// Current binding point
    /// </summary>
    protected int Binding;

    private int _bufferHandle;
    private bool _isRange;
    private int _rangeOffset;
    private int _rangeSize;

    private readonly ProgramInterface _programInterface;


    internal BufferBinding(BufferRangeTarget bindingTarget, ProgramInterface programInterface)
    {
        BindingTarget = bindingTarget;
        _programInterface = programInterface;
    }


    protected override void InitializeVariable(ShaderProgram shaderProgram, PropertyInfo property)
    {
        Index = GL.GetProgramResourceIndex(ProgramHandle, _programInterface, Name);
        Active = Index > -1;
        Binding = -1;
        if (!Active)
            Logger.WarnFormat("Binding block not found or not active: {0}", Name);
    }


    /// <summary>
    /// Assigns a binding point.
    /// </summary>
    public virtual void ChangeBinding(int binding)
    {
        Binding = binding;
    }


    /// <summary>
    /// Binds a buffer to this binding point.
    /// </summary>
    /// <typeparam name="T">The type of the container elements.</typeparam>
    /// <param name="buffer">The buffer to bind.</param>
    public void SetBuffer<T>(Buffer<T> buffer) where T : struct
    {
        _bufferHandle = buffer.Handle;
        _isRange = false;
    }


    /// <summary>
    /// Binds a buffer to this binding point.
    /// </summary>
    /// <typeparam name="T">The type of the container elements.</typeparam>
    /// <param name="buffer">The buffer to bind.</param>
    /// <param name="offset">The starting offset in basic machine units into the buffer object buffer. </param>
    /// <param name="size">The amount of data in machine units that can be read from the buffer object while used as an indexed target. </param>
    public void SetBuffer<T>(Buffer<T> buffer, int offset, int size) where T : struct
    {
        _bufferHandle = buffer.Handle;
        _isRange = true;
        _rangeOffset = offset;
        _rangeSize = size;
    }


    protected override void BindProperty()
    {
        if (!Active)
            return;
        
        if (_isRange)
            GL.BindBufferRange(BindingTarget, Binding, _bufferHandle, (IntPtr)_rangeOffset, (IntPtr)_rangeSize);
        else
            GL.BindBufferBase(BindingTarget, Binding, _bufferHandle);
    }


    /// <summary>
    /// Unbinds any buffer from this binding point.
    /// </summary>
    public void Unbind()
    {
        if (!Active) return;
        GL.BindBufferBase(BindingTarget, Binding, 0);
    }
}