// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace KorpiEngine.Core.API;

/// <summary>
/// A structure encapsulating four single precision floating point values and provides hardware accelerated methods.
/// </summary>
public struct Vector4 : IEquatable<Vector4>, IFormattable
{
    public double X;
    public double Y;
    public double Z;
    public double W;


    #region Constructors

    /// <summary> Constructs a vector whose elements are all the single specified value. </summary>
    public Vector4(double value) : this(value, value, value, value)
    {
    }


    /// <summary> Constructs a vector with the given individual elements. </summary>
    public Vector4(double x, double y, double z, double w)
    {
        W = w;
        X = x;
        Y = y;
        Z = z;
    }


    /// <summary> Constructs a Vector4 from the given Vector2 and a Z and W component. </summary>
    public Vector4(Vector2 value, double z = 0.0, double w = 0.0)
    {
        X = value.X;
        Y = value.Y;
        Z = z;
        W = w;
    }


    /// <summary> Constructs a Vector4 from the given Vector3 and a W component. </summary>
    public Vector4(Vector3 value, double w = 0.0)
    {
        X = value.X;
        Y = value.Y;
        Z = value.Z;
        W = w;
    }

    #endregion Constructors


    #region Public Instance Properties

    public Vector4 Normalized => Normalize(this);

    public double Magnitude => Mathf.Sqrt(X * X + Y * Y + Z * Z + W * W);

    public double SqrMagnitude => X * X + Y * Y + Z * Z + W * W;

    public Vector3 Xyz => new(X, Y, Z);

    public double this[int index]
    {
        get
        {
            switch (index)
            {
                case 0:
                    return X;
                case 1:
                    return Y;
                case 2:
                    return Z;
                case 3:
                    return W;
                default:
                    throw new IndexOutOfRangeException("Invalid Vector4 index!");
            }
        }

        set
        {
            switch (index)
            {
                case 0:
                    X = value;
                    break;
                case 1:
                    Y = value;
                    break;
                case 2:
                    Z = value;
                    break;
                case 3:
                    W = value;
                    break;
                default:
                    throw new IndexOutOfRangeException("Invalid Vector4 index!");
            }
        }
    }

    #endregion


    #region Public Static Properties

    public static Vector4 zero => new();
    public static Vector4 one => new(1.0, 1.0, 1.0, 1.0);
    public static Vector4 right => new(1.0, 0.0, 0.0, 0.0);
    public static Vector4 up => new(0.0, 1.0, 0.0, 0.0);
    public static Vector4 forward => new(0.0, 0.0, 1.0, 0.0);
    public static Vector4 unitw => new(0.0, 0.0, 0.0, 1.0);

    public static Vector4 infinity = new(Mathf.INFINITY, Mathf.INFINITY, Mathf.INFINITY, Mathf.INFINITY);

    #endregion Public Static Properties


    #region Public Instance methods

    public System.Numerics.Vector4 ToFloat() => new((float)X, (float)Y, (float)Z, (float)W);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Scale(Vector4 scale)
    {
        X *= scale.X;
        Y *= scale.Y;
        Z *= scale.Z;
        W *= scale.W;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Normalize()
    {
        double ls = X * X + Y * Y + Z * Z + W * W;
        double invNorm = 1.0 / (double)Math.Sqrt((double)ls);
        X *= invNorm;
        Y *= invNorm;
        Z *= invNorm;
        W *= invNorm;
    }


    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode()
    {
        int hash = X.GetHashCode();
        hash = HashCode.Combine(hash, Y.GetHashCode());
        hash = HashCode.Combine(hash, Z.GetHashCode());
        hash = HashCode.Combine(hash, W.GetHashCode());
        return hash;
    }


    /// <summary>
    /// Returns a boolean indicating whether the given Object is equal to this Vector4 instance.
    /// </summary>
    /// <param name="obj">The Object to compare against.</param>
    /// <returns>True if the Object is equal to this Vector4; False otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj)
    {
        if (obj is not Vector4)
            return false;
        return Equals((Vector4)obj);
    }


    /// <summary>
    /// Returns a boolean indicating whether the given Vector4 is equal to this Vector4 instance.
    /// </summary>
    /// <param name="other">The Vector4 to compare this instance to.</param>
    /// <returns>True if the other Vector4 is equal to this instance; False otherwise.</returns>
    public bool Equals(Vector4 other) =>
        X == other.X
        && Y == other.Y
        && Z == other.Z
        && W == other.W;


    /// <summary>
    /// Returns a String representing this Vector4 instance.
    /// </summary>
    /// <returns>The string representation.</returns>
    public override string ToString() => ToString("G", CultureInfo.CurrentCulture);


    /// <summary>
    /// Returns a String representing this Vector4 instance, using the specified format to format individual elements.
    /// </summary>
    /// <param name="format">The format of individual elements.</param>
    /// <returns>The string representation.</returns>
    public string ToString(string format) => ToString(format, CultureInfo.CurrentCulture);


    /// <summary>
    /// Returns a String representing this Vector4 instance, using the specified format to format individual elements 
    /// and the given IFormatProvider.
    /// </summary>
    /// <param name="format">The format of individual elements.</param>
    /// <param name="formatProvider">The format provider to use when formatting elements.</param>
    /// <returns>The string representation.</returns>
    public string ToString(string format, IFormatProvider formatProvider)
    {
        StringBuilder sb = new StringBuilder();
        string separator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;
        sb.Append('<');
        sb.Append(X.ToString(format, formatProvider));
        sb.Append(separator);
        sb.Append(' ');
        sb.Append(Y.ToString(format, formatProvider));
        sb.Append(separator);
        sb.Append(' ');
        sb.Append(Z.ToString(format, formatProvider));
        sb.Append(separator);
        sb.Append(' ');
        sb.Append(W.ToString(format, formatProvider));
        sb.Append('>');
        return sb.ToString();
    }


    public bool IsFinate() => Mathf.IsValid(X) && Mathf.IsValid(Y) && Mathf.IsValid(Z) && Mathf.IsValid(W);

    #endregion Public Instance Methods


    #region Public Static Methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 MoveTowards(Vector4 current, Vector4 target, float maxDistanceDelta)
    {
        Vector4 toVector = target - current;
        double dist = toVector.Magnitude;
        if (dist <= maxDistanceDelta || dist == 0)
            return target;
        return current + toVector / dist * maxDistanceDelta;
    }


    /// <summary>
    /// Returns the Euclidean distance between the two given points.
    /// </summary>
    /// <param name="value1">The first point.</param>
    /// <param name="value2">The second point.</param>
    /// <returns>The distance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Distance(Vector4 value1, Vector4 value2)
    {
        double dx = value1.X - value2.X;
        double dy = value1.Y - value2.Y;
        double dz = value1.Z - value2.Z;
        double dw = value1.W - value2.W;

        double ls = dx * dx + dy * dy + dz * dz + dw * dw;

        return (double)Math.Sqrt((double)ls);
    }


    /// <summary>
    /// Returns the Euclidean distance squared between the two given points.
    /// </summary>
    /// <param name="value1">The first point.</param>
    /// <param name="value2">The second point.</param>
    /// <returns>The distance squared.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double DistanceSquared(Vector4 value1, Vector4 value2)
    {
        double dx = value1.X - value2.X;
        double dy = value1.Y - value2.Y;
        double dz = value1.Z - value2.Z;
        double dw = value1.W - value2.W;

        return dx * dx + dy * dy + dz * dz + dw * dw;
    }


    /// <summary>
    /// Returns a vector with the same direction as the given vector, but with a length of 1.
    /// </summary>
    /// <param name="vector">The vector to normalize.</param>
    /// <returns>The normalized vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Normalize(Vector4 vector)
    {
        double ls = vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z + vector.W * vector.W;
        double invNorm = 1.0 / (double)Math.Sqrt((double)ls);

        return new Vector4(
            vector.X * invNorm,
            vector.Y * invNorm,
            vector.Z * invNorm,
            vector.W * invNorm);
    }


    /// <summary>
    /// Restricts a vector between a min and max value.
    /// </summary>
    /// <param name="value1">The source vector.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <returns>The restricted vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Clamp(Vector4 value1, Vector4 min, Vector4 max)
    {
        // This compare order is very important!!!
        // We must follow HLSL behavior in the case user specified min value is bigger than max value.

        double x = value1.X;
        x = x > max.X ? max.X : x;
        x = x < min.X ? min.X : x;

        double y = value1.Y;
        y = y > max.Y ? max.Y : y;
        y = y < min.Y ? min.Y : y;

        double z = value1.Z;
        z = z > max.Z ? max.Z : z;
        z = z < min.Z ? min.Z : z;

        double w = value1.W;
        w = w > max.W ? max.W : w;
        w = w < min.W ? min.W : w;

        return new Vector4(x, y, z, w);
    }


    /// <summary>
    /// Linearly interpolates between two vectors based on the given weighting.
    /// </summary>
    /// <param name="value1">The first source vector.</param>
    /// <param name="value2">The second source vector.</param>
    /// <param name="amount">Value between 0 and 1 indicating the weight of the second source vector.</param>
    /// <returns>The interpolated vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Lerp(Vector4 value1, Vector4 value2, double amount) =>
        new(
            value1.X + (value2.X - value1.X) * amount,
            value1.Y + (value2.Y - value1.Y) * amount,
            value1.Z + (value2.Z - value1.Z) * amount,
            value1.W + (value2.W - value1.W) * amount);


    /// <summary>
    /// Transforms a vector by the given matrix.
    /// </summary>
    /// <param name="position">The source vector.</param>
    /// <param name="matrix">The transformation matrix.</param>
    /// <returns>The transformed vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Transform(Vector2 position, Matrix4x4 matrix) =>
        new(
            position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M41,
            position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M42,
            position.X * matrix.M13 + position.Y * matrix.M23 + matrix.M43,
            position.X * matrix.M14 + position.Y * matrix.M24 + matrix.M44);


    /// <summary>
    /// Transforms a vector by the given matrix.
    /// </summary>
    /// <param name="position">The source vector.</param>
    /// <param name="matrix">The transformation matrix.</param>
    /// <returns>The transformed vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Transform(Vector3 position, Matrix4x4 matrix) =>
        new(
            position.X * matrix.M11 + position.Y * matrix.M21 + position.Z * matrix.M31 + matrix.M41,
            position.X * matrix.M12 + position.Y * matrix.M22 + position.Z * matrix.M32 + matrix.M42,
            position.X * matrix.M13 + position.Y * matrix.M23 + position.Z * matrix.M33 + matrix.M43,
            position.X * matrix.M14 + position.Y * matrix.M24 + position.Z * matrix.M34 + matrix.M44);


    /// <summary>
    /// Transforms a vector by the given matrix.
    /// </summary>
    /// <param name="vector">The source vector.</param>
    /// <param name="matrix">The transformation matrix.</param>
    /// <returns>The transformed vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Transform(Vector4 vector, Matrix4x4 matrix) =>
        new(
            vector.X * matrix.M11 + vector.Y * matrix.M21 + vector.Z * matrix.M31 + vector.W * matrix.M41,
            vector.X * matrix.M12 + vector.Y * matrix.M22 + vector.Z * matrix.M32 + vector.W * matrix.M42,
            vector.X * matrix.M13 + vector.Y * matrix.M23 + vector.Z * matrix.M33 + vector.W * matrix.M43,
            vector.X * matrix.M14 + vector.Y * matrix.M24 + vector.Z * matrix.M34 + vector.W * matrix.M44);


    /// <summary>
    /// Transforms a vector by the given Quaternion rotation value.
    /// </summary>
    /// <param name="value">The source vector to be rotated.</param>
    /// <param name="rotation">The rotation to apply.</param>
    /// <returns>The transformed vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Transform(Vector2 value, Quaternion rotation)
    {
        double x2 = rotation.X + rotation.X;
        double y2 = rotation.Y + rotation.Y;
        double z2 = rotation.Z + rotation.Z;

        double wx2 = rotation.W * x2;
        double wy2 = rotation.W * y2;
        double wz2 = rotation.W * z2;
        double xx2 = rotation.X * x2;
        double xy2 = rotation.X * y2;
        double xz2 = rotation.X * z2;
        double yy2 = rotation.Y * y2;
        double yz2 = rotation.Y * z2;
        double zz2 = rotation.Z * z2;

        return new Vector4(
            value.X * (1.0 - yy2 - zz2) + value.Y * (xy2 - wz2),
            value.X * (xy2 + wz2) + value.Y * (1.0 - xx2 - zz2),
            value.X * (xz2 - wy2) + value.Y * (yz2 + wx2),
            1.0);
    }


    /// <summary>
    /// Transforms a vector by the given Quaternion rotation value.
    /// </summary>
    /// <param name="value">The source vector to be rotated.</param>
    /// <param name="rotation">The rotation to apply.</param>
    /// <returns>The transformed vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Transform(Vector3 value, Quaternion rotation)
    {
        double x2 = rotation.X + rotation.X;
        double y2 = rotation.Y + rotation.Y;
        double z2 = rotation.Z + rotation.Z;

        double wx2 = rotation.W * x2;
        double wy2 = rotation.W * y2;
        double wz2 = rotation.W * z2;
        double xx2 = rotation.X * x2;
        double xy2 = rotation.X * y2;
        double xz2 = rotation.X * z2;
        double yy2 = rotation.Y * y2;
        double yz2 = rotation.Y * z2;
        double zz2 = rotation.Z * z2;

        return new Vector4(
            value.X * (1.0 - yy2 - zz2) + value.Y * (xy2 - wz2) + value.Z * (xz2 + wy2),
            value.X * (xy2 + wz2) + value.Y * (1.0 - xx2 - zz2) + value.Z * (yz2 - wx2),
            value.X * (xz2 - wy2) + value.Y * (yz2 + wx2) + value.Z * (1.0 - xx2 - yy2),
            1.0);
    }


    /// <summary>
    /// Transforms a vector by the given Quaternion rotation value.
    /// </summary>
    /// <param name="value">The source vector to be rotated.</param>
    /// <param name="rotation">The rotation to apply.</param>
    /// <returns>The transformed vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Transform(Vector4 value, Quaternion rotation)
    {
        double x2 = rotation.X + rotation.X;
        double y2 = rotation.Y + rotation.Y;
        double z2 = rotation.Z + rotation.Z;

        double wx2 = rotation.W * x2;
        double wy2 = rotation.W * y2;
        double wz2 = rotation.W * z2;
        double xx2 = rotation.X * x2;
        double xy2 = rotation.X * y2;
        double xz2 = rotation.X * z2;
        double yy2 = rotation.Y * y2;
        double yz2 = rotation.Y * z2;
        double zz2 = rotation.Z * z2;

        return new Vector4(
            value.X * (1.0 - yy2 - zz2) + value.Y * (xy2 - wz2) + value.Z * (xz2 + wy2),
            value.X * (xy2 + wz2) + value.Y * (1.0 - xx2 - zz2) + value.Z * (yz2 - wx2),
            value.X * (xz2 - wy2) + value.Y * (yz2 + wx2) + value.Z * (1.0 - xx2 - yy2),
            value.W);
    }


    /// <summary>
    /// Returns the dot product of two vectors.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The dot product.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Dot(Vector4 vector1, Vector4 vector2) =>
        vector1.X * vector2.X +
        vector1.Y * vector2.Y +
        vector1.Z * vector2.Z +
        vector1.W * vector2.W;


    /// <summary>
    /// Returns a vector whose elements are the minimum of each of the pairs of elements in the two source vectors.
    /// </summary>
    /// <param name="value1">The first source vector.</param>
    /// <param name="value2">The second source vector.</param>
    /// <returns>The minimized vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Min(Vector4 value1, Vector4 value2) =>
        new(
            value1.X < value2.X ? value1.X : value2.X,
            value1.Y < value2.Y ? value1.Y : value2.Y,
            value1.Z < value2.Z ? value1.Z : value2.Z,
            value1.W < value2.W ? value1.W : value2.W);


    /// <summary>
    /// Returns a vector whose elements are the maximum of each of the pairs of elements in the two source vectors.
    /// </summary>
    /// <param name="value1">The first source vector.</param>
    /// <param name="value2">The second source vector.</param>
    /// <returns>The maximized vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Max(Vector4 value1, Vector4 value2) =>
        new(
            value1.X > value2.X ? value1.X : value2.X,
            value1.Y > value2.Y ? value1.Y : value2.Y,
            value1.Z > value2.Z ? value1.Z : value2.Z,
            value1.W > value2.W ? value1.W : value2.W);


    /// <summary>
    /// Returns a vector whose elements are the absolute values of each of the source vector's elements.
    /// </summary>
    /// <param name="value">The source vector.</param>
    /// <returns>The absolute value vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Abs(Vector4 value) => new(Math.Abs(value.X), Math.Abs(value.Y), Math.Abs(value.Z), Math.Abs(value.W));


    /// <summary>
    /// Returns a vector whose elements are the square root of each of the source vector's elements.
    /// </summary>
    /// <param name="value">The source vector.</param>
    /// <returns>The square root vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 SquareRoot(Vector4 value) => new(
        (double)Math.Sqrt(value.X), (double)Math.Sqrt(value.Y), (double)Math.Sqrt(value.Z), (double)Math.Sqrt(value.W));

    #endregion Public Static Methods


    #region Public static operators

    /// <summary>
    /// Adds two vectors together.
    /// </summary>
    /// <param name="left">The first source vector.</param>
    /// <param name="right">The second source vector.</param>
    /// <returns>The summed vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 operator +(Vector4 left, Vector4 right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);


    /// <summary>
    /// Subtracts the second vector from the first.
    /// </summary>
    /// <param name="left">The first source vector.</param>
    /// <param name="right">The second source vector.</param>
    /// <returns>The difference vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 operator -(Vector4 left, Vector4 right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);


    /// <summary>
    /// Multiplies two vectors together.
    /// </summary>
    /// <param name="left">The first source vector.</param>
    /// <param name="right">The second source vector.</param>
    /// <returns>The product vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 operator *(Vector4 left, Vector4 right) => new(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);


    /// <summary>
    /// Multiplies a vector by the given scalar.
    /// </summary>
    /// <param name="left">The source vector.</param>
    /// <param name="right">The scalar value.</param>
    /// <returns>The scaled vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 operator *(Vector4 left, double right) => left * new Vector4(right);


    /// <summary>
    /// Multiplies a vector by the given scalar.
    /// </summary>
    /// <param name="left">The scalar value.</param>
    /// <param name="right">The source vector.</param>
    /// <returns>The scaled vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 operator *(double left, Vector4 right) => new Vector4(left) * right;


    /// <summary>
    /// Divides the first vector by the second.
    /// </summary>
    /// <param name="left">The first source vector.</param>
    /// <param name="right">The second source vector.</param>
    /// <returns>The vector resulting from the division.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 operator /(Vector4 left, Vector4 right) => new(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);


    /// <summary>
    /// Divides the vector by the given scalar.
    /// </summary>
    /// <param name="value1">The source vector.</param>
    /// <param name="value2">The scalar value.</param>
    /// <returns>The result of the division.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 operator /(Vector4 value1, double value2)
    {
        double invDiv = 1.0 / value2;

        return new Vector4(
            value1.X * invDiv,
            value1.Y * invDiv,
            value1.Z * invDiv,
            value1.W * invDiv);
    }


    /// <summary>
    /// Negates a given vector.
    /// </summary>
    /// <param name="value">The source vector.</param>
    /// <returns>The negated vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 operator -(Vector4 value) => zero - value;


    /// <summary>
    /// Returns a boolean indicating whether the two given vectors are equal.
    /// </summary>
    /// <param name="left">The first vector to compare.</param>
    /// <param name="right">The second vector to compare.</param>
    /// <returns>True if the vectors are equal; False otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Vector4 left, Vector4 right) => left.Equals(right);


    /// <summary>
    /// Returns a boolean indicating whether the two given vectors are not equal.
    /// </summary>
    /// <param name="left">The first vector to compare.</param>
    /// <param name="right">The second vector to compare.</param>
    /// <returns>True if the vectors are not equal; False if they are equal.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Vector4 left, Vector4 right) => !(left == right);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator System.Numerics.Vector4(Vector4 value) => new((float)value.X, (float)value.Y, (float)value.Z, (float)value.W);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector4(System.Numerics.Vector4 value) => new(value.X, value.Y, value.Z, value.W);

    #endregion Public static operators
}