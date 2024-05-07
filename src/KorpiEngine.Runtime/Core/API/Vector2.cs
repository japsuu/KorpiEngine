﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace KorpiEngine.Core.API;

/// <summary>
/// A structure encapsulating two single precision floating point values and provides hardware accelerated methods.
/// </summary>
public struct Vector2 : IEquatable<Vector2>, IFormattable
{
    public double x;
    public double y;


    #region Constructors

    /// <summary> Constructs a vector whose elements are all the single specified value. </summary>
    public Vector2(double value) : this(value, value)
    {
    }


    /// <summary> Constructs a vector with the given individual elements. </summary>
    public Vector2(double x, double y)
    {
        this.x = x;
        this.y = y;
    }

    #endregion Constructors


    #region Public Instance Properties

    public Vector2 normalized => Normalize(this);

    public double magnitude => Mathf.Sqrt(x * x + y * y);

    public double sqrMagnitude => x * x + y * y;

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
                default:
                    throw new IndexOutOfRangeException("Invalid Vector2 index!");
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
                default:
                    throw new IndexOutOfRangeException("Invalid Vector2 index!");
            }
        }
    }

    #endregion


    #region Public Instance methods

    public System.Numerics.Vector2 ToFloat() => new((float)x, (float)y);


    public void Scale(Vector2 scale)
    {
        x *= scale.x;
        y *= scale.y;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Normalize()
    {
        double ls = x * x + y * y;
        double invNorm = 1.0 / (double)Math.Sqrt((double)ls);
        x *= invNorm;
        y *= invNorm;
    }


    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode()
    {
        int hash = x.GetHashCode();
        hash = HashCode.Combine(hash, y.GetHashCode());
        return hash;
    }


    /// <summary>
    /// Returns a boolean indicating whether the given Object is equal to this Vector2 instance.
    /// </summary>
    /// <param name="obj">The Object to compare against.</param>
    /// <returns>True if the Object is equal to this Vector2; False otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj)
    {
        if (obj is not Vector2)
            return false;
        return Equals((Vector2)obj);
    }


    /// <summary>
    /// Returns a boolean indicating whether the given Vector2 is equal to this Vector2 instance.
    /// </summary>
    /// <param name="other">The Vector2 to compare this instance to.</param>
    /// <returns>True if the other Vector2 is equal to this instance; False otherwise.</returns>
    public bool Equals(Vector2 other) => x == other.x && y == other.y;


    /// <summary>
    /// Returns a String representing this Vector2 instance.
    /// </summary>
    /// <returns>The string representation.</returns>
    public override string ToString() => ToString("G", CultureInfo.CurrentCulture);


    /// <summary>
    /// Returns a String representing this Vector2 instance, using the specified format to format individual elements.
    /// </summary>
    /// <param name="format">The format of individual elements.</param>
    /// <returns>The string representation.</returns>
    public string ToString(string format) => ToString(format, CultureInfo.CurrentCulture);


    /// <summary>
    /// Returns a String representing this Vector2 instance, using the specified format to format individual elements 
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
        sb.Append('>');
        return sb.ToString();
    }


    public bool IsFinate() => Mathf.IsValid(x) && Mathf.IsValid(y);

    #endregion Public Instance Methods


    #region Public Static Properties

    public static Vector2 zero => new();
    public static Vector2 one => new(1.0, 1.0);
    public static Vector2 right => new(1.0, 0.0);
    public static Vector2 left => new(1.0, 0.0);
    public static Vector2 up => new(0.0, 1.0);
    public static Vector2 down => new(0.0, 1.0);

    public static Vector2 infinity = new(Mathf.Infinity, Mathf.Infinity);

    #endregion Public Static Properties


    #region Public Static Methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double AngleBetween(Vector2 from, Vector2 to) => Mathf.Acos(Mathf.Clamp(Dot(from.normalized, to.normalized), -1, 1)) * Mathf.Rad2Deg;


    /// <summary>
    /// Returns the Euclidean distance between the two given points.
    /// </summary>
    /// <param name="value1">The first point.</param>
    /// <param name="value2">The second point.</param>
    /// <returns>The distance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Distance(Vector2 value1, Vector2 value2)
    {
        double dx = value1.x - value2.x;
        double dy = value1.y - value2.y;

        double ls = dx * dx + dy * dy;

        return (double)Math.Sqrt((double)ls);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 MoveTowards(Vector2 current, Vector2 target, float maxDistanceDelta)
    {
        Vector2 toVector = target - current;
        double dist = toVector.magnitude;
        if (dist <= maxDistanceDelta || dist == 0)
            return target;
        return current + toVector / dist * maxDistanceDelta;
    }


    /// <summary>
    /// Returns a vector with the same direction as the given vector, but with a length of 1.
    /// </summary>
    /// <param name="value">The vector to normalize.</param>
    /// <returns>The normalized vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Normalize(Vector2 value)
    {
        double ls = value.x * value.x + value.y * value.y;
        double invNorm = 1.0 / (double)Math.Sqrt((double)ls);

        return new Vector2(
            value.x * invNorm,
            value.y * invNorm);
    }


    /// <summary>
    /// Returns the reflection of a vector off a surface that has the specified normal.
    /// </summary>
    /// <param name="vector">The source vector.</param>
    /// <param name="normal">The normal of the surface being reflected off.</param>
    /// <returns>The reflected vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Reflect(Vector2 vector, Vector2 normal)
    {
        double dot = vector.x * normal.x + vector.y * normal.y;

        return new Vector2(
            vector.x - 2.0 * dot * normal.x,
            vector.y - 2.0 * dot * normal.y);
    }


    /// <summary>
    /// Restricts a vector between a min and max value.
    /// </summary>
    /// <param name="value1">The source vector.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Clamp(Vector2 value1, Vector2 min, Vector2 max)
    {
        // This compare order is very important!!!
        // We must follow HLSL behavior in the case user specified min value is bigger than max value.
        double x = value1.x;
        x = x > max.x ? max.x : x;
        x = x < min.x ? min.x : x;

        double y = value1.y;
        y = y > max.y ? max.y : y;
        y = y < min.y ? min.y : y;

        return new Vector2(x, y);
    }


    /// <summary>
    /// Linearly interpolates between two vectors based on the given weighting.
    /// </summary>
    /// <param name="value1">The first source vector.</param>
    /// <param name="value2">The second source vector.</param>
    /// <param name="amount">Value between 0 and 1 indicating the weight of the second source vector.</param>
    /// <returns>The interpolated vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Lerp(Vector2 value1, Vector2 value2, double amount) =>
        new(
            value1.x + (value2.x - value1.x) * amount,
            value1.y + (value2.y - value1.y) * amount);


    /// <summary>
    /// Transforms a vector by the given matrix.
    /// </summary>
    /// <param name="position">The source vector.</param>
    /// <param name="matrix">The transformation matrix.</param>
    /// <returns>The transformed vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Transform(Vector2 position, Matrix4x4 matrix) =>
        new(
            position.x * matrix.M11 + position.y * matrix.M21 + matrix.M41,
            position.x * matrix.M12 + position.y * matrix.M22 + matrix.M42);


    /// <summary>
    /// Transforms a vector normal by the given matrix.
    /// </summary>
    /// <param name="normal">The source vector.</param>
    /// <param name="matrix">The transformation matrix.</param>
    /// <returns>The transformed vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 TransformNormal(Vector2 normal, Matrix4x4 matrix) =>
        new(
            normal.x * matrix.M11 + normal.y * matrix.M21,
            normal.x * matrix.M12 + normal.y * matrix.M22);


    /// <summary>
    /// Transforms a vector by the given Quaternion rotation value.
    /// </summary>
    /// <param name="value">The source vector to be rotated.</param>
    /// <param name="rotation">The rotation to apply.</param>
    /// <returns>The transformed vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Transform(Vector2 value, Quaternion rotation)
    {
        double x2 = rotation.x + rotation.x;
        double y2 = rotation.y + rotation.y;
        double z2 = rotation.z + rotation.z;

        double wz2 = rotation.w * z2;
        double xx2 = rotation.x * x2;
        double xy2 = rotation.x * y2;
        double yy2 = rotation.y * y2;
        double zz2 = rotation.z * z2;

        return new Vector2(
            value.x * (1.0 - yy2 - zz2) + value.y * (xy2 - wz2),
            value.x * (xy2 + wz2) + value.y * (1.0 - xx2 - zz2));
    }


    /// <summary>
    /// Returns the dot product of two vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <returns>The dot product.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Dot(Vector2 value1, Vector2 value2) =>
        value1.x * value2.x +
        value1.y * value2.y;


    /// <summary>
    /// Returns a vector whose elements are the minimum of each of the pairs of elements in the two source vectors.
    /// </summary>
    /// <param name="value1">The first source vector.</param>
    /// <param name="value2">The second source vector.</param>
    /// <returns>The minimized vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Min(Vector2 value1, Vector2 value2) =>
        new(
            value1.x < value2.x ? value1.x : value2.x,
            value1.y < value2.y ? value1.y : value2.y);


    /// <summary>
    /// Returns a vector whose elements are the maximum of each of the pairs of elements in the two source vectors
    /// </summary>
    /// <param name="value1">The first source vector</param>
    /// <param name="value2">The second source vector</param>
    /// <returns>The maximized vector</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Max(Vector2 value1, Vector2 value2) =>
        new(
            value1.x > value2.x ? value1.x : value2.x,
            value1.y > value2.y ? value1.y : value2.y);


    /// <summary>
    /// Returns a vector whose elements are the absolute values of each of the source vector's elements.
    /// </summary>
    /// <param name="value">The source vector.</param>
    /// <returns>The absolute value vector.</returns>        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Abs(Vector2 value) => new(Math.Abs(value.x), Math.Abs(value.y));


    /// <summary>
    /// Returns a vector whose elements are the square root of each of the source vector's elements.
    /// </summary>
    /// <param name="value">The source vector.</param>
    /// <returns>The square root vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 SquareRoot(Vector2 value) => new((double)Math.Sqrt(value.x), (double)Math.Sqrt(value.y));

    #endregion Public Static Methods


    #region Public Static Operators

    /// <summary>
    /// Adds two vectors together.
    /// </summary>
    /// <param name="left">The first source vector.</param>
    /// <param name="right">The second source vector.</param>
    /// <returns>The summed vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator +(Vector2 left, Vector2 right) => new(left.x + right.x, left.y + right.y);


    /// <summary>
    /// Subtracts the second vector from the first.
    /// </summary>
    /// <param name="left">The first source vector.</param>
    /// <param name="right">The second source vector.</param>
    /// <returns>The difference vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator -(Vector2 left, Vector2 right) => new(left.x - right.x, left.y - right.y);


    /// <summary>
    /// Multiplies two vectors together.
    /// </summary>
    /// <param name="left">The first source vector.</param>
    /// <param name="right">The second source vector.</param>
    /// <returns>The product vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator *(Vector2 left, Vector2 right) => new(left.x * right.x, left.y * right.y);


    /// <summary>
    /// Multiplies a vector by the given scalar.
    /// </summary>
    /// <param name="left">The scalar value.</param>
    /// <param name="right">The source vector.</param>
    /// <returns>The scaled vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator *(double left, Vector2 right) => new Vector2(left, left) * right;


    /// <summary>
    /// Multiplies a vector by the given scalar.
    /// </summary>
    /// <param name="left">The source vector.</param>
    /// <param name="right">The scalar value.</param>
    /// <returns>The scaled vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator *(Vector2 left, double right) => left * new Vector2(right, right);


    /// <summary>
    /// Divides the first vector by the second.
    /// </summary>
    /// <param name="left">The first source vector.</param>
    /// <param name="right">The second source vector.</param>
    /// <returns>The vector resulting from the division.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator /(Vector2 left, Vector2 right) => new(left.x / right.x, left.y / right.y);


    /// <summary>
    /// Divides the vector by the given scalar.
    /// </summary>
    /// <param name="value1">The source vector.</param>
    /// <param name="value2">The scalar value.</param>
    /// <returns>The result of the division.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator /(Vector2 value1, double value2)
    {
        double invDiv = 1.0 / value2;
        return new Vector2(
            value1.x * invDiv,
            value1.y * invDiv);
    }


    /// <summary>
    /// Negates a given vector.
    /// </summary>
    /// <param name="value">The source vector.</param>
    /// <returns>The negated vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator -(Vector2 value) => zero - value;


    /// <summary>
    /// Returns a boolean indicating whether the two given vectors are equal.
    /// </summary>
    /// <param name="left">The first vector to compare.</param>
    /// <param name="right">The second vector to compare.</param>
    /// <returns>True if the vectors are equal; False otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Vector2 left, Vector2 right) => left.Equals(right);


    /// <summary>
    /// Returns a boolean indicating whether the two given vectors are not equal.
    /// </summary>
    /// <param name="left">The first vector to compare.</param>
    /// <param name="right">The second vector to compare.</param>
    /// <returns>True if the vectors are not equal; False if they are equal.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Vector2 left, Vector2 right) => !(left == right);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator System.Numerics.Vector2(Vector2 value) => new((float)value.x, (float)value.y);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector2(System.Numerics.Vector2 value) => new(value.X, value.Y);

    #endregion Public Static Operators
}