// MIT License
// Copyright (C) 2024 KorpiEngine Team.
// Copyright (C) 2019 VIMaec LLC.
// Copyright (C) 2019 Ara 3D. Inc
// https://ara3d.com
// Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace KorpiEngine;

public static partial class MathOps
{
    /// <inheritdoc cref="float.MinValue"/>
    public const float EPSILON_FLOAT = float.MinValue;

    /// <inheritdoc cref="double.MinValue"/>
    public const double EPSILON_DOUBLE = double.MinValue;
    
    
    public static float MoveTowards(float current, float target, float maxDelta)
    {
        if (Math.Abs(target - current) <= maxDelta)
            return target;
        return current + Math.Sign(target - current) * maxDelta;
    }
    
    
    /// <summary>
    /// Expresses two values as a ratio
    /// </summary>
    public static double Percentage(double denominator, double numerator) => numerator / denominator * 100.0;


    /// <summary>
    /// Calculate the nearest power of 2 from the input number
    /// </summary>
    public static int ToNearestPowOf2(int x) => (int)Math.Pow(2, Math.Round(Math.Log(x) / Math.Log(2)));


    /// <summary>
    /// Performs a Catmull-Rom interpolation using the specified positions.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CatmullRom(this float value1, float value2, float value3, float value4, float amount)
    {
        // Using formula from http://www.mvps.org/directx/articles/catmull/
        // Internally using doubles not to lose precision
        double amountSquared = amount * amount;
        double amountCubed = amountSquared * amount;
        return (float)(0.5 *
                       (2.0 * value2 +
                        (value3 - value1) * amount +
                        (2.0 * value1 - 5.0 * value2 + 4.0 * value3 - value4) * amountSquared +
                        (3.0 * value2 - value1 - 3.0 * value3 + value4) * amountCubed));
    }


    /// <summary>
    /// Creates a new <see cref="Vector3"/> that contains CatmullRom interpolation of the specified vectors.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 CatmullRom(this Vector3 value1, Vector3 value2, Vector3 value3, Vector3 value4, float amount) =>
        new(
            value1.X.CatmullRom(value2.X, value3.X, value4.X, amount), value1.Y.CatmullRom(value2.Y, value3.Y, value4.Y, amount),
            value1.Z.CatmullRom(value2.Z, value3.Z, value4.Z, amount));


    /// <summary>
    /// Performs a Hermite spline interpolation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Hermite(this float value1, float tangent1, float value2, float tangent2, float amount)
    {
        // All transformed to double not to lose precision
        // Otherwise, for high numbers of param:amount the result is NaN instead of Infinity
        double v1 = value1,
            v2 = value2,
            t1 = tangent1,
            t2 = tangent2,
            s = amount,
            result;
        double sCubed = s * s * s;
        double sSquared = s * s;

        if (amount == 0f)
            result = value1;
        else if (amount == 1f)
            result = value2;
        else
        {
            result = (2 * v1 - 2 * v2 + t2 + t1) * sCubed +
                     (3 * v2 - 3 * v1 - 2 * t1 - t2) * sSquared +
                     t1 * s +
                     v1;
        }

        return (float)result;
    }


    /// <summary>
    /// Creates a new <see cref="Vector3"/> that contains hermite spline interpolation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Hermite(this Vector3 value1, Vector3 tangent1, Vector3 value2, Vector3 tangent2, float amount) =>
        new(
            value1.X.Hermite(tangent1.X, value2.X, tangent2.X, amount), value1.Y.Hermite(tangent1.Y, value2.Y, tangent2.Y, amount),
            value1.Z.Hermite(tangent1.Z, value2.Z, tangent2.Z, amount));


    /// <summary>
    /// Interpolates between two values using a cubic equation (Hermite),
    /// clamping the amount to 0 to 1
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SmoothStep(this float value1, float value2, float amount) => Hermite(value1, 0f, value2, 0f, Clamp(amount, 0f, 1f));


    /// <summary>
    /// Reduces a given angle to a value between π and -π.
    /// </summary>
    /// <param name="angle">The angle to reduce, in radians.</param>
    /// <returns>The new angle, in radians.</returns>
    public static float WrapAngleRadians(this float angle)
    {
        if (angle > -Constants.PI && angle <= Constants.PI)
            return angle;
        angle %= Constants.TWO_PI;
        if (angle <= -Constants.PI)
            return angle + Constants.TWO_PI;
        if (angle > Constants.PI)
            return angle - Constants.TWO_PI;
        return angle;
    }


    /// <summary>
    /// Reduces all components of the given vector to values between π and -π.
    /// </summary>
    /// <param name="eulers">The vector to reduce, in radians.</param>
    /// <returns></returns>
    public static Vector3 WrapEulersRadians(this Vector3 eulers)
    {
        float x = eulers.X.WrapAngleRadians();
        float y = eulers.Y.WrapAngleRadians();
        float z = eulers.Z.WrapAngleRadians();
        return new Vector3(x, y, z);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNonZeroAndValid(this float self, float tolerance = Constants.TOLERANCE) =>
        !self.IsInfinity() && !self.IsNaN() && self.Abs() > tolerance;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float[] ToFloats(this Matrix4x4 m) =>
    [
        m.M11, m.M12, m.M13, m.M14,
        m.M21, m.M22, m.M23, m.M24,
        m.M31, m.M32, m.M33, m.M34,
        m.M41, m.M42, m.M43, m.M44
    ];


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float[] ToFloats(this Matrix4x4[] matrixArray)
    {
        float[] ret = new float[matrixArray.Length * 16];
        for (int i = 0; i < matrixArray.Length; i++)
        {
            int j = i * 16;
            ret[j + 0] = matrixArray[i].M11;
            ret[j + 1] = matrixArray[i].M12;
            ret[j + 2] = matrixArray[i].M13;
            ret[j + 3] = matrixArray[i].M14;
            ret[j + 4] = matrixArray[i].M21;
            ret[j + 5] = matrixArray[i].M22;
            ret[j + 6] = matrixArray[i].M23;
            ret[j + 7] = matrixArray[i].M24;
            ret[j + 8] = matrixArray[i].M31;
            ret[j + 9] = matrixArray[i].M32;
            ret[j + 10] = matrixArray[i].M33;
            ret[j + 11] = matrixArray[i].M34;
            ret[j + 12] = matrixArray[i].M41;
            ret[j + 13] = matrixArray[i].M42;
            ret[j + 14] = matrixArray[i].M43;
            ret[j + 15] = matrixArray[i].M44;
        }

        return ret;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4x4[] ToMatrixArray(this float[] m)
    {
        Debug.Assert(m.Length % 16 == 0);
        Matrix4x4[] ret = new Matrix4x4[m.Length / 16];

        for (int i = 0; i < ret.Length; i++)
        {
            int i16 = i * 16;
            ret[i] = new Matrix4x4(
                m[i16 + 0], m[i16 + 1], m[i16 + 2], m[i16 + 3],
                m[i16 + 4], m[i16 + 5], m[i16 + 6], m[i16 + 7],
                m[i16 + 8], m[i16 + 9], m[i16 + 10], m[i16 + 11],
                m[i16 + 12], m[i16 + 13], m[i16 + 14], m[i16 + 15]
            );
        }

        return ret;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AABox[] ToAABoxArray(this float[] m)
    {
        const int numFloats = 6;
        Debug.Assert(m.Length % numFloats == 0);
        AABox[] ret = new AABox[m.Length / numFloats];

        for (int i = 0; i < ret.Length; i++)
        {
            int i6 = i * numFloats;
            ret[i] = new AABox(
                new Vector3(m[i6 + 0], m[i6 + 1], m[i6 + 2]),
                new Vector3(m[i6 + 3], m[i6 + 4], m[i6 + 5])
            );
        }

        return ret;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Ray RayFromProjectionMatrix(this Matrix4x4 projection, Vector2 normalisedScreenCoordinates)
    {
        Matrix4x4 invProjection = projection.Inverse();

        Vector2 invertedY = new(normalisedScreenCoordinates.X, 1.0f - normalisedScreenCoordinates.Y);
        Vector2 scalesNormalisedScreenCoordinates = invertedY * 2.0f - 1.0f;

        Vector4 p0 = new(scalesNormalisedScreenCoordinates, 0.0f, 1.0f);
        Vector4 p1 = new(scalesNormalisedScreenCoordinates, 1.0f, 1.0f);

        p0 = p0.Transform(invProjection);
        p1 = p1.Transform(invProjection);

        p0 /= p0.W;
        p1 /= p1.W;

        Ray ret = new(p0.ToVector3(), (p1 - p0).ToVector3().Normalize());
        return ret;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4x4 Inverse(this Matrix4x4 m) => Matrix4x4.Invert(m, out Matrix4x4 r) ? r : throw new InvalidOperationException("No inversion of matrix available");


    /// <summary>
    /// Transforms a vector by the given Quaternion rotation value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Transform(this Vector4 value, Matrix4x4 matrix) => value.Transform(matrix);


    /// <summary>
    /// Transforms a vector by the given Quaternion rotation value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 Transform(this Vector4 value, Quaternion rotation)
    {
        float x2 = rotation.X + rotation.X;
        float y2 = rotation.Y + rotation.Y;
        float z2 = rotation.Z + rotation.Z;

        float wx2 = rotation.W * x2;
        float wy2 = rotation.W * y2;
        float wz2 = rotation.W * z2;
        float xx2 = rotation.X * x2;
        float xy2 = rotation.X * y2;
        float xz2 = rotation.X * z2;
        float yy2 = rotation.Y * y2;
        float yz2 = rotation.Y * z2;
        float zz2 = rotation.Z * z2;

        return new Vector4(
            value.X * (1.0f - yy2 - zz2) + value.Y * (xy2 - wz2) + value.Z * (xz2 + wy2),
            value.X * (xy2 + wz2) + value.Y * (1.0f - xx2 - zz2) + value.Z * (yz2 - wx2),
            value.X * (xz2 - wy2) + value.Y * (yz2 + wx2) + value.Z * (1.0f - xx2 - yy2),
            value.W);
    }


    /// <summary>
    /// Transforms a vector by the given Quaternion rotation value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Transform(this Vector3 value, Quaternion rotation)
    {
        float x2 = rotation.X + rotation.X;
        float y2 = rotation.Y + rotation.Y;
        float z2 = rotation.Z + rotation.Z;

        float wx2 = rotation.W * x2;
        float wy2 = rotation.W * y2;
        float wz2 = rotation.W * z2;
        float xx2 = rotation.X * x2;
        float xy2 = rotation.X * y2;
        float xz2 = rotation.X * z2;
        float yy2 = rotation.Y * y2;
        float yz2 = rotation.Y * z2;
        float zz2 = rotation.Z * z2;

        return new Vector3(
            value.X * (1.0f - yy2 - zz2) + value.Y * (xy2 - wz2) + value.Z * (xz2 + wy2),
            value.X * (xy2 + wz2) + value.Y * (1.0f - xx2 - zz2) + value.Z * (yz2 - wx2),
            value.X * (xz2 - wy2) + value.Y * (yz2 + wx2) + value.Z * (1.0f - xx2 - yy2));
    }


    /// <summary>
    /// Transforms a vector by the given matrix.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Transform(this Vector3 value, Matrix4x4 mat) => value.Transform(mat);


    /// <summary>
    /// Transforms a vector by the given Quaternion rotation value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Transform(this Vector2 value, Quaternion rotation)
    {
        float x2 = rotation.X + rotation.X;
        float y2 = rotation.Y + rotation.Y;
        float z2 = rotation.Z + rotation.Z;

        float wz2 = rotation.W * z2;
        float xx2 = rotation.X * x2;
        float xy2 = rotation.X * y2;
        float yy2 = rotation.Y * y2;
        float zz2 = rotation.Z * z2;

        return new Vector2(
            value.X * (1.0f - yy2 - zz2) + value.Y * (xy2 - wz2),
            value.X * (xy2 + wz2) + value.Y * (1.0f - xx2 - zz2));
    }


    /// <summary>
    /// Transforms a vector by the given matrix.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Transform(this Vector2 position, Matrix4x4 matrix) =>
        new(
            position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M41,
            position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M42);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 ToVector2(this float v) => new(v);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 ToVector2(this Vector3 v) => new(v.X, v.Y);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 ToVector2(this Vector4 v) => new(v.X, v.Y);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 ToVector3(this float v) => new(v);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 ToVector3(this Vector2 v) => new(v.X, v.Y, 0);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 ToVector3(this Vector4 v) => new(v.X, v.Y, v.Z);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 ToVector4(this float v) => new(v);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 ToVector4(this Vector2 v) => new(v.X, v.Y, 0, 0);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 ToVector4(this Vector3 v) => new(v.X, v.Y, v.Z, 0);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Rotate(this Vector3 self, Vector3 axis, float angle) => self.Transform(Matrix4x4.CreateFromAxisAngle(axis, angle));


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNonZeroAndValid(this Vector3 self) => self.LengthSquared().IsNonZeroAndValid();


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsZeroOrInvalid(this Vector3 self) => !self.IsNonZeroAndValid();


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPerpendicular(this Vector3 v1, Vector3 v2, float tolerance = Constants.TOLERANCE)

        // If either vector is vector(0,0,0) the vectors are not perpendicular
        =>
            v1 != Vector3.Zero && v2 != Vector3.Zero && v1.Dot(v2).AlmostZero(tolerance);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Projection(this Vector3 v1, Vector3 v2) => v2 * (v1.Dot(v2) / v2.LengthSquared());


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Rejection(this Vector3 v1, Vector3 v2) => v1 - v1.Projection(v2);


    // The smaller of the two possible angles between the two vectors is returned, therefore, the result will never be greater than 180 degrees or smaller than -180 degrees.
    // If you imagine the from and to vectors as lines on a piece of paper, both originating from the same point, then the /axis/ vector would point up out of the paper.
    // The measured angle between the two vectors would be positive in a clockwise direction and negative in an anti-clockwise direction.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SignedAngle(Vector3 from, Vector3 to, Vector3 axis) => Angle(from, to) * Math.Sign(axis.Dot(from.Cross(to)));


    // The smaller of the two possible angles between the two vectors is returned, therefore, the result will never be greater than 180 degrees or smaller than -180 degrees.
    // If you imagine the from and to vectors as lines on a piece of paper, both originating from the same point, then the /axis/ vector would point up out of the paper.
    // The measured angle between the two vectors would be positive in a clockwise direction and negative in an anti-clockwise direction.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SignedAngle(this Vector3 from, Vector3 to) => SignedAngle(from, to, Vector3.UnitZ);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Angle(this Vector3 v1, Vector3 v2, float tolerance = Constants.TOLERANCE)
    {
        float d = v1.LengthSquared().Sqrt() * v2.LengthSquared().Sqrt();
        if (d < tolerance)
            return 0;
        return (v1.Dot(v2) / d).Clamp(-1F, 1F).Acos();
    }
    
    
    /// <returns>The angle in degrees between two Quaternions.</returns>
    public static float AngleDegrees(this Quaternion q1, Quaternion q2) => AngleRadians(q1, q2).ToDegrees();


    /// <returns>The angle in radians between two Quaternions.</returns>
    public static float AngleRadians(this Quaternion q1, Quaternion q2)
    {
        float dot = Dot(q1, q2);
        return (float)Math.Acos(Math.Min(Math.Abs(dot), 1.0f)) * 2.0f;
    }


    /// <summary>
    /// Calculates the dot product of two Quaternions.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Dot(this Quaternion quaternion1, Quaternion quaternion2) =>
        quaternion1.X * quaternion2.X +
        quaternion1.Y * quaternion2.Y +
        quaternion1.Z * quaternion2.Z +
        quaternion1.W * quaternion2.W;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Colinear(this Vector3 v1, Vector3 v2, float tolerance = Constants.TOLERANCE) =>
        !v1.IsNaN() && !v2.IsNaN() && v1.SignedAngle(v2) <= tolerance;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBackFace(this Vector3 normal, Vector3 lineOfSight) => normal.Dot(lineOfSight) < 0;


    /// <summary>
    /// Creates a new <see cref="Vector3"/> that contains cubic interpolation of the specified vectors.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 SmoothStep(this Vector3 value1, Vector3 value2, float amount) =>
        new(value1.X.SmoothStep(value2.X, amount), value1.Y.SmoothStep(value2.Y, amount), value1.Z.SmoothStep(value2.Z, amount));


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Line ToLine(this Vector3 v) => new(Vector3.Zero, v);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Along(this Vector3 v, float d) => v.Normalize() * d;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 AlongX(this float self) => Vector3.UnitX * self;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 AlongY(this float self) => Vector3.UnitY * self;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 AlongZ(this float self) => Vector3.UnitX * self;


    /// <summary>
    /// Returns the reflection of a vector off a surface that has the specified normal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 Reflect(Vector2 vector, Vector2 normal) => vector - 2 * (vector.Dot(normal) * normal);


    /// <summary>
    /// Returns the reflection of a vector off a surface that has the specified normal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Reflect(Vector3 vector, Vector3 normal) => vector - 2 * (vector.Dot(normal) * normal);


    /// <summary>
    /// Transforms a vector normal by the given matrix.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 TransformNormal(Vector2 normal, Matrix4x4 matrix) =>
        new(
            normal.X * matrix.M11 + normal.Y * matrix.M21,
            normal.X * matrix.M12 + normal.Y * matrix.M22);


    /// <summary>
    /// Transforms a vector normal by the given matrix.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 TransformNormal(Vector3 normal, Matrix4x4 matrix) =>
        new(
            normal.X * matrix.M11 + normal.Y * matrix.M21 + normal.Z * matrix.M31,
            normal.X * matrix.M12 + normal.Y * matrix.M22 + normal.Z * matrix.M32,
            normal.X * matrix.M13 + normal.Y * matrix.M23 + normal.Z * matrix.M33
        );


    /// <summary>
    /// Transforms a vector normal by the given matrix.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 TransformNormal(Vector4 normal, Matrix4x4 matrix) =>
        new(
            normal.X * matrix.M11 + normal.Y * matrix.M21 + normal.Z * matrix.M31 + normal.W * matrix.M41,
            normal.X * matrix.M12 + normal.Y * matrix.M22 + normal.Z * matrix.M32 + normal.W * matrix.M42,
            normal.X * matrix.M13 + normal.Y * matrix.M23 + normal.Z * matrix.M33 + normal.W * matrix.M43,
            normal.X * matrix.M14 + normal.Y * matrix.M24 + normal.Z * matrix.M34 + normal.W * matrix.M44
        );


    /// <summary>
    /// Transforms a vector by the given Quaternion rotation value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 TransformToVector4(Vector2 value, Quaternion rotation)
    {
        float x2 = rotation.X + rotation.X;
        float y2 = rotation.Y + rotation.Y;
        float z2 = rotation.Z + rotation.Z;

        float wx2 = rotation.W * x2;
        float wy2 = rotation.W * y2;
        float wz2 = rotation.W * z2;
        float xx2 = rotation.X * x2;
        float xy2 = rotation.X * y2;
        float xz2 = rotation.X * z2;
        float yy2 = rotation.Y * y2;
        float yz2 = rotation.Y * z2;
        float zz2 = rotation.Z * z2;

        return new Vector4(
            value.X * (1.0f - yy2 - zz2) + value.Y * (xy2 - wz2),
            value.X * (xy2 + wz2) + value.Y * (1.0f - xx2 - zz2),
            value.X * (xz2 - wy2) + value.Y * (yz2 + wx2),
            1.0f);
    }


    /// <summary>
    /// Transforms a vector by the given Quaternion rotation value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 TransformToVector4(Vector3 value, Quaternion rotation)
    {
        float x2 = rotation.X + rotation.X;
        float y2 = rotation.Y + rotation.Y;
        float z2 = rotation.Z + rotation.Z;

        float wx2 = rotation.W * x2;
        float wy2 = rotation.W * y2;
        float wz2 = rotation.W * z2;
        float xx2 = rotation.X * x2;
        float xy2 = rotation.X * y2;
        float xz2 = rotation.X * z2;
        float yy2 = rotation.Y * y2;
        float yz2 = rotation.Y * z2;
        float zz2 = rotation.Z * z2;

        return new Vector4(
            value.X * (1.0f - yy2 - zz2) + value.Y * (xy2 - wz2) + value.Z * (xz2 + wy2),
            value.X * (xy2 + wz2) + value.Y * (1.0f - xx2 - zz2) + value.Z * (yz2 - wx2),
            value.X * (xz2 - wy2) + value.Y * (yz2 + wx2) + value.Z * (1.0f - xx2 - yy2),
            1.0f);
    }


    /// <summary>
    /// Transforms a vector by the given matrix.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 TransformToVector4(Vector2 position, Matrix4x4 matrix) =>
        new(
            position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M41,
            position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M42,
            position.X * matrix.M13 + position.Y * matrix.M23 + matrix.M43,
            position.X * matrix.M14 + position.Y * matrix.M24 + matrix.M44);


    /// <summary>
    /// Transforms a vector by the given matrix.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 TransformToVector4(Vector3 position, Matrix4x4 matrix) =>
        new(
            position.X * matrix.M11 + position.Y * matrix.M21 + position.Z * matrix.M31 + matrix.M41,
            position.X * matrix.M12 + position.Y * matrix.M22 + position.Z * matrix.M32 + matrix.M42,
            position.X * matrix.M13 + position.Y * matrix.M23 + position.Z * matrix.M33 + matrix.M43,
            position.X * matrix.M14 + position.Y * matrix.M24 + position.Z * matrix.M34 + matrix.M44);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Cross(Vector3 a, Vector3 b) => a.Cross(b);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DVector3 Cross(DVector3 a, DVector3 b) => a.Cross(b);


    /// <summary>
    /// Returns the bounding box, given stats on a Vector3
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AABox ToBox(this Stats<Vector3> stats) => new(stats.Min, stats.Max);


    /// <summary>
    /// Returns the bounding box, given stats on a DVector3
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DAABox ToBox(this Stats<DVector3> stats) => new(stats.Min, stats.Max);


    /// <summary>
    /// Returns the bounding box for a series of points
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AABox ToBox(this IEnumerable<Vector3> points) => AABox.Create(points);


    /// <summary>
    /// Returns true if the four points are co-planar. 
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Coplanar(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float epsilon = Constants.TOLERANCE) =>
        Math.Abs(Vector3.Dot(v3 - v1, (v2 - v1).Cross(v4 - v1))) < epsilon;


    /// <summary>
    /// Returns a matrix from a float array.
    /// </summary>
    /// <param name="m"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4x4 ToMatrix(this float[] m) =>
        new(
            m[0], m[1], m[2], m[3], m[4], m[5], m[6], m[7], m[8], m[9], m[10], m[11], m[12], m[13],
            m[14], m[15]);


    /// <summary>
    /// Returns a translation matrix. 
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4x4 ToMatrix(this Vector3 self) => Matrix4x4.CreateTranslation(self);


    /// <summary>
    /// Returns a rotation matrix. 
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4x4 ToMatrix(this Quaternion self) => Matrix4x4.CreateRotation(self);


    /// <summary>
    /// Returns a matrix for translation and then rotation. 
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4x4 ToMatrix(this TransformData self) => Matrix4x4.CreateTRS(self.Position, self.Orientation, self.Scale);


    /// <summary>
    ///  Linearly interpolates between two quaternions.
    /// </summary>
    /// (1.0f - 1) 
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion Lerp(this Quaternion q1, Quaternion q2, float t) =>
        (MathOps.Dot(q1, q2) >= 0.0f
            ? q1 * (1.0f - t) + q2 * t
            : q1 * (1.0f - t) - q2 * t).Normalize();


    /// <summary>
    /// Interpolates between two quaternions, using spherical linear interpolation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion Slerp(this Quaternion q1, Quaternion q2, float t)
    {
        const float epsilon = 1e-6f;

        float cosOmega = q1.X * q2.X +
                         q1.Y * q2.Y +
                         q1.Z * q2.Z +
                         q1.W * q2.W;

        bool flip = false;

        if (cosOmega < 0.0f)
        {
            flip = true;
            cosOmega = -cosOmega;
        }

        float s1,
            s2;

        if (cosOmega > 1.0f - epsilon)
        {
            // Too close, do straight linear interpolation.
            s1 = 1.0f - t;
            s2 = flip ? -t : t;
        }
        else
        {
            float omega = cosOmega.Acos();
            float invSinOmega = 1 / omega.Sin();

            s1 = ((1.0f - t) * omega).Sin() * invSinOmega;
            s2 = flip
                ? -(t * omega).Sin() * invSinOmega
                : (t * omega).Sin() * invSinOmega;
        }

        return q1 * s1 + q2 * s2;
    }


    /// <summary>
    /// Concatenates two Quaternions; the result represents the value1 rotation followed by the value2 rotation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion Concatenate(this Quaternion value1, Quaternion value2)
    {
        // Concatenate rotation is actually q2 * q1 instead of q1 * q2.
        // So that's why value2 goes q1 and value1 goes q2.
        float q1x = value2.X;
        float q1y = value2.Y;
        float q1z = value2.Z;
        float q1w = value2.W;

        float q2x = value1.X;
        float q2y = value1.Y;
        float q2z = value1.Z;
        float q2w = value1.W;

        // cross(av, bv)
        float cx = q1y * q2z - q1z * q2y;
        float cy = q1z * q2x - q1x * q2z;
        float cz = q1x * q2y - q1y * q2x;

        float dot = q1x * q2x + q1y * q2y + q1z * q2z;

        return new Quaternion(
            q1x * q2w + q2x * q1w + cx,
            q1y * q2w + q2y * q1w + cy,
            q1z * q2w + q2z * q1w + cz,
            q1w * q2w - dot);
    }
}