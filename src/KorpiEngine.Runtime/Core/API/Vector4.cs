﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace KorpiEngine.Core.API;

/// <summary>
/// A structure encapsulating four single precision floating point values and provides hardware accelerated methods.
/// </summary>
public struct Vector4 : IEquatable<Vector4>, IFormattable
{
    public double x;

    public double y;

    public double z;

    public double w;


    #region Constructors

    /// <summary> Constructs a vector whose elements are all the single specified value. </summary>
    public Vector4(double value) : this(value, value, value, value)
    {
    }


    /// <summary> Constructs a vector with the given individual elements. </summary>
    public Vector4(double x, double y, double z, double w)
    {
        this.w = w;
        this.x = x;
        this.y = y;
        this.z = z;
    }


    /// <summary> Constructs a Vector4 from the given Vector2 and a Z and W component. </summary>
    public Vector4(Vector2 value, double z = 0.0, double w = 0.0)
    {
        x = value.x;
        y = value.y;
        this.z = z;
        this.w = w;
    }


    /// <summary> Constructs a Vector4 from the given Vector3 and a W component. </summary>
    public Vector4(Vector3 value, double w = 0.0)
    {
        x = value.x;
        y = value.y;
        z = value.z;
        this.w = w;
    }

    #endregion Constructors


    #region Public Instance Properties

    public Vector4 normalized => Normalize(this);

    public double magnitude => Mathf.Sqrt(x * x + y * y + z * z + w * w);

    public double sqrMagnitude => x * x + y * y + z * z + w * w;

    public Vector3 xyz => new(x, y, z);

    public double this[int index]
    {
        get
        {
            switch (index)
            {
                case 0:
                    return x;
                case 1:
                    return y;
                case 2:
                    return z;
                case 3:
                    return w;
                default:
                    throw new IndexOutOfRangeException("Invalid Vector4 index!");
            }
        }

        set
        {
            switch (index)
            {
                case 0:
                    x = value;
                    break;
                case 1:
                    y = value;
                    break;
                case 2:
                    z = value;
                    break;
                case 3:
                    w = value;
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

    public static Vector4 infinity = new(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);

    #endregion Public Static Properties


    #region Public Instance methods

    public System.Numerics.Vector4 ToFloat() => new((float)x, (float)y, (float)z, (float)w);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Scale(Vector4 scale)
    {
        x *= scale.x;
        y *= scale.y;
        z *= scale.z;
        w *= scale.w;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Normalize()
    {
        double ls = x * x + y * y + z * z + w * w;
        double invNorm = 1.0 / (double)Math.Sqrt((double)ls);
        x *= invNorm;
        y *= invNorm;
        z *= invNorm;
        w *= invNorm;
    }


    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode()
    {
        int hash = x.GetHashCode();
        hash = HashCode.Combine(hash, y.GetHashCode());
        hash = HashCode.Combine(hash, z.GetHashCode());
        hash = HashCode.Combine(hash, w.GetHashCode());
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
        x == other.x
        && y == other.y
        && z == other.z
        && w == other.w;


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
        sb.Append(x.ToString(format, formatProvider));
        sb.Append(separator);
        sb.Append(' ');
        sb.Append(y.ToString(format, formatProvider));
        sb.Append(separator);
        sb.Append(' ');
        sb.Append(z.ToString(format, formatProvider));
        sb.Append(separator);
        sb.Append(' ');
        sb.Append(w.ToString(format, formatProvider));
        sb.Append('>');
        return sb.ToString();
    }


    public bool IsFinate() => Mathf.IsValid(x) && Mathf.IsValid(y) && Mathf.IsValid(z) && Mathf.IsValid(w);

    #endregion Public Instance Methods


    #region Public Static Methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 MoveTowards(Vector4 current, Vector4 target, float maxDistanceDelta)
    {
        Vector4 toVector = target - current;
        double dist = toVector.magnitude;
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
        double dx = value1.x - value2.x;
        double dy = value1.y - value2.y;
        double dz = value1.z - value2.z;
        double dw = value1.w - value2.w;

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
        double dx = value1.x - value2.x;
        double dy = value1.y - value2.y;
        double dz = value1.z - value2.z;
        double dw = value1.w - value2.w;

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
        double ls = vector.x * vector.x + vector.y * vector.y + vector.z * vector.z + vector.w * vector.w;
        double invNorm = 1.0 / (double)Math.Sqrt((double)ls);

        return new Vector4(
            vector.x * invNorm,
            vector.y * invNorm,
            vector.z * invNorm,
            vector.w * invNorm);
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

        double x = value1.x;
        x = x > max.x ? max.x : x;
        x = x < min.x ? min.x : x;

        double y = value1.y;
        y = y > max.y ? max.y : y;
        y = y < min.y ? min.y : y;

        double z = value1.z;
        z = z > max.z ? max.z : z;
        z = z < min.z ? min.z : z;

        double w = value1.w;
        w = w > max.w ? max.w : w;
        w = w < min.w ? min.w : w;

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
            value1.x + (value2.x - value1.x) * amount,
            value1.y + (value2.y - value1.y) * amount,
            value1.z + (value2.z - value1.z) * amount,
            value1.w + (value2.w - value1.w) * amount);


    /// <summary>
    /// Transforms a vector by the given matrix.
    /// </summary>
    /// <param name="position">The source vector.</param>
    /// <param name="matrix">The transformation matrix.</param>
    /// <returns>The transformed vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Transform(Vector2 position, Matrix4x4 matrix) =>
        new(
            position.x * matrix.M11 + position.y * matrix.M21 + matrix.M41,
            position.x * matrix.M12 + position.y * matrix.M22 + matrix.M42,
            position.x * matrix.M13 + position.y * matrix.M23 + matrix.M43,
            position.x * matrix.M14 + position.y * matrix.M24 + matrix.M44);


    /// <summary>
    /// Transforms a vector by the given matrix.
    /// </summary>
    /// <param name="position">The source vector.</param>
    /// <param name="matrix">The transformation matrix.</param>
    /// <returns>The transformed vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Transform(Vector3 position, Matrix4x4 matrix) =>
        new(
            position.x * matrix.M11 + position.y * matrix.M21 + position.z * matrix.M31 + matrix.M41,
            position.x * matrix.M12 + position.y * matrix.M22 + position.z * matrix.M32 + matrix.M42,
            position.x * matrix.M13 + position.y * matrix.M23 + position.z * matrix.M33 + matrix.M43,
            position.x * matrix.M14 + position.y * matrix.M24 + position.z * matrix.M34 + matrix.M44);


    /// <summary>
    /// Transforms a vector by the given matrix.
    /// </summary>
    /// <param name="vector">The source vector.</param>
    /// <param name="matrix">The transformation matrix.</param>
    /// <returns>The transformed vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Transform(Vector4 vector, Matrix4x4 matrix) =>
        new(
            vector.x * matrix.M11 + vector.y * matrix.M21 + vector.z * matrix.M31 + vector.w * matrix.M41,
            vector.x * matrix.M12 + vector.y * matrix.M22 + vector.z * matrix.M32 + vector.w * matrix.M42,
            vector.x * matrix.M13 + vector.y * matrix.M23 + vector.z * matrix.M33 + vector.w * matrix.M43,
            vector.x * matrix.M14 + vector.y * matrix.M24 + vector.z * matrix.M34 + vector.w * matrix.M44);


    /// <summary>
    /// Transforms a vector by the given Quaternion rotation value.
    /// </summary>
    /// <param name="value">The source vector to be rotated.</param>
    /// <param name="rotation">The rotation to apply.</param>
    /// <returns>The transformed vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Transform(Vector2 value, Quaternion rotation)
    {
        double x2 = rotation.x + rotation.x;
        double y2 = rotation.y + rotation.y;
        double z2 = rotation.z + rotation.z;

        double wx2 = rotation.w * x2;
        double wy2 = rotation.w * y2;
        double wz2 = rotation.w * z2;
        double xx2 = rotation.x * x2;
        double xy2 = rotation.x * y2;
        double xz2 = rotation.x * z2;
        double yy2 = rotation.y * y2;
        double yz2 = rotation.y * z2;
        double zz2 = rotation.z * z2;

        return new Vector4(
            value.x * (1.0 - yy2 - zz2) + value.y * (xy2 - wz2),
            value.x * (xy2 + wz2) + value.y * (1.0 - xx2 - zz2),
            value.x * (xz2 - wy2) + value.y * (yz2 + wx2),
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
        double x2 = rotation.x + rotation.x;
        double y2 = rotation.y + rotation.y;
        double z2 = rotation.z + rotation.z;

        double wx2 = rotation.w * x2;
        double wy2 = rotation.w * y2;
        double wz2 = rotation.w * z2;
        double xx2 = rotation.x * x2;
        double xy2 = rotation.x * y2;
        double xz2 = rotation.x * z2;
        double yy2 = rotation.y * y2;
        double yz2 = rotation.y * z2;
        double zz2 = rotation.z * z2;

        return new Vector4(
            value.x * (1.0 - yy2 - zz2) + value.y * (xy2 - wz2) + value.z * (xz2 + wy2),
            value.x * (xy2 + wz2) + value.y * (1.0 - xx2 - zz2) + value.z * (yz2 - wx2),
            value.x * (xz2 - wy2) + value.y * (yz2 + wx2) + value.z * (1.0 - xx2 - yy2),
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
        double x2 = rotation.x + rotation.x;
        double y2 = rotation.y + rotation.y;
        double z2 = rotation.z + rotation.z;

        double wx2 = rotation.w * x2;
        double wy2 = rotation.w * y2;
        double wz2 = rotation.w * z2;
        double xx2 = rotation.x * x2;
        double xy2 = rotation.x * y2;
        double xz2 = rotation.x * z2;
        double yy2 = rotation.y * y2;
        double yz2 = rotation.y * z2;
        double zz2 = rotation.z * z2;

        return new Vector4(
            value.x * (1.0 - yy2 - zz2) + value.y * (xy2 - wz2) + value.z * (xz2 + wy2),
            value.x * (xy2 + wz2) + value.y * (1.0 - xx2 - zz2) + value.z * (yz2 - wx2),
            value.x * (xz2 - wy2) + value.y * (yz2 + wx2) + value.z * (1.0 - xx2 - yy2),
            value.w);
    }


    /// <summary>
    /// Returns the dot product of two vectors.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The dot product.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Dot(Vector4 vector1, Vector4 vector2) =>
        vector1.x * vector2.x +
        vector1.y * vector2.y +
        vector1.z * vector2.z +
        vector1.w * vector2.w;


    /// <summary>
    /// Returns a vector whose elements are the minimum of each of the pairs of elements in the two source vectors.
    /// </summary>
    /// <param name="value1">The first source vector.</param>
    /// <param name="value2">The second source vector.</param>
    /// <returns>The minimized vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Min(Vector4 value1, Vector4 value2) =>
        new(
            value1.x < value2.x ? value1.x : value2.x,
            value1.y < value2.y ? value1.y : value2.y,
            value1.z < value2.z ? value1.z : value2.z,
            value1.w < value2.w ? value1.w : value2.w);


    /// <summary>
    /// Returns a vector whose elements are the maximum of each of the pairs of elements in the two source vectors.
    /// </summary>
    /// <param name="value1">The first source vector.</param>
    /// <param name="value2">The second source vector.</param>
    /// <returns>The maximized vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Max(Vector4 value1, Vector4 value2) =>
        new(
            value1.x > value2.x ? value1.x : value2.x,
            value1.y > value2.y ? value1.y : value2.y,
            value1.z > value2.z ? value1.z : value2.z,
            value1.w > value2.w ? value1.w : value2.w);


    /// <summary>
    /// Returns a vector whose elements are the absolute values of each of the source vector's elements.
    /// </summary>
    /// <param name="value">The source vector.</param>
    /// <returns>The absolute value vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Abs(Vector4 value) => new(Math.Abs(value.x), Math.Abs(value.y), Math.Abs(value.z), Math.Abs(value.w));


    /// <summary>
    /// Returns a vector whose elements are the square root of each of the source vector's elements.
    /// </summary>
    /// <param name="value">The source vector.</param>
    /// <returns>The square root vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 SquareRoot(Vector4 value) => new(
        (double)Math.Sqrt(value.x), (double)Math.Sqrt(value.y), (double)Math.Sqrt(value.z), (double)Math.Sqrt(value.w));

    #endregion Public Static Methods


    #region Public static operators

    /// <summary>
    /// Adds two vectors together.
    /// </summary>
    /// <param name="left">The first source vector.</param>
    /// <param name="right">The second source vector.</param>
    /// <returns>The summed vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 operator +(Vector4 left, Vector4 right) => new(left.x + right.x, left.y + right.y, left.z + right.z, left.w + right.w);


    /// <summary>
    /// Subtracts the second vector from the first.
    /// </summary>
    /// <param name="left">The first source vector.</param>
    /// <param name="right">The second source vector.</param>
    /// <returns>The difference vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 operator -(Vector4 left, Vector4 right) => new(left.x - right.x, left.y - right.y, left.z - right.z, left.w - right.w);


    /// <summary>
    /// Multiplies two vectors together.
    /// </summary>
    /// <param name="left">The first source vector.</param>
    /// <param name="right">The second source vector.</param>
    /// <returns>The product vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 operator *(Vector4 left, Vector4 right) => new(left.x * right.x, left.y * right.y, left.z * right.z, left.w * right.w);


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
    public static Vector4 operator /(Vector4 left, Vector4 right) => new(left.x / right.x, left.y / right.y, left.z / right.z, left.w / right.w);


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
            value1.x * invDiv,
            value1.y * invDiv,
            value1.z * invDiv,
            value1.w * invDiv);
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
    public static implicit operator System.Numerics.Vector4(Vector4 value) => new((float)value.x, (float)value.y, (float)value.z, (float)value.w);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector4(System.Numerics.Vector4 value) => new(value.X, value.Y, value.Z, value.W);

    #endregion Public static operators
}