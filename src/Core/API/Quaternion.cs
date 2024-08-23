﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;

namespace KorpiEngine.Core.API;

/// <summary>
/// A structure encapsulating a four-dimensional vector (x,y,z,w), 
/// which is used to efficiently rotate an object about the (x,y,z) vector by the angle theta, where w = cos(theta/2).
/// </summary>
public struct Quaternion : IEquatable<Quaternion>
{
    /// <summary>
    /// Specifies the X-value of the vector component of the Quaternion.
    /// </summary>
    public double X;

    /// <summary>
    /// Specifies the Y-value of the vector component of the Quaternion.
    /// </summary>
    public double Y;

    /// <summary>
    /// Specifies the Z-value of the vector component of the Quaternion.
    /// </summary>
    public double Z;

    /// <summary>
    /// Specifies the rotation component of the Quaternion.
    /// </summary>
    public double W;

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
                    throw new IndexOutOfRangeException("Invalid Quaternion index!");
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
                    throw new IndexOutOfRangeException("Invalid Quaternion index!");
            }
        }
    }

    public Vector3 EulerAngles
    {
        get => this.GetRotation().ToDeg().NormalizeEulerAngleDegrees();
        set => this = value.NormalizeEulerAngleDegrees().ToRad().GetQuaternion();
    }

    /// <summary>
    /// Returns a Quaternion representing no rotation. 
    /// </summary>
    public static Quaternion Identity => new(0, 0, 0, 1);


    /// <summary>
    /// Constructs a Quaternion from the given components.
    /// </summary>
    /// <param name="x">The X component of the Quaternion.</param>
    /// <param name="y">The Y component of the Quaternion.</param>
    /// <param name="z">The Z component of the Quaternion.</param>
    /// <param name="w">The W component of the Quaternion.</param>
    public Quaternion(double x, double y, double z, double w)
    {
        this.X = x;
        this.Y = y;
        this.Z = z;
        this.W = w;
    }


    /// <summary>
    /// Constructs a Quaternion from the given vector and rotation parts.
    /// </summary>
    /// <param name="vectorPart">The vector part of the Quaternion.</param>
    /// <param name="scalarPart">The rotation part of the Quaternion.</param>
    public Quaternion(Vector3 vectorPart, double scalarPart)
    {
        X = vectorPart.X;
        Y = vectorPart.Y;
        Z = vectorPart.Z;
        W = scalarPart;
    }


    public System.Numerics.Quaternion Todouble() => new((float)X, (float)Y, (float)Z, (float)W);


    /// <summary>
    /// Calculates the length of the Quaternion.
    /// </summary>
    /// <returns>The computed length of the Quaternion.</returns>
    public double Magnitude()
    {
        double ls = X * X + Y * Y + Z * Z + W * W;
        return (double)Math.Sqrt((double)ls);
    }


    /// <summary>
    /// Calculates the length squared of the Quaternion. This operation is cheaper than Length().
    /// </summary>
    /// <returns>The length squared of the Quaternion.</returns>
    public double SqrMagnitude() => X * X + Y * Y + Z * Z + W * W;
    
#warning TODO: Test Quaterion.LookRotation


    #region UNTESTED CODE

    public static Quaternion LookRotation(Vector3 forward, Vector3 upwards = default)
    {
        if (upwards == default)
            upwards = Vector3.Up;

        forward = Vector3.Normalize(forward);

        // Return identity if forward is zero
        if (forward == Vector3.Zero)
            return Identity;

        Vector3 right = Vector3.Cross(upwards, forward);

        // If forward and upwards are colinear or upwards is zero, use FromToRotation
        if (right == Vector3.Zero || upwards == Vector3.Zero)
        {
            return FromToRotation(new Vector3(0, 0, 1), forward);
        }

        right = Vector3.Normalize(right);
        Vector3 up = Vector3.Cross(forward, right);

        // Construct a rotation from the right, up, and forward vectors
        Matrix4x4 m = new Matrix4x4(
            right.X, right.Y, right.Z, 0,
            up.X, up.Y, up.Z, 0,
            forward.X, forward.Y, forward.Z, 0,
            0, 0, 0, 1);

        return MatrixToQuaternion(m);
    }
    
    
    public static Quaternion FromToRotation(Vector3 fromDirection, Vector3 toDirection)
    {
        fromDirection = Vector3.Normalize(fromDirection);
        toDirection = Vector3.Normalize(toDirection);

        Vector3 crossProd = Vector3.Cross(fromDirection, toDirection);
        double dotProd = Vector3.Dot(fromDirection, toDirection);
        double angle = Math.Acos(dotProd);

        // Handle parallel vectors
        if (crossProd.Magnitude < Mathd.EPSILON)
        {
            // If vectors are opposite
            if (dotProd < -1 + Mathd.EPSILON)
            {
                // Find an orthogonal vector to use as the rotation axis
                Vector3 orthogonal = Math.Abs(fromDirection.X) < Math.Abs(fromDirection.Y) ? new Vector3(1, 0, 0) : new Vector3(0, 1, 0);
                crossProd = Vector3.Cross(fromDirection, orthogonal);
            }
            else
            {
                // Vectors are the same
                return Identity;
            }
        }

        crossProd = Vector3.Normalize(crossProd);
        return AngleAxis(angle, crossProd);
    }

    #endregion


    public static Quaternion NormalizeSafe(Quaternion q)
    {
        double mag = q.Magnitude();
        if (mag < Mathd.EPSILON)
            return Identity;
        else
            return q / mag;
    }


    /// <summary>
    /// Divides each component of the Quaternion by the length of the Quaternion.
    /// </summary>
    /// <param name="value">The source Quaternion.</param>
    /// <returns>The normalized Quaternion.</returns>
    public static Quaternion Normalize(Quaternion value)
    {
        Quaternion ans;

        double ls = value.X * value.X + value.Y * value.Y + value.Z * value.Z + value.W * value.W;

        double invNorm = 1.0 / (double)Math.Sqrt((double)ls);

        ans.X = value.X * invNorm;
        ans.Y = value.Y * invNorm;
        ans.Z = value.Z * invNorm;
        ans.W = value.W * invNorm;

        return ans;
    }


    /// <summary>
    /// Creates the conjugate of a specified Quaternion.
    /// </summary>
    /// <param name="value">The Quaternion of which to return the conjugate.</param>
    /// <returns>A new Quaternion that is the conjugate of the specified one.</returns>
    public static Quaternion Conjugate(Quaternion value)
    {
        Quaternion ans;

        ans.X = -value.X;
        ans.Y = -value.Y;
        ans.Z = -value.Z;
        ans.W = value.W;

        return ans;
    }


    /// <summary>
    /// Returns the inverse of a Quaternion.
    /// </summary>
    /// <param name="value">The source Quaternion.</param>
    /// <returns>The inverted Quaternion.</returns>
    public static Quaternion Inverse(Quaternion value)
    {
        //  -1   (       a              -v       )
        // q   = ( -------------   ------------- )
        //       (  a^2 + |v|^2  ,  a^2 + |v|^2  )

        Quaternion ans;

        double ls = value.X * value.X + value.Y * value.Y + value.Z * value.Z + value.W * value.W;
        double invNorm = 1.0 / ls;

        ans.X = -value.X * invNorm;
        ans.Y = -value.Y * invNorm;
        ans.Z = -value.Z * invNorm;
        ans.W = value.W * invNorm;

        return ans;
    }


    public static Quaternion Euler(double x, double y, double z) => Euler(new Vector3(x, y, z));

    public static Quaternion Euler(Vector3 euler) => euler.NormalizeEulerAngleDegrees().ToRad().GetQuaternion();

    public Vector3 ToEuler() => this.GetRotation().ToDeg().NormalizeEulerAngleDegrees();

    
    /// <summary>
    /// Compares two Quaternions for approximate equality, using a default tolerance value.
    /// </summary>
    /// <param name="q1">First Quaternion to compare.</param>
    /// <param name="q2">Second Quaternion to compare.</param>
    /// <param name="tolerance">The tolerance value used to determine if the Quaternions are close. 0-1 range.</param>
    /// <returns>If the Quaternions are approximately equal.</returns>
    public static bool Approximately(Quaternion q1, Quaternion q2, float tolerance) => Dot(q1, q2) > 1f - tolerance;


    /// <summary>
    /// Creates a Quaternion from a normalized vector axis and an angle to rotate about the vector.
    /// </summary>
    /// <param name="axis">The unit vector to rotate around.
    /// This vector must be normalized before calling this function or the resulting Quaternion will be incorrect.</param>
    /// <param name="angle">The angle, in radians, to rotate around the vector.</param>
    /// <returns>The created Quaternion.</returns>
    public static Quaternion AngleAxis(double angle, Vector3 axis)
    {
        Quaternion ans;

        double halfAngle = angle * 0.5;
        double s = (double)Math.Sin(halfAngle);
        double c = (double)Math.Cos(halfAngle);

        ans.X = axis.X * s;
        ans.Y = axis.Y * s;
        ans.Z = axis.Z * s;
        ans.W = c;

        return ans;
    }


    /// <summary>
    /// Creates a new Quaternion from the given yaw, pitch, and roll, in radians.
    /// </summary>
    /// <param name="yaw">The yaw angle, in radians, around the Y-axis.</param>
    /// <param name="pitch">The pitch angle, in radians, around the X-axis.</param>
    /// <param name="roll">The roll angle, in radians, around the Z-axis.</param>
    /// <returns></returns>
    public static Quaternion CreateFromYawPitchRoll(double yaw, double pitch, double roll)
    {
        //  Roll first, about axis the object is facing, then
        //  pitch upward, then yaw to face into the new heading
        double sr,
            cr,
            sp,
            cp,
            sy,
            cy;

        double halfRoll = roll * 0.5;
        sr = (double)Math.Sin(halfRoll);
        cr = (double)Math.Cos(halfRoll);

        double halfPitch = pitch * 0.5;
        sp = (double)Math.Sin(halfPitch);
        cp = (double)Math.Cos(halfPitch);

        double halfYaw = yaw * 0.5;
        sy = (double)Math.Sin(halfYaw);
        cy = (double)Math.Cos(halfYaw);

        Quaternion result;

        result.X = cy * sp * cr + sy * cp * sr;
        result.Y = sy * cp * cr - cy * sp * sr;
        result.Z = cy * cp * sr - sy * sp * cr;
        result.W = cy * cp * cr + sy * sp * sr;

        return result;
    }


    /// <summary>
    /// Creates a Quaternion from the given rotation matrix.
    /// </summary>
    /// <param name="matrix">The rotation matrix.</param>
    /// <returns>The created Quaternion.</returns>
    public static Quaternion MatrixToQuaternion(Matrix4x4 matrix)
    {
        double trace = matrix.M11 + matrix.M22 + matrix.M33;

        Quaternion q = new();

        if (trace > 0.0)
        {
            double s = (double)Math.Sqrt(trace + 1.0);
            q.W = s * 0.5;
            s = 0.5 / s;
            q.X = (matrix.M23 - matrix.M32) * s;
            q.Y = (matrix.M31 - matrix.M13) * s;
            q.Z = (matrix.M12 - matrix.M21) * s;
        }
        else
        {
            if (matrix.M11 >= matrix.M22 && matrix.M11 >= matrix.M33)
            {
                double s = (double)Math.Sqrt(1.0 + matrix.M11 - matrix.M22 - matrix.M33);
                double invS = 0.5 / s;
                q.X = 0.5 * s;
                q.Y = (matrix.M12 + matrix.M21) * invS;
                q.Z = (matrix.M13 + matrix.M31) * invS;
                q.W = (matrix.M23 - matrix.M32) * invS;
            }
            else if (matrix.M22 > matrix.M33)
            {
                double s = (double)Math.Sqrt(1.0 + matrix.M22 - matrix.M11 - matrix.M33);
                double invS = 0.5 / s;
                q.X = (matrix.M21 + matrix.M12) * invS;
                q.Y = 0.5 * s;
                q.Z = (matrix.M32 + matrix.M23) * invS;
                q.W = (matrix.M31 - matrix.M13) * invS;
            }
            else
            {
                double s = (double)Math.Sqrt(1.0 + matrix.M33 - matrix.M11 - matrix.M22);
                double invS = 0.5 / s;
                q.X = (matrix.M31 + matrix.M13) * invS;
                q.Y = (matrix.M32 + matrix.M23) * invS;
                q.Z = 0.5 * s;
                q.W = (matrix.M12 - matrix.M21) * invS;
            }
        }

        return q;
    }


    /// <summary>
    /// Calculates the dot product of two Quaternions.
    /// </summary>
    /// <param name="quaternion1">The first source Quaternion.</param>
    /// <param name="quaternion2">The second source Quaternion.</param>
    /// <returns>The dot product of the Quaternions.</returns>
    public static double Dot(Quaternion quaternion1, Quaternion quaternion2) =>
        quaternion1.X * quaternion2.X +
        quaternion1.Y * quaternion2.Y +
        quaternion1.Z * quaternion2.Z +
        quaternion1.W * quaternion2.W;


    /// <summary>
    /// Interpolates between two quaternions, using spherical linear interpolation.
    /// </summary>
    /// <param name="quaternion1">The first source Quaternion.</param>
    /// <param name="quaternion2">The second source Quaternion.</param>
    /// <param name="amount">The relative weight of the second source Quaternion in the interpolation.</param>
    /// <returns>The interpolated Quaternion.</returns>
    public static Quaternion Slerp(Quaternion quaternion1, Quaternion quaternion2, double amount)
    {
        const double epsilon = 1e-6;

        double t = amount;

        double cosOmega = quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y +
                          quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W;

        bool flip = false;

        if (cosOmega < 0.0)
        {
            flip = true;
            cosOmega = -cosOmega;
        }

        double s1,
            s2;

        if (cosOmega > 1.0 - epsilon)
        {
            // Too close, do straight linear interpolation.
            s1 = 1.0 - t;
            s2 = flip ? -t : t;
        }
        else
        {
            double omega = (double)Math.Acos(cosOmega);
            double invSinOmega = (double)(1 / Math.Sin(omega));

            s1 = (double)Math.Sin((1.0 - t) * omega) * invSinOmega;
            s2 = flip
                ? (double)-Math.Sin(t * omega) * invSinOmega
                : (double)Math.Sin(t * omega) * invSinOmega;
        }

        Quaternion ans;

        ans.X = s1 * quaternion1.X + s2 * quaternion2.X;
        ans.Y = s1 * quaternion1.Y + s2 * quaternion2.Y;
        ans.Z = s1 * quaternion1.Z + s2 * quaternion2.Z;
        ans.W = s1 * quaternion1.W + s2 * quaternion2.W;

        return ans;
    }


    /// <summary>
    ///  Linearly interpolates between two quaternions.
    /// </summary>
    /// <param name="quaternion1">The first source Quaternion.</param>
    /// <param name="quaternion2">The second source Quaternion.</param>
    /// <param name="amount">The relative weight of the second source Quaternion in the interpolation.</param>
    /// <returns>The interpolated Quaternion.</returns>
    public static Quaternion Lerp(Quaternion quaternion1, Quaternion quaternion2, double amount)
    {
        double t = amount;
        double t1 = 1.0 - t;

        Quaternion r = new();

        double dot = quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y +
                     quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W;

        if (dot >= 0.0)
        {
            r.X = t1 * quaternion1.X + t * quaternion2.X;
            r.Y = t1 * quaternion1.Y + t * quaternion2.Y;
            r.Z = t1 * quaternion1.Z + t * quaternion2.Z;
            r.W = t1 * quaternion1.W + t * quaternion2.W;
        }
        else
        {
            r.X = t1 * quaternion1.X - t * quaternion2.X;
            r.Y = t1 * quaternion1.Y - t * quaternion2.Y;
            r.Z = t1 * quaternion1.Z - t * quaternion2.Z;
            r.W = t1 * quaternion1.W - t * quaternion2.W;
        }

        // Normalize it.
        double ls = r.X * r.X + r.Y * r.Y + r.Z * r.Z + r.W * r.W;
        double invNorm = 1.0 / (double)Math.Sqrt((double)ls);

        r.X *= invNorm;
        r.Y *= invNorm;
        r.Z *= invNorm;
        r.W *= invNorm;

        return r;
    }


    /// <summary>
    /// Returns the angle in degrees between two rotations.</para>
    /// </summary>
    public static double Angle(Quaternion a, Quaternion b) => Mathd.Acos(Mathd.Min(Mathd.Abs(Dot(a, b)), 1.0)) * 2.0 * Mathd.RAD_2_DEG;


    public static Quaternion RotateTowards(Quaternion from, Quaternion to, double maxDegreesDelta)
    {
        double angle = Angle(from, to);
        return angle == 0.0 ? to : Slerp(from, to, Mathd.Min(1.0, maxDegreesDelta / angle));
    }


    /// <summary>
    /// Concatenates two Quaternions; the result represents the value1 rotation followed by the value2 rotation.
    /// </summary>
    /// <param name="value1">The first Quaternion rotation in the series.</param>
    /// <param name="value2">The second Quaternion rotation in the series.</param>
    /// <returns>A new Quaternion representing the concatenation of the value1 rotation followed by the value2 rotation.</returns>
    public static Quaternion Concatenate(Quaternion value1, Quaternion value2)
    {
        Quaternion ans;

        // Concatenate rotation is actually q2 * q1 instead of q1 * q2.
        // So that's why value2 goes q1 and value1 goes q2.
        double q1x = value2.X;
        double q1y = value2.Y;
        double q1z = value2.Z;
        double q1w = value2.W;

        double q2x = value1.X;
        double q2y = value1.Y;
        double q2z = value1.Z;
        double q2w = value1.W;

        // cross(av, bv)
        double cx = q1y * q2z - q1z * q2y;
        double cy = q1z * q2x - q1x * q2z;
        double cz = q1x * q2y - q1y * q2x;

        double dot = q1x * q2x + q1y * q2y + q1z * q2z;

        ans.X = q1x * q2w + q2x * q1w + cx;
        ans.Y = q1y * q2w + q2y * q1w + cy;
        ans.Z = q1z * q2w + q2z * q1w + cz;
        ans.W = q1w * q2w - dot;

        return ans;
    }


    /// <summary>
    /// Flips the sign of each component of the quaternion.
    /// </summary>
    /// <param name="value">The source Quaternion.</param>
    /// <returns>The negated Quaternion.</returns>
    public static Quaternion Negate(Quaternion value)
    {
        Quaternion ans;

        ans.X = -value.X;
        ans.Y = -value.Y;
        ans.Z = -value.Z;
        ans.W = -value.W;

        return ans;
    }


    /// <summary>
    /// Adds two Quaternions element-by-element.
    /// </summary>
    /// <param name="value1">The first source Quaternion.</param>
    /// <param name="value2">The second source Quaternion.</param>
    /// <returns>The result of adding the Quaternions.</returns>
    public static Quaternion Add(Quaternion value1, Quaternion value2)
    {
        Quaternion ans;

        ans.X = value1.X + value2.X;
        ans.Y = value1.Y + value2.Y;
        ans.Z = value1.Z + value2.Z;
        ans.W = value1.W + value2.W;

        return ans;
    }


    /// <summary>
    /// Subtracts one Quaternion from another.
    /// </summary>
    /// <param name="value1">The first source Quaternion.</param>
    /// <param name="value2">The second Quaternion, to be subtracted from the first.</param>
    /// <returns>The result of the subtraction.</returns>
    public static Quaternion Subtract(Quaternion value1, Quaternion value2)
    {
        Quaternion ans;

        ans.X = value1.X - value2.X;
        ans.Y = value1.Y - value2.Y;
        ans.Z = value1.Z - value2.Z;
        ans.W = value1.W - value2.W;

        return ans;
    }


    /// <summary>
    /// Multiplies two Quaternions together.
    /// </summary>
    /// <param name="value1">The Quaternion on the left side of the multiplication.</param>
    /// <param name="value2">The Quaternion on the right side of the multiplication.</param>
    /// <returns>The result of the multiplication.</returns>
    public static Quaternion Multiply(Quaternion value1, Quaternion value2)
    {
        Quaternion ans;

        double q1x = value1.X;
        double q1y = value1.Y;
        double q1z = value1.Z;
        double q1w = value1.W;

        double q2x = value2.X;
        double q2y = value2.Y;
        double q2z = value2.Z;
        double q2w = value2.W;

        // cross(av, bv)
        double cx = q1y * q2z - q1z * q2y;
        double cy = q1z * q2x - q1x * q2z;
        double cz = q1x * q2y - q1y * q2x;

        double dot = q1x * q2x + q1y * q2y + q1z * q2z;

        ans.X = q1x * q2w + q2x * q1w + cx;
        ans.Y = q1y * q2w + q2y * q1w + cy;
        ans.Z = q1z * q2w + q2z * q1w + cz;
        ans.W = q1w * q2w - dot;

        return ans;
    }


    /// <summary>
    /// Multiplies a Quaternion by a scalar value.
    /// </summary>
    /// <param name="value1">The source Quaternion.</param>
    /// <param name="value2">The scalar value.</param>
    /// <returns>The result of the multiplication.</returns>
    public static Quaternion Multiply(Quaternion value1, double value2)
    {
        Quaternion ans;

        ans.X = value1.X * value2;
        ans.Y = value1.Y * value2;
        ans.Z = value1.Z * value2;
        ans.W = value1.W * value2;

        return ans;
    }


    /// <summary>
    /// Divides a Quaternion by another Quaternion.
    /// </summary>
    /// <param name="value1">The source Quaternion.</param>
    /// <param name="value2">The divisor.</param>
    /// <returns>The result of the division.</returns>
    public static Quaternion Divide(Quaternion value1, Quaternion value2)
    {
        Quaternion ans;

        double q1x = value1.X;
        double q1y = value1.Y;
        double q1z = value1.Z;
        double q1w = value1.W;

        //-------------------------------------
        // Inverse part.
        double ls = value2.X * value2.X + value2.Y * value2.Y +
                    value2.Z * value2.Z + value2.W * value2.W;
        double invNorm = 1.0 / ls;

        double q2x = -value2.X * invNorm;
        double q2y = -value2.Y * invNorm;
        double q2z = -value2.Z * invNorm;
        double q2w = value2.W * invNorm;

        //-------------------------------------
        // Multiply part.

        // cross(av, bv)
        double cx = q1y * q2z - q1z * q2y;
        double cy = q1z * q2x - q1x * q2z;
        double cz = q1x * q2y - q1y * q2x;

        double dot = q1x * q2x + q1y * q2y + q1z * q2z;

        ans.X = q1x * q2w + q2x * q1w + cx;
        ans.Y = q1y * q2w + q2y * q1w + cy;
        ans.Z = q1z * q2w + q2z * q1w + cz;
        ans.W = q1w * q2w - dot;

        return ans;
    }


    /// <summary>
    /// Flips the sign of each component of the quaternion.
    /// </summary>
    /// <param name="value">The source Quaternion.</param>
    /// <returns>The negated Quaternion.</returns>
    public static Quaternion operator -(Quaternion value)
    {
        Quaternion ans;

        ans.X = -value.X;
        ans.Y = -value.Y;
        ans.Z = -value.Z;
        ans.W = -value.W;

        return ans;
    }


    /// <summary>
    /// Adds two Quaternions element-by-element.
    /// </summary>
    /// <param name="value1">The first source Quaternion.</param>
    /// <param name="value2">The second source Quaternion.</param>
    /// <returns>The result of adding the Quaternions.</returns>
    public static Quaternion operator +(Quaternion value1, Quaternion value2)
    {
        Quaternion ans;

        ans.X = value1.X + value2.X;
        ans.Y = value1.Y + value2.Y;
        ans.Z = value1.Z + value2.Z;
        ans.W = value1.W + value2.W;

        return ans;
    }


    /// <summary>
    /// Subtracts one Quaternion from another.
    /// </summary>
    /// <param name="value1">The first source Quaternion.</param>
    /// <param name="value2">The second Quaternion, to be subtracted from the first.</param>
    /// <returns>The result of the subtraction.</returns>
    public static Quaternion operator -(Quaternion value1, Quaternion value2)
    {
        Quaternion ans;

        ans.X = value1.X - value2.X;
        ans.Y = value1.Y - value2.Y;
        ans.Z = value1.Z - value2.Z;
        ans.W = value1.W - value2.W;

        return ans;
    }


    /// <summary>
    /// Multiplies two Quaternions together.
    /// </summary>
    /// <param name="value1">The Quaternion on the left side of the multiplication.</param>
    /// <param name="value2">The Quaternion on the right side of the multiplication.</param>
    /// <returns>The result of the multiplication.</returns>
    public static Quaternion operator *(Quaternion value1, Quaternion value2)
    {
        Quaternion ans;

        double q1x = value1.X;
        double q1y = value1.Y;
        double q1z = value1.Z;
        double q1w = value1.W;

        double q2x = value2.X;
        double q2y = value2.Y;
        double q2z = value2.Z;
        double q2w = value2.W;

        // cross(av, bv)
        double cx = q1y * q2z - q1z * q2y;
        double cy = q1z * q2x - q1x * q2z;
        double cz = q1x * q2y - q1y * q2x;

        double dot = q1x * q2x + q1y * q2y + q1z * q2z;

        ans.X = q1x * q2w + q2x * q1w + cx;
        ans.Y = q1y * q2w + q2y * q1w + cy;
        ans.Z = q1z * q2w + q2z * q1w + cz;
        ans.W = q1w * q2w - dot;

        return ans;
    }


    /// <summary>
    /// Multiplies a Quaternion by a scalar value.
    /// </summary>
    /// <param name="value1">The source Quaternion.</param>
    /// <param name="value2">The scalar value.</param>
    /// <returns>The result of the multiplication.</returns>
    public static Quaternion operator *(Quaternion value1, double value2)
    {
        Quaternion ans;

        ans.X = value1.X * value2;
        ans.Y = value1.Y * value2;
        ans.Z = value1.Z * value2;
        ans.W = value1.W * value2;

        return ans;
    }


    public static Vector3 operator *(Quaternion rotation, Vector3 point)
    {
        double x = rotation.X * 2.0;
        double y = rotation.Y * 2.0;
        double z = rotation.Z * 2.0;
        double xx = rotation.X * x;
        double yy = rotation.Y * y;
        double zz = rotation.Z * z;
        double xy = rotation.X * y;
        double xz = rotation.X * z;
        double yz = rotation.Y * z;
        double wx = rotation.W * x;
        double wy = rotation.W * y;
        double wz = rotation.W * z;

        Vector3 res;
        res.X = (1.0 - (yy + zz)) * point.X + (xy - wz) * point.Y + (xz + wy) * point.Z;
        res.Y = (xy + wz) * point.X + (1.0 - (xx + zz)) * point.Y + (yz - wx) * point.Z;
        res.Z = (xz - wy) * point.X + (yz + wx) * point.Y + (1.0 - (xx + yy)) * point.Z;
        return res;
    }


    /// <summary>
    /// Divides a Quaternion by another Quaternion.
    /// </summary>
    /// <param name="value1">The source Quaternion.</param>
    /// <param name="value2">The divisor.</param>
    /// <returns>The result of the division.</returns>
    public static Quaternion operator /(Quaternion value1, Quaternion value2)
    {
        Quaternion ans;

        double q1x = value1.X;
        double q1y = value1.Y;
        double q1z = value1.Z;
        double q1w = value1.W;

        //-------------------------------------
        // Inverse part.
        double ls = value2.X * value2.X + value2.Y * value2.Y +
                    value2.Z * value2.Z + value2.W * value2.W;
        double invNorm = 1.0 / ls;

        double q2x = -value2.X * invNorm;
        double q2y = -value2.Y * invNorm;
        double q2z = -value2.Z * invNorm;
        double q2w = value2.W * invNorm;

        //-------------------------------------
        // Multiply part.

        // cross(av, bv)
        double cx = q1y * q2z - q1z * q2y;
        double cy = q1z * q2x - q1x * q2z;
        double cz = q1x * q2y - q1y * q2x;

        double dot = q1x * q2x + q1y * q2y + q1z * q2z;

        ans.X = q1x * q2w + q2x * q1w + cx;
        ans.Y = q1y * q2w + q2y * q1w + cy;
        ans.Z = q1z * q2w + q2z * q1w + cz;
        ans.W = q1w * q2w - dot;

        return ans;
    }


    public static Quaternion operator /(Quaternion q, double v) => new(q.X / v, q.Y / v, q.Z / v, q.W / v);


    /// <summary>
    /// Returns a boolean indicating whether the two given Quaternions are equal.
    /// </summary>
    /// <param name="value1">The first Quaternion to compare.</param>
    /// <param name="value2">The second Quaternion to compare.</param>
    /// <returns>True if the Quaternions are equal; False otherwise.</returns>
    public static bool operator ==(Quaternion value1, Quaternion value2) =>
        value1.X == value2.X &&
        value1.Y == value2.Y &&
        value1.Z == value2.Z &&
        value1.W == value2.W;


    /// <summary>
    /// Returns a boolean indicating whether the two given Quaternions are not equal.
    /// </summary>
    /// <param name="value1">The first Quaternion to compare.</param>
    /// <param name="value2">The second Quaternion to compare.</param>
    /// <returns>True if the Quaternions are not equal; False if they are equal.</returns>
    public static bool operator !=(Quaternion value1, Quaternion value2) =>
        value1.X != value2.X ||
        value1.Y != value2.Y ||
        value1.Z != value2.Z ||
        value1.W != value2.W;


    public static implicit operator System.Numerics.Quaternion(Quaternion value) => new((float)value.X, (float)value.Y, (float)value.Z, (float)value.W);

    public static implicit operator Quaternion(System.Numerics.Quaternion value) => new(value.X, value.Y, value.Z, value.W);


    /// <summary>
    /// Returns a boolean indicating whether the given Quaternion is equal to this Quaternion instance.
    /// </summary>
    /// <param name="other">The Quaternion to compare this instance to.</param>
    /// <returns>True if the other Quaternion is equal to this instance; False otherwise.</returns>
    public bool Equals(Quaternion other) =>
        X == other.X &&
        Y == other.Y &&
        Z == other.Z &&
        W == other.W;


    /// <summary>
    /// Returns a boolean indicating whether the given Object is equal to this Quaternion instance.
    /// </summary>
    /// <param name="obj">The Object to compare against.</param>
    /// <returns>True if the Object is equal to this Quaternion; False otherwise.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is Quaternion quaternion)
            return Equals(quaternion);

        return false;
    }


    /// <summary>
    /// Returns a String representing this Quaternion instance.
    /// </summary>
    /// <returns>The string representation.</returns>
    public override string ToString()
    {
        CultureInfo ci = CultureInfo.CurrentCulture;

        return string.Format(ci, "{{X:{0} Y:{1} Z:{2} W:{3}}}", X.ToString(ci), Y.ToString(ci), Z.ToString(ci), W.ToString(ci));
    }


    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode() => X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode() + W.GetHashCode();
}