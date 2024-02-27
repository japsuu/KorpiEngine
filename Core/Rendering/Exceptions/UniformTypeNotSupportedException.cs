using System.Runtime.Serialization;

namespace KorpiEngine.Core.Rendering.Exceptions
{
    /// <summary>
    /// The exception that is thrown when the generic type parameter used for an instance of <see cref="Uniform{T}"/> is not supported.
    /// </summary>
    [Serializable]
    public class UniformTypeNotSupportedException : OpenGLException
    {
        /// <summary>
        /// The unsupported type parameter to <see cref="Uniform{T}"/> which caused the initialization to fail.
        /// </summary>
        public readonly Type UniformType;

        internal UniformTypeNotSupportedException(Type uniformType)
            : base($"Uniforms of type {uniformType.Name} are not supported")
        {
            UniformType = uniformType;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("UniformType", UniformType);
        }
    }
}