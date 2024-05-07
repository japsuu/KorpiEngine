// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace KorpiEngine.Core.API;

/// <summary>
/// A structure encapsulating three single precision floating point values and provides hardware accelerated methods.
/// </summary>
public struct Vector3 : IEquatable<Vector3>, IFormattable
{
    public double X;
    public double Y;
    public double Z;


    #region Constructors

    /// <summary> Constructs a vector whose elements are all the single specified value. </summary>
    public Vector3(double value) : this(value, value, value)
    {
    }


    /// <summary> Constructs a Vector3 from the given Vector2 and a third value. </summary>
    public Vector3(Vector2 value, double z = 0f) : this(value.X, value.Y, z)
    {
    }


    /// <summary> Constructs a vector with the given individual elements. </summary>
    public Vector3(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    #endregion Constructors


    #region Public Instance Properties

    public Vector3 Normalized => Normalize(this);

    public double Magnitude => Mathf.Sqrt(X * X + Y * Y + Z * Z);

    public double SqrMagnitude => X * X + Y * Y + Z * Z;

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
                default:
                    throw new IndexOutOfRangeException("Invalid Vector3 index!");
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
                default:
                    throw new IndexOutOfRangeException("Invalid Vector3 index!");
            }
        }
    }

    #endregion


    #region Public Instance Methods

    public System.Numerics.Vector3 ToFloat() => new((float)X, (float)Y, (float)Z);


    public void Scale(Vector3 scale)
    {
        X *= scale.X;
        Y *= scale.Y;
        Z *= scale.Z;
    }


    public static Vector3 Scale(Vector3 v, Vector3 scale) => new(v.X * scale.X, v.Y * scale.Y, v.Z * scale.Z);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Normalize()
    {
        double ls = X * X + Y * Y + Z * Z;
        double invNorm = 1.0 / (double)Math.Sqrt((double)ls);
        X *= invNorm;
        Y *= invNorm;
        Z *= invNorm;
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
        return hash;
    }


    /// <summary>
    /// Returns a boolean indicating whether the given Object is equal to this Vector3 instance.
    /// </summary>
    /// <param name="obj">The Object to compare against.</param>
    /// <returns>True if the Object is equal to this Vector3; False otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj)
    {
        if (obj is not Vector3)
            return false;
        return Equals((Vector3)obj);
    }


    /// <summary>
    /// Returns a boolean indicating whether the given Vector3 is equal to this Vector3 instance.
    /// </summary>
    /// <param name="other">The Vector3 to compare this instance to.</param>
    /// <returns>True if the other Vector3 is equal to this instance; False otherwise.</returns>
    public bool Equals(Vector3 other) =>
        X == other.X &&
        Y == other.Y &&
        Z == other.Z;


    /// <summary>
    /// Returns a String representing this Vector3 instance.
    /// </summary>
    /// <returns>The string representation.</returns>
    public override string ToString() => ToString("G", CultureInfo.CurrentCulture);


    /// <summary>
    /// Returns a String representing this Vector3 instance, using the specified format to format individual elements.
    /// </summary>
    /// <param name="format">The format of individual elements.</param>
    /// <returns>The string representation.</returns>
    public string ToString(string format) => ToString(format, CultureInfo.CurrentCulture);


    /// <summary>
    /// Returns a String representing this Vector3 instance, using the specified format to format individual elements 
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
        sb.Append(((IFormattable)X).ToString(format, formatProvider));
        sb.Append(separator);
        sb.Append(' ');
        sb.Append(((IFormattable)Y).ToString(format, formatProvider));
        sb.Append(separator);
        sb.Append(' ');
        sb.Append(((IFormattable)Z).ToString(format, formatProvider));
        sb.Append('>');
        return sb.ToString();
    }


    public bool IsFinate() => Mathf.IsValid(X) && Mathf.IsValid(Y) && Mathf.IsValid(Z);

    #endregion Public Instance Methods


    #region Public Static Properties

    public static Vector3 Zero => new();
    public static Vector3 One => new(1.0, 1.0, 1.0);
    public static Vector3 Right => new(1.0, 0.0, 0.0);
    public static Vector3 Left => new(-1.0, 0.0, 0.0);
    public static Vector3 Up => new(0.0, 1.0, 0.0);
    public static Vector3 Down => new(0.0, -1.0, 0.0);
    public static Vector3 Forward => new(0.0, 0.0, 1.0);
    public static Vector3 Backward => new(0.0, 0.0, -1.0);

    public static Vector3 Infinity = new(Mathf.INFINITY, Mathf.INFINITY, Mathf.INFINITY);

    #endregion Public Static Properties


    #region Public Static Methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double AngleBetween(Vector3 from, Vector3 to) => Mathf.Acos(Mathf.Clamp(Dot(from.Normalized, to.Normalized), -1f, 1f));


    /// <summary> Returns the Euclidean distance between the two given points. </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Distance(Vector3 value1, Vector3 value2)
    {
        double dx = value1.X - value2.X;
        double dy = value1.Y - value2.Y;
        double dz = value1.Z - value2.Z;

        double ls = dx * dx + dy * dy + dz * dz;

        return (double)Math.Sqrt((double)ls);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDistanceDelta)
    {
        Vector3 toVector = target - current;
        double dist = toVector.Magnitude;
        if (dist <= maxDistanceDelta || dist == 0)
            return target;
        return current + toVector / dist * maxDistanceDelta;
    }


    public static Vector3 SmoothDamp(Vector3 current, Vector3 target, ref Vector3 currentVelocity, double smoothTime, double maxSpeed = Mathf.INFINITY,
        double deltaTime = 0.02)
    {
        // Based on Game Programming Gems 4 Chapter 1.10
        smoothTime = Mathf.Max(0.0001F, smoothTime);
        double omega = 2 / smoothTime;

        double x = omega * deltaTime;
        double exp = 1 / (1 + x + 0.48 * x * x + 0.235 * x * x * x);
        Vector3 change = current - target;
        Vector3 originalTo = target;

        // Clamp maximum speed
        double maxChange = maxSpeed * smoothTime;
        change = ClampMagnitude(change, maxChange);
        target = current - change;

        Vector3 temp = (currentVelocity + omega * change) * deltaTime;
        currentVelocity = (currentVelocity - omega * temp) * exp;
        Vector3 output = target + (change + temp) * exp;

        // Prevent overshooting
        if (Dot(originalTo - current, output - originalTo) > 0)
        {
            output = originalTo;
            currentVelocity = (output - originalTo) / deltaTime;
        }

        return output;
    }


    /// <summary> Returns a vector with the same direction as the given vector, but with a length of 1. </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Normalize(Vector3 value)
    {
        double ls = value.X * value.X + value.Y * value.Y + value.Z * value.Z;
        double length = (double)Math.Sqrt(ls);
        return new Vector3(value.X / length, value.Y / length, value.Z / length);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 ClampMagnitude(Vector3 vector, double maxLength)
    {
        if (vector.SqrMagnitude > maxLength * maxLength)
            return vector.Normalized * maxLength;
        return vector;
    }


    /// <summary>
    /// Computes the cross product of two vectors.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The cross product.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Cross(Vector3 vector1, Vector3 vector2) =>
        new(
            vector1.Y * vector2.Z - vector1.Z * vector2.Y,
            vector1.Z * vector2.X - vector1.X * vector2.Z,
            vector1.X * vector2.Y - vector1.Y * vector2.X);


    /// <summary>
    /// Returns the reflection of a vector off a surface that has the specified normal.
    /// </summary>
    /// <param name="vector">The source vector.</param>
    /// <param name="normal">The normal of the surface being reflected off.</param>
    /// <returns>The reflected vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Reflect(Vector3 vector, Vector3 normal)
    {
        double dot = vector.X * normal.X + vector.Y * normal.Y + vector.Z * normal.Z;
        double tempX = normal.X * dot * 2;
        double tempY = normal.Y * dot * 2;
        double tempZ = normal.Z * dot * 2;
        return new Vector3(vector.X - tempX, vector.Y - tempY, vector.Z - tempZ);
    }


    /// <summary>
    /// Restricts a vector between a min and max value.
    /// </summary>
    /// <param name="value1">The source vector.</param>
    /// <param name="min">The minimum value.</param>
    /// <param name="max">The maximum value.</param>
    /// <returns>The restricted vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Clamp(Vector3 value1, Vector3 min, Vector3 max)
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

        return new Vector3(x, y, z);
    }


    /// <summary>
    /// Linearly interpolates between two vectors based on the given weighting.
    /// </summary>
    /// <param name="value1">The first source vector.</param>
    /// <param name="value2">The second source vector.</param>
    /// <param name="amount">Value between 0 and 1 indicating the weight of the second source vector.</param>
    /// <returns>The interpolated vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Lerp(Vector3 value1, Vector3 value2, double amount) =>
        new(
            value1.X + (value2.X - value1.X) * amount,
            value1.Y + (value2.Y - value1.Y) * amount,
            value1.Z + (value2.Z - value1.Z) * amount);


    /// <summary>
    /// Transforms a vector by the given matrix.
    /// </summary>
    /// <param name="position">The source vector.</param>
    /// <param name="matrix">The transformation matrix.</param>
    /// <returns>The transformed vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Transform(Vector3 position, Matrix4x4 matrix) =>
        new(
            position.X * matrix.M11 + position.Y * matrix.M21 + position.Z * matrix.M31 + matrix.M41,
            position.X * matrix.M12 + position.Y * matrix.M22 + position.Z * matrix.M32 + matrix.M42,
            position.X * matrix.M13 + position.Y * matrix.M23 + position.Z * matrix.M33 + matrix.M43);


    /// <summary>
    /// Transforms a vector normal by the given matrix.
    /// </summary>
    /// <param name="normal">The source vector.</param>
    /// <param name="matrix">The transformation matrix.</param>
    /// <returns>The transformed vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 TransformNormal(Vector3 normal, Matrix4x4 matrix) =>
        new(
            normal.X * matrix.M11 + normal.Y * matrix.M21 + normal.Z * matrix.M31,
            normal.X * matrix.M12 + normal.Y * matrix.M22 + normal.Z * matrix.M32,
            normal.X * matrix.M13 + normal.Y * matrix.M23 + normal.Z * matrix.M33);


    /// <summary>
    /// Transforms a vector by the given Quaternion rotation value.
    /// </summary>
    /// <param name="value">The source vector to be rotated.</param>
    /// <param name="rotation">The rotation to apply.</param>
    /// <returns>The transformed vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Transform(Vector3 value, Quaternion rotation)
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

        return new Vector3(
            value.X * (1.0 - yy2 - zz2) + value.Y * (xy2 - wz2) + value.Z * (xz2 + wy2),
            value.X * (xy2 + wz2) + value.Y * (1.0 - xx2 - zz2) + value.Z * (yz2 - wx2),
            value.X * (xz2 - wy2) + value.Y * (yz2 + wx2) + value.Z * (1.0 - xx2 - yy2));
    }


    /// <summary>
    /// Returns the dot product of two vectors.
    /// </summary>
    /// <param name="vector1">The first vector.</param>
    /// <param name="vector2">The second vector.</param>
    /// <returns>The dot product.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Dot(Vector3 vector1, Vector3 vector2) =>
        vector1.X * vector2.X +
        vector1.Y * vector2.Y +
        vector1.Z * vector2.Z;


    /// <summary>
    /// Returns a vector whose elements are the minimum of each of the pairs of elements in the two source vectors.
    /// </summary>
    /// <param name="value1">The first source vector.</param>
    /// <param name="value2">The second source vector.</param>
    /// <returns>The minimized vector.</returns>
    public static Vector3 Min(Vector3 value1, Vector3 value2) =>
        new(
            value1.X < value2.X ? value1.X : value2.X,
            value1.Y < value2.Y ? value1.Y : value2.Y,
            value1.Z < value2.Z ? value1.Z : value2.Z);


    /// <summary>
    /// Returns a vector whose elements are the maximum of each of the pairs of elements in the two source vectors.
    /// </summary>
    /// <param name="value1">The first source vector.</param>
    /// <param name="value2">The second source vector.</param>
    /// <returns>The maximized vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Max(Vector3 value1, Vector3 value2) =>
        new(
            value1.X > value2.X ? value1.X : value2.X,
            value1.Y > value2.Y ? value1.Y : value2.Y,
            value1.Z > value2.Z ? value1.Z : value2.Z);


    /// <summary>
    /// Returns a vector whose elements are the absolute values of each of the source vector's elements.
    /// </summary>
    /// <param name="value">The source vector.</param>
    /// <returns>The absolute value vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Abs(Vector3 value) => new(Math.Abs(value.X), Math.Abs(value.Y), Math.Abs(value.Z));


    /// <summary>
    /// Returns a vector whose elements are the square root of each of the source vector's elements.
    /// </summary>
    /// <param name="value">The source vector.</param>
    /// <returns>The square root vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 SquareRoot(Vector3 value) => new((double)Math.Sqrt(value.X), (double)Math.Sqrt(value.Y), (double)Math.Sqrt(value.Z));


    public static Vector3 ProjectOnPlane(Vector3 vector, Vector3 planeNormal)
    {
        // Normalize the plane normal to ensure it's a unit vector.
        planeNormal = Normalize(planeNormal);

        // Calculate the distance of the vector from the plane along the normal.
        double distance = Dot(vector, planeNormal);

        // Project the vector onto the plane.
        Vector3 projectedVector = vector - distance * planeNormal;

        return projectedVector;
    }

    #endregion Public Static Methods


    #region Public Static Operators

    /// <summary>
    /// Adds two vectors together.
    /// </summary>
    /// <param name="left">The first source vector.</param>
    /// <param name="right">The second source vector.</param>
    /// <returns>The summed vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 operator +(Vector3 left, Vector3 right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);


    /// <summary>
    /// Subtracts the second vector from the first.
    /// </summary>
    /// <param name="left">The first source vector.</param>
    /// <param name="right">The second source vector.</param>
    /// <returns>The difference vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 operator -(Vector3 left, Vector3 right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);


    /// <summary>
    /// Multiplies two vectors together.
    /// </summary>
    /// <param name="left">The first source vector.</param>
    /// <param name="right">The second source vector.</param>
    /// <returns>The product vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 operator *(Vector3 left, Vector3 right) => new(left.X * right.X, left.Y * right.Y, left.Z * right.Z);


    /// <summary>
    /// Multiplies a vector by the given scalar.
    /// </summary>
    /// <param name="left">The source vector.</param>
    /// <param name="right">The scalar value.</param>
    /// <returns>The scaled vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 operator *(Vector3 left, double right) => left * new Vector3(right);


    /// <summary>
    /// Multiplies a vector by the given scalar.
    /// </summary>
    /// <param name="left">The scalar value.</param>
    /// <param name="right">The source vector.</param>
    /// <returns>The scaled vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 operator *(double left, Vector3 right) => new Vector3(left) * right;


    /// <summary>
    /// Divides the first vector by the second.
    /// </summary>
    /// <param name="left">The first source vector.</param>
    /// <param name="right">The second source vector.</param>
    /// <returns>The vector resulting from the division.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 operator /(Vector3 left, Vector3 right) => new(left.X / right.X, left.Y / right.Y, left.Z / right.Z);


    /// <summary>
    /// Divides the vector by the given scalar.
    /// </summary>
    /// <param name="value1">The source vector.</param>
    /// <param name="value2">The scalar value.</param>
    /// <returns>The result of the division.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 operator /(Vector3 value1, double value2)
    {
        double invDiv = 1.0 / value2;

        return new Vector3(
            value1.X * invDiv,
            value1.Y * invDiv,
            value1.Z * invDiv);
    }


    /// <summary>
    /// Negates a given vector.
    /// </summary>
    /// <param name="value">The source vector.</param>
    /// <returns>The negated vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 operator -(Vector3 value) => Zero - value;


    /// <summary>
    /// Returns a boolean indicating whether the two given vectors are equal.
    /// </summary>
    /// <param name="left">The first vector to compare.</param>
    /// <param name="right">The second vector to compare.</param>
    /// <returns>True if the vectors are equal; False otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Vector3 left, Vector3 right) =>
        left.X == right.X &&
        left.Y == right.Y &&
        left.Z == right.Z;


    /// <summary>
    /// Returns a boolean indicating whether the two given vectors are not equal.
    /// </summary>
    /// <param name="left">The first vector to compare.</param>
    /// <param name="right">The second vector to compare.</param>
    /// <returns>True if the vectors are not equal; False if they are equal.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Vector3 left, Vector3 right) =>
        left.X != right.X ||
        left.Y != right.Y ||
        left.Z != right.Z;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator System.Numerics.Vector3(Vector3 value) => new((float)value.X, (float)value.Y, (float)value.Z);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector3(System.Numerics.Vector3 value) => new(value.X, value.Y, value.Z);

    #endregion Public operator methods
}