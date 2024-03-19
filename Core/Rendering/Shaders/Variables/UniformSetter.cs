using KorpiEngine.Core.Rendering.Exceptions;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace KorpiEngine.Core.Rendering.Shaders.Variables;

internal abstract class UniformSetter
{
    protected abstract Type MappedType { get; }

    private class Map<T> : UniformSetter where T : struct
    {
        protected override Type MappedType => typeof(T);

        public readonly Action<int, T> Setter;


        public Map(Action<int, T> setter)
        {
            Setter = setter;
        }
    }


    private UniformSetter()
    {
    }


    private static readonly List<UniformSetter> Setters;


    static UniformSetter()
    {
        Setters = new List<UniformSetter>
        {
            new Map<bool>((i, value) => GL.Uniform1(i, value ? 1 : 0)),
            new Map<int>(GL.Uniform1),
            new Map<uint>(GL.Uniform1),
            new Map<float>(GL.Uniform1),
            new Map<double>(GL.Uniform1),
            //new Map<Half>((h, half) => GL.Uniform1(h, half)),
            new Map<Color>((c, color) => GL.Uniform4(c, color)),
            new Map<Vector2>(GL.Uniform2),
            new Map<Vector3>(GL.Uniform3),
            new Map<Vector4>(GL.Uniform4),
            new Map<Vector2d>((i, vector) => GL.Uniform2(i, vector.X, vector.Y)),
            new Map<Vector2h>((i, vector) => GL.Uniform2(i, vector.X, vector.Y)),
            new Map<Vector3d>((i, vector) => GL.Uniform3(i, vector.X, vector.Y, vector.Z)),
            new Map<Vector3h>((i, vector) => GL.Uniform3(i, vector.X, vector.Y, vector.Z)),
            new Map<Vector4d>((i, vector) => GL.Uniform4(i, vector.X, vector.Y, vector.Z, vector.W)),
            new Map<Vector4h>((i, vector) => GL.Uniform4(i, vector.X, vector.Y, vector.Z, vector.W)),
            new Map<Matrix2>((i, matrix) => GL.UniformMatrix2(i, true, ref matrix)),
            new Map<Matrix3>((i, matrix) => GL.UniformMatrix3(i, true, ref matrix)),
            new Map<Matrix4>((i, matrix) => GL.UniformMatrix4(i, true, ref matrix)),
            new Map<Matrix2x3>((i, matrix) => GL.UniformMatrix2x3(i, true, ref matrix)),
            new Map<Matrix2x4>((i, matrix) => GL.UniformMatrix2x4(i, true, ref matrix)),
            new Map<Matrix3x2>((i, matrix) => GL.UniformMatrix3x2(i, true, ref matrix)),
            new Map<Matrix3x4>((i, matrix) => GL.UniformMatrix3x4(i, true, ref matrix)),
            new Map<Matrix4x2>((i, matrix) => GL.UniformMatrix4x2(i, true, ref matrix)),
            new Map<Matrix4x3>((i, matrix) => GL.UniformMatrix4x3(i, true, ref matrix))
        };
    }


    public static Action<int, T> Get<T>() where T : struct
    {
        UniformSetter? setter = Setters.FirstOrDefault(u => u.MappedType == typeof(T));
        if (setter == null) throw new UniformTypeNotSupportedException(typeof(T));
        return ((Map<T>)setter).Setter;
    }
}