#region License

/*
MIT License
Copyright © 2006 The Mono.Xna Team

All rights reserved.

Authors:
Olivier Dufour (Duff)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

#endregion License


using System.Globalization;
using System.Runtime.CompilerServices;

namespace KorpiEngine.Core.API;

public enum PlaneIntersectionType
{
    Front,
    Back,
    Intersecting
}

public struct Plane : IEquatable<Plane>
{
    #region Public Fields

    public double Distance;

    public Vector3 Normal;

    #endregion Public Fields


    #region Constructors

    public Plane(Vector4 value)
        : this(new Vector3(value.X, value.Y, value.Z), value.W)
    {
    }


    public Plane(Vector3 normal, double d)
    {
        Normal = normal;
        Distance = d;
    }


    public Plane(Vector3 a, Vector3 b, Vector3 c)
    {
        Set3Points(a, b, c);
    }


    public Plane(double a, double b, double c, double d)
        : this(new Vector3(a, b, c), d)
    {
    }

    #endregion Constructors


    #region Public Methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Dot(Vector4 value) => Normal.X * value.X + Normal.Y * value.Y + Normal.Z * value.Z + Distance * value.W;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dot(ref Vector4 value, out double result) => result = Normal.X * value.X + Normal.Y * value.Y + Normal.Z * value.Z + Distance * value.W;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double DotCoordinate(Vector3 value) => Normal.X * value.X + Normal.Y * value.Y + Normal.Z * value.Z + Distance;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DotCoordinate(ref Vector3 value, out double result) => result = Normal.X * value.X + Normal.Y * value.Y + Normal.Z * value.Z + Distance;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double DotNormal(Vector3 value) => Normal.X * value.X + Normal.Y * value.Y + Normal.Z * value.Z;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DotNormal(ref Vector3 value, out double result) => result = Normal.X * value.X + Normal.Y * value.Y + Normal.Z * value.Z;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool GetSide(Vector3 inPt) => Vector3.Dot(Normal, inPt) + Distance > 0.0;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsOnPositiveSide(Vector3 point) => Vector3.Dot(Normal, point) > Distance;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double GetDistanceToPoint(Vector3 inPt) => Math.Abs(Vector3.Dot(Normal, inPt) + Distance);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsOnPlane(Vector3 point, double tolerance = 0) => Mathd.Abs(Vector3.Dot(Normal, point) - Distance) <= tolerance;


    public void Normalize()
    {
        Vector3 normal = Normal;
        Normal = Vector3.Normalize(Normal);
        double factor = Math.Sqrt(Normal.X * Normal.X + Normal.Y * Normal.Y + Normal.Z * Normal.Z) /
                        Math.Sqrt(normal.X * normal.X + normal.Y * normal.Y + normal.Z * normal.Z);
        Distance *= factor;
    }


    public void Set3Points(Vector3 a, Vector3 b, Vector3 c)
    {
        Normal = Vector3.Normalize(Vector3.Cross(a - c, a - b));
        Distance = Vector3.Dot(Normal, a);
    }


    public static Plane Normalize(Plane value)
    {
        Plane ret;
        Normalize(ref value, out ret);
        return ret;
    }


    public static void Normalize(ref Plane value, out Plane result)
    {
        double factor;
        result.Normal = Vector3.Normalize(value.Normal);
        factor = Math.Sqrt(result.Normal.X * result.Normal.X + result.Normal.Y * result.Normal.Y + result.Normal.Z * result.Normal.Z) /
                 Math.Sqrt(value.Normal.X * value.Normal.X + value.Normal.Y * value.Normal.Y + value.Normal.Z * value.Normal.Z);
        result.Distance = value.Distance * factor;
    }


    public static bool operator !=(Plane plane1, Plane plane2) => !plane1.Equals(plane2);

    public static bool operator ==(Plane plane1, Plane plane2) => plane1.Equals(plane2);

    public override bool Equals(object? obj) => obj is Plane plane && Equals(plane);

    public bool Equals(Plane other) => Normal == other.Normal && Mathd.ApproximatelyEquals(Distance, other.Distance);

    public override int GetHashCode() => Normal.GetHashCode() ^ Distance.GetHashCode();

    public PlaneIntersectionType Intersects(Bounds box) => box.Intersects(this);

    public void Intersects(ref Bounds box, out PlaneIntersectionType result) => box.Intersects(ref this, out result);


    internal PlaneIntersectionType Intersects(ref Vector3 point)
    {
        double distance;
        DotCoordinate(ref point, out distance);

        if (distance > 0)
            return PlaneIntersectionType.Front;

        if (distance < 0)
            return PlaneIntersectionType.Back;

        return PlaneIntersectionType.Intersecting;
    }


    public bool DoesLineIntersectPlane(Vector3 lineStart, Vector3 lineEnd, out Vector3 result)
    {
        result = Vector3.Zero;

        Vector3 segment = lineStart - lineEnd;
        double den = Vector3.Dot(Normal, segment);

        if (Mathd.Abs(den) < Mathd.SMALL)
            return false;

        double dist = (Vector3.Dot(Normal, lineStart) - Distance) / den;

        if (dist < -Mathd.SMALL || dist > 1.0f + Mathd.SMALL)
            return false;

        dist = -dist;
        result = lineStart + segment * dist;

        return true;
    }


    internal string DebugDisplayString => string.Concat(Normal.ToString(), "  ", Distance.ToString(CultureInfo.InvariantCulture));

    public override string ToString() => "{Normal:" + Normal + " Distance:" + Distance + "}";

    #endregion
}