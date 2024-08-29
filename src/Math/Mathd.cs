using System.Runtime.CompilerServices;

namespace KorpiEngine;

public static class Mathd
{
    private const MethodImplOptions IN = MethodImplOptions.AggressiveInlining;


    #region Constants

    /// <summary>The circle constant. Defined as the circumference of a circle divided by its radius. Equivalent to 2*pi</summary>
    public const double TAU = 6.28318530717959;

    /// <summary>An obscure circle constant. Defined as the circumference of a circle divided by its diameter. Equivalent to 0.5*tau</summary>
    public const double PI = 3.14159265359;

    /// <summary>Euler's number. The base of the natural logarithm. f(x)=e^x is equal to its own derivative</summary>
    public const double E = 2.71828182846;

    /// <summary>The golden ratio. It is the value of a/b where a/b = (a+b)/a. It's the positive root of x^2-x-1</summary>
    public const double GOLDEN_RATIO = 1.61803398875;

    /// <summary>The square root of two. The length of the vector (1,1)</summary>
    public const double SQRT2 = 1.41421356237;

    /// <summary>Multiply an angle in degrees by this, to convert it to radians</summary>
    public const double DEG_2_RAD = TAU / 360;

    /// <summary>Multiply an angle in radians by this, to convert it to degrees</summary>
    public const double RAD_2_DEG = 360 / TAU;

    /// A small but not tiny value, Used in places like ApproximatelyEquals, where there is some tolerance (0.000001)
    public const double SMALL = 0.000001;

    /// <inheritdoc cref="double.MinValue"/>
    public const double EPSILON = double.MinValue;

    /// <inheritdoc cref="double.PositiveInfinity"/>
    public const double INFINITY = double.PositiveInfinity;

    /// <inheritdoc cref="double.NegativeInfinity"/>
    public const double NEGATIVE_INFINITY = double.NegativeInfinity;

    #endregion


    #region Operations/Methods

    [MethodImpl(IN)]
    public static bool IsValid(double x) => !double.IsNaN(x) && !double.IsInfinity(x);


    /// <inheritdoc cref="Math.Sin(double)"/>
    [MethodImpl(IN)]
    public static double Sin(double value) => Math.Sin(value);


    /// <inheritdoc cref="Math.Cos(double)"/>
    [MethodImpl(IN)]
    public static double Cos(double value) => Math.Cos(value);


    /// <inheritdoc cref="Math.Tan(double)"/>
    [MethodImpl(IN)]
    public static double Tan(double value) => Math.Tan(value);


    /// <inheritdoc cref="Math.Asin(double)"/>
    [MethodImpl(IN)]
    public static double Asin(double value) => Math.Asin(value);


    /// <inheritdoc cref="Math.Acos(double)"/>
    [MethodImpl(IN)]
    public static double Acos(double value) => Math.Acos(value);


    /// <inheritdoc cref="Math.Atan(double)"/>
    [MethodImpl(IN)]
    public static double Atan(double value) => Math.Atan(value);


    /// <inheritdoc cref="Math.Atan2(double, double)"/>
    [MethodImpl(IN)]
    public static double Atan(double y, double x) => Math.Atan2(y, x);


    /// <inheritdoc cref="Math.Sqrt(double)"/>
    [MethodImpl(IN)]
    public static double Sqrt(double value) => Math.Sqrt(value);


    /// <inheritdoc cref="Math.Abs(double)"/>
    [MethodImpl(IN)]
    public static double Abs(double value) => Math.Abs(value);


    /// <inheritdoc cref="Math.Pow(double, double)"/>
    [MethodImpl(IN)]
    public static double Pow(double value, double exponent) => Math.Pow(value, exponent);


    /// <inheritdoc cref="Math.Exp(double)"/>
    [MethodImpl(IN)]
    public static double Exp(double power) => Math.Exp(power);


    /// <inheritdoc cref="Math.Log(double, double)"/>
    [MethodImpl(IN)]
    public static double Log(double value, double newBase) => Math.Log(value, newBase);


    /// <inheritdoc cref="Math.Log(double)"/>
    [MethodImpl(IN)]
    public static double Log(double value) => Math.Log(value);


    /// <inheritdoc cref="Math.Log10(double)"/>
    [MethodImpl(IN)]
    public static double Log10(double value) => Math.Log10(value);


    /// <inheritdoc cref="Math.Clamp(double,double,double)"/>
    [MethodImpl(IN)]
    public static double Clamp(double value, double min, double max) => Math.Clamp(value, min, max);


    /// <inheritdoc cref="Math.Clamp(double,double,double)"/>
    [MethodImpl(IN)]
    public static int Clamp(int value, int min, int max) => Math.Clamp(value, min, max);


    [MethodImpl(IN)]
    public static double Clamp01(double value) => Clamp(value, 0, 1);


    [MethodImpl(IN)]
    public static int Min(params int[] values) => values.Min();


    [MethodImpl(IN)]
    public static double Min(params double[] values) => values.Min();


    [MethodImpl(IN)]
    public static int Max(params int[] values) => values.Max();


    [MethodImpl(IN)]
    public static double Max(params double[] values) => values.Max();


    [MethodImpl(IN)]
    public static double Sign(double value) => value >= 0 ? 1 : -1;


    [MethodImpl(IN)]
    public static double Floor(double value) => Math.Floor(value);


    [MethodImpl(IN)]
    public static int FloorToInt(double value) => (int)Math.Floor(value);


    [MethodImpl(IN)]
    public static double Ceil(double value) => Math.Ceiling(value);


    [MethodImpl(IN)]
    public static int CeilToInt(double value) => (int)Math.Ceiling(value);


    [MethodImpl(IN)]
    public static double Round(double value, MidpointRounding midpointRounding = MidpointRounding.ToEven) => Math.Round(value, midpointRounding);


    [MethodImpl(IN)]
    public static double Round(double value, double snapInterval, MidpointRounding midpointRounding = MidpointRounding.ToEven) =>
        Math.Round(value / snapInterval, midpointRounding) * snapInterval;


    [MethodImpl(IN)]
    public static int RoundToInt(double value, MidpointRounding midpointRounding = MidpointRounding.ToEven) => (int)Math.Round(value, midpointRounding);


    [MethodImpl(IN)]
    public static double Frac(double x) => x - Floor(x);


    /// <summary> Repeats the given value in the interval specified by length </summary>
    [MethodImpl(IN)]
    public static double Repeat(double value, double length) => Clamp(value - Floor(value / length) * length, 0.0, length);


    /// <summary> Repeats a value within a range, going back and forth </summary>
    [MethodImpl(IN)]
    public static double PingPong(double t, double length) => length - Abs(Repeat(t, length * 2) - length);


    /// <summary> Cubic EaseInOut </summary>
    [MethodImpl(IN)]
    public static double Smooth01(double x) => x * x * (3 - 2 * x);


    /// <summary> Quintic EaseInOut </summary>
    [MethodImpl(IN)]
    public static double Smoother01(double x) => x * x * x * (x * (x * 6 - 15) + 10);


    [MethodImpl(IN)]
    public static double Lerp(double a, double b, double t) => (1 - t) * a + t * b;


    [MethodImpl(IN)]
    public static Vector2 Lerp(Vector2 a, Vector2 b, Vector2 t) => new(Lerp(a.X, b.X, t.X), Lerp(a.Y, b.Y, t.Y));


    [MethodImpl(IN)]
    public static Vector3 Lerp(Vector3 a, Vector3 b, Vector3 t) => new(Lerp(a.X, b.X, t.X), Lerp(a.Y, b.Y, t.Y), Lerp(a.Z, b.Z, t.Z));


    [MethodImpl(IN)]
    public static Vector4 Lerp(Vector4 a, Vector4 b, Vector4 t) => new(Lerp(a.X, b.X, t.X), Lerp(a.Y, b.Y, t.Y), Lerp(a.Z, b.Z, t.Z), Lerp(a.W, b.W, t.W));


    [MethodImpl(IN)]
    public static double LerpClamped(double a, double b, double t) => Lerp(a, b, Clamp01(t));


    [MethodImpl(IN)]
    public static double LerpSmooth(double a, double b, double t) => Lerp(a, b, Smooth01(Clamp01(t)));


    public static double MoveTowards(double current, double target, double maxDelta)
    {
        if (Math.Abs(target - current) <= maxDelta)
            return target;
        return current + Math.Sign(target - current) * maxDelta;
    }


    public static Vector2 ClampMagnitude(Vector2 v, double min, double max)
    {
        double mag = v.Magnitude;
        if (mag < min)
            return v / mag * min;
        return mag > max ? v / mag * max : v;
    }


    public static Vector3 ClampMagnitude(Vector3 v, double min, double max)
    {
        double mag = v.Magnitude;
        if (mag < min)
            return v / mag * min;
        return mag > max ? v / mag * max : v;
    }


    public static double Angle(Quaternion a, Quaternion b)
    {
        double v = Min(Abs(Quaternion.Dot(a, b)), 1);
        return v > 0.999998986721039 ? 0.0 : Math.Acos(v) * 2.0;
    }


    public static double LerpAngle(double aRad, double bRad, double t)
    {
        double delta = Repeat(bRad - aRad, TAU);
        if (delta > PI)
            delta -= TAU;
        return aRad + delta * Clamp01(t);
    }


    [MethodImpl(IN)]
    public static uint Pack4F(this Vector4 color) => Pack4U((uint)(color.W * 255), (uint)(color.X * 255), (uint)(color.Y * 255), (uint)(color.Z * 255));


    [MethodImpl(IN)]
    public static uint Pack4U(uint a, uint r, uint g, uint b) => (a << 24) + (r << 16) + (g << 8) + b;


    [MethodImpl(IN)]
    public static int ComputeMipLevels(int width, int height) => (int)Math.Log2(Math.Max(width, height));


    [MethodImpl(IN)]
    public static bool ApproximatelyEquals(double a, double b) => Abs(a - b) < 0.00001f;


    [MethodImpl(IN)]
    public static bool ApproximatelyEquals(Vector2 a, Vector2 b) => ApproximatelyEquals(a.X, b.X) && ApproximatelyEquals(a.Y, b.Y);


    [MethodImpl(IN)]
    public static bool ApproximatelyEquals(Vector3 a, Vector3 b) =>
        ApproximatelyEquals(a.X, b.X) && ApproximatelyEquals(a.Y, b.Y) && ApproximatelyEquals(a.Z, b.Z);


    [MethodImpl(IN)]
    public static bool ApproximatelyEquals(Vector4 a, Vector4 b) => ApproximatelyEquals(a.X, b.X) && ApproximatelyEquals(a.Y, b.Y) &&
                                                                    ApproximatelyEquals(a.Z, b.Z) && ApproximatelyEquals(a.W, b.W);


    /// <summary> 
    /// Compute the closest position on a line to point 
    /// </summary>
    public static Vector2 GetClosestPointOnLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        Vector2 p = point - lineStart;
        Vector2 n = lineEnd - lineStart;
        double l2 = n.SqrMagnitude;
        if (l2 < 1e-20f)
            return lineStart; // Both points are the same, just give any.

        double d = Vector2.Dot(n, p) / l2;

        if (d <= 0.0f)
            return lineStart; // Before first point.
        else if (d >= 1.0f)
            return lineEnd; // After first point.
        else
            return lineStart + n * d; // Inside.
    }


    /// <summary>
    /// Checks if two Lines Intersect (Maths.Small Tolerance)
    /// </summary>
    public static bool DoesLineIntersectLine(Vector2 startA, Vector2 endA, Vector2 startB, Vector2 endB, out Vector2 result)
    {
        result = Vector2.Zero;

        Vector2 ab = endA - startA;
        Vector2 ac = startB - startA;
        Vector2 ad = endB - startA;

        double abLen = Vector2.Dot(ab, ab);
        if (abLen <= 0)
            return false;
        Vector2 abNorm = ab / abLen;
        ac = new Vector2(ac.X * abNorm.X + ac.Y * abNorm.Y, ac.Y * abNorm.X - ac.X * abNorm.Y);
        ad = new Vector2(ad.X * abNorm.X + ad.Y * abNorm.Y, ad.Y * abNorm.X - ad.X * abNorm.Y);

        // segments don't intersect
        if ((ac.Y < -SMALL && ad.Y < -SMALL) || (ac.Y > SMALL && ad.Y > SMALL))
            return false;

        if (Abs(ad.Y - ac.Y) < SMALL)
            return false;

        double abPos = ad.X + (ac.X - ad.X) * ad.Y / (ad.Y - ac.Y);
        if (abPos < 0 || abPos > 1)
            return false;

        result = startA + ab * abPos;
        return true;
    }


    /// <summary>
    /// Checks if two 2D lines are Parallel within a tolerance
    /// </summary>
    public static bool AreLinesParallel(Vector2 startA, Vector2 endA, Vector2 startB, Vector2 endB, double tolerance)
    {
        Vector2 segment1 = endA - startA;
        Vector2 segment2 = endB - startB;
        double segment1Length2 = Vector2.Dot(segment1, segment1);
        double segment2Length2 = Vector2.Dot(segment2, segment2);
        double segmentOntoSegment = Vector2.Dot(segment2, segment1);

        if (segment1Length2 < tolerance || segment2Length2 < tolerance)
            return true;

        double maxSeparation2;
        if (segment1Length2 > segment2Length2)
            maxSeparation2 = segment2Length2 - segmentOntoSegment * segmentOntoSegment / segment1Length2;
        else
            maxSeparation2 = segment1Length2 - segmentOntoSegment * segmentOntoSegment / segment2Length2;

        return maxSeparation2 < tolerance;
    }


    /// <summary>
    /// Checks if a Ray intersects a triangle (Uses Maths.Small for Error)
    /// </summary>
    public static bool RayIntersectsTriangle(Vector3 origin, Vector3 dir, Vector3 a, Vector3 b, Vector3 c, out Vector3 intersection)
    {
        intersection = Vector3.Zero;

        Vector3 edge1 = b - a;
        Vector3 edge2 = c - a;
        Vector3 h = Vector3.Cross(dir, edge2);
        double dot = Vector3.Dot(edge1, h);

        // Check if ray is parallel to triangle.
        if (Abs(dot) < SMALL)
            return false;
        double f = 1.0f / dot;

        Vector3 s = origin - a;
        double u = f * Vector3.Dot(s, h);
        if (u < 0.0 - SMALL || u > 1.0 + SMALL)
            return false;

        Vector3 q = Vector3.Cross(s, edge1);
        double v = f * Vector3.Dot(dir, q);
        if (v < 0.0 - SMALL || u + v > 1.0 + SMALL)
            return false;

        // Ray intersects triangle.
        // Calculate distance.
        double t = f * Vector3.Dot(edge2, q);

        // Confirm triangle is in front of ray.
        if (t >= SMALL)
        {
            intersection = origin + dir * t;
            return true;
        }
        else
        {
            return false;
        }
    }


    private static bool Internal_IsPointInTriangle(Vector3 point, Vector3 a, Vector3 b, Vector3 c, int shifted)
    {
        double det = Vector3.Dot(a, Vector3.Cross(b, c));

        // If determinant is, zero try shift the triangle and the point.
        if (Abs(det) < SMALL)
        {
            if (shifted > 2)
                return false; // Triangle appears degenerate, so ignore it.

            Vector3 shiftBy = Vector3.Zero;
            shiftBy[shifted] = 1;
            Vector3 shiftedPoint = point + shiftBy;
            return Internal_IsPointInTriangle(shiftedPoint, a + shiftBy, b + shiftBy, c + shiftBy, shifted + 1);
        }

        // Find the barycentric coordinates of the point with respect to the vertices.
        double[] lambda =
        [
            Vector3.Dot(point, Vector3.Cross(b, c)) / det,
            Vector3.Dot(point, Vector3.Cross(c, a)) / det,
            Vector3.Dot(point, Vector3.Cross(a, b)) / det
        ];

        // Point is in the plane if all lambdas sum to 1.
        if (Abs(lambda[0] + lambda[1] + lambda[2] - 1) >= SMALL)
            return false;

        // The Point is inside the triangle if all lambdas are positive.
        return lambda[0] >= 0 && lambda[1] >= 0 && lambda[2] >= 0;
    }


    /// <summary>
    /// Checks if a 3D Point exists inside a 3D Triangle
    /// </summary>
    public static bool IsPointInTriangle(Vector3 point, Vector3 a, Vector3 b, Vector3 c) => Internal_IsPointInTriangle(point, a, b, c, 0);


    /// <summary>
    /// Checks if a 2D Point exists inside a 2D Triangle
    /// </summary>
    public static bool IsPointInTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
    {
        Vector2 an = a - point;
        Vector2 bn = b - point;
        Vector2 cn = c - point;

        bool orientation = an.X * bn.Y - an.Y * bn.X > 0;

        if (bn.X * cn.Y - bn.Y * cn.X > 0 != orientation)
            return false;

        return cn.X * an.Y - cn.Y * an.X > 0 == orientation;
    }

    #endregion


    #region Extensions

    public static Vector2 ToDouble(this System.Numerics.Vector2 v) => new(v.X, v.Y);

    public static Vector3 ToDouble(this System.Numerics.Vector3 v) => new(v.X, v.Y, v.Z);

    public static Vector4 ToDouble(this System.Numerics.Vector4 v) => new(v.X, v.Y, v.Z, v.W);

    public static Quaternion ToDouble(this System.Numerics.Quaternion v) => new(v.X, v.Y, v.Z, v.W);


    public static Matrix4x4 ToDouble(this System.Numerics.Matrix4x4 m)
    {
        Matrix4x4 result;
        result.M11 = m.M11;
        result.M12 = m.M12;
        result.M13 = m.M13;
        result.M14 = m.M14;

        result.M21 = m.M21;
        result.M22 = m.M22;
        result.M23 = m.M23;
        result.M24 = m.M24;

        result.M31 = m.M31;
        result.M32 = m.M32;
        result.M33 = m.M33;
        result.M34 = m.M34;

        result.M41 = m.M41;
        result.M42 = m.M42;
        result.M43 = m.M43;
        result.M44 = m.M44;
        return result;
    }


    [MethodImpl(IN)]
    public static Vector3 GetRotation(this Quaternion r)
    {
        double yaw = Math.Atan2(2.0 * (r.Y * r.W + r.X * r.Z), 1.0 - 2.0 * (r.X * r.X + r.Y * r.Y));
        double pitch = Math.Asin(2.0 * (r.X * r.W - r.Y * r.Z));
        double roll = Math.Atan2(2.0 * (r.X * r.Y + r.Z * r.W), 1.0 - 2.0 * (r.X * r.X + r.Z * r.Z));

        // If any nan or inf, set that value to 0
        if (double.IsNaN(yaw) || double.IsInfinity(yaw))
            yaw = 0;
        if (double.IsNaN(pitch) || double.IsInfinity(pitch))
            pitch = 0;
        if (double.IsNaN(roll) || double.IsInfinity(roll))
            roll = 0;
        return new Vector3(pitch, yaw, roll);
    }


    [MethodImpl(IN)]
    public static float ToDeg(this float v) => (float)(v * RAD_2_DEG);


    [MethodImpl(IN)]
    public static double ToDeg(this double v) => v * RAD_2_DEG;


    [MethodImpl(IN)]
    public static Vector3 ToDeg(this Vector3 v) => new(v.X * RAD_2_DEG, v.Y * RAD_2_DEG, v.Z * RAD_2_DEG);


    [MethodImpl(IN)]
    public static float ToRad(this float v) => (float)(v * DEG_2_RAD);


    [MethodImpl(IN)]
    public static double ToRad(this double v) => v * DEG_2_RAD;


    [MethodImpl(IN)]
    public static Vector3 ToRad(this Vector3 v) => new(v.X * DEG_2_RAD, v.Y * DEG_2_RAD, v.Z * DEG_2_RAD);


    [MethodImpl(IN)]
    public static Quaternion GetQuaternion(this Vector3 vector) => Quaternion.CreateFromYawPitchRoll(vector.Y, vector.X, vector.Z);


    [MethodImpl(IN)]
    public static Vector3 NormalizeEulerAngleDegrees(this Vector3 angle)
    {
        double normalizedX = angle.X % 360;
        double normalizedY = angle.Y % 360;
        double normalizedZ = angle.Z % 360;
        if (normalizedX < 0)
            normalizedX += 360;

        if (normalizedY < 0)
            normalizedY += 360;

        if (normalizedZ < 0)
            normalizedZ += 360;

        return new Vector3(normalizedX, normalizedY, normalizedZ);
    }

    #endregion
}