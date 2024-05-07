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

using OpenTK.Mathematics;

namespace KorpiEngine.Core.API;

public enum ContainmentType
{
    Disjoint,
    Contains,
    Intersects
}

public struct Bounds : IEquatable<Bounds>
{
    #region Public Fields

    public Vector3 min;

    public Vector3 max;

    public const int CornerCount = 8;

    #endregion Public Fields


    #region Public Properties

    public Vector3 center
    {
        get => (min + max) * 0.5f;
        set
        {
            var s = size * 0.5f;
            min = value - s;
            max = value + s;
        }
    }

    public Vector3 extents
    {
        get => (max - min) * 0.5f;
        set
        {
            var c = center;
            min = c - value;
            max = c + value;
        }
    }

    public Vector3 size
    {
        get => max - min;
        set
        {
            var c = center;
            var s = value * 0.5f;
            min = c - s;
            max = c + s;
        }
    }

    #endregion


    #region Public Constructors

    public Bounds(Vector3 center, Vector3 size)
    {
        var hs = size * 0.5f;
        min = center - hs;
        max = center + hs;
    }

    #endregion Public Constructors


    #region Public Methods

    public ContainmentType Contains(Bounds box)
    {
        //test if all corner is in the same side of a face by just checking min and max
        if (box.max.X < min.X
            || box.min.X > max.X
            || box.max.Y < min.Y
            || box.min.Y > max.Y
            || box.max.Z < min.Z
            || box.min.Z > max.Z)
            return ContainmentType.Disjoint;


        if (box.min.X >= min.X
            && box.max.X <= max.X
            && box.min.Y >= min.Y
            && box.max.Y <= max.Y
            && box.min.Z >= min.Z
            && box.max.Z <= max.Z)
            return ContainmentType.Contains;

        return ContainmentType.Intersects;
    }


    public void Contains(ref Bounds box, out ContainmentType result)
    {
        result = Contains(box);
    }


    public ContainmentType Contains(BoundingFrustum frustum)
    {
        //TODO: bad done here need a fix. 
        //Because question is not frustum contain box but reverse and this is not the same
        int i;
        ContainmentType contained;
        Vector3[] corners = frustum.GetCorners();

        // First we check if frustum is in box
        for (i = 0; i < corners.Length; i++)
        {
            Contains(ref corners[i], out contained);
            if (contained == ContainmentType.Disjoint)
                break;
        }

        if (i == corners.Length) // This means we checked all the corners and they were all contain or instersect
            return ContainmentType.Contains;

        if (i != 0) // if i is not equal to zero, we can fastpath and say that this box intersects
            return ContainmentType.Intersects;


        // If we get here, it means the first (and only) point we checked was actually contained in the frustum.
        // So we assume that all other points will also be contained. If one of the points is disjoint, we can
        // exit immediately saying that the result is Intersects
        i++;
        for (; i < corners.Length; i++)
        {
            Contains(ref corners[i], out contained);
            if (contained != ContainmentType.Contains)
                return ContainmentType.Intersects;
        }

        // If we get here, then we know all the points were actually contained, therefore result is Contains
        return ContainmentType.Contains;
    }


    public ContainmentType Contains(Vector3 point)
    {
        ContainmentType result;
        Contains(ref point, out result);
        return result;
    }


    public void Contains(ref Vector3 point, out ContainmentType result)
    {
        //first we get if point is out of box
        if (point.X < min.X
            || point.X > max.X
            || point.Y < min.Y
            || point.Y > max.Y
            || point.Z < min.Z
            || point.Z > max.Z)
            result = ContainmentType.Disjoint;

        //or if point is on box because coordonate of point is lesser or equal
        else if (point.X == min.X
                 || point.X == max.X
                 || point.Y == min.Y
                 || point.Y == max.Y
                 || point.Z == min.Z
                 || point.Z == max.Z)
            result = ContainmentType.Intersects;
        else
            result = ContainmentType.Contains;
    }


    private static readonly Vector3 MaxVector3 = new Vector3(double.MaxValue);
    private static readonly Vector3 MinVector3 = new Vector3(double.MinValue);


    /// <summary>
    /// Create a bounding box from the given list of points.
    /// </summary>
    /// <param name="points">The list of Vector3 instances defining the point cloud to bound</param>
    /// <returns>A bounding box that encapsulates the given point cloud.</returns>
    /// <exception cref="System.ArgumentException">Thrown if the given list has no points.</exception>
    public static Bounds CreateFromPoints(IEnumerable<Vector3> points)
    {
        if (points == null)
            throw new ArgumentNullException();

        bool empty = true;
        var minVec = MaxVector3;
        var maxVec = MinVector3;
        foreach (var ptVector in points)
        {
            minVec.X = minVec.X < ptVector.X ? minVec.X : ptVector.X;
            minVec.Y = minVec.Y < ptVector.Y ? minVec.Y : ptVector.Y;
            minVec.Z = minVec.Z < ptVector.Z ? minVec.Z : ptVector.Z;

            maxVec.X = maxVec.X > ptVector.X ? maxVec.X : ptVector.X;
            maxVec.Y = maxVec.Y > ptVector.Y ? maxVec.Y : ptVector.Y;
            maxVec.Z = maxVec.Z > ptVector.Z ? maxVec.Z : ptVector.Z;

            empty = false;
        }

        if (empty)
            throw new ArgumentException();

        return new Bounds(minVec, maxVec);
    }


    public void Encapsulate(Vector3 point)
    {
        min = Vector3.Min(min, point);
        max = Vector3.Max(max, point);
    }


    public void Encapsulate(Bounds bounds)
    {
        Encapsulate(bounds.center - bounds.extents);
        Encapsulate(bounds.center + bounds.extents);
    }


    public void Expand(double amount)
    {
        extents += new Vector3(amount, amount, amount) * .5;
    }


    public void Expand(Vector3 amount)
    {
        extents += amount * .5;
    }


    public static Bounds CreateMerged(Bounds original, Bounds additional)
    {
        Bounds result;
        CreateMerged(ref original, ref additional, out result);
        return result;
    }


    public static void CreateMerged(ref Bounds original, ref Bounds additional, out Bounds result)
    {
        result.min.X = Math.Min(original.min.X, additional.min.X);
        result.min.Y = Math.Min(original.min.Y, additional.min.Y);
        result.min.Z = Math.Min(original.min.Z, additional.min.Z);
        result.max.X = Math.Max(original.max.X, additional.max.X);
        result.max.Y = Math.Max(original.max.Y, additional.max.Y);
        result.max.Z = Math.Max(original.max.Z, additional.max.Z);
    }


    public bool Equals(Bounds other) => min == other.min && max == other.max;

    public override bool Equals(object? obj) => obj is Bounds bounds ? Equals(bounds) : false;


    public Vector3[] GetCorners()
    {
        return new Vector3[]
        {
            new Vector3(min.X, max.Y, max.Z),
            new Vector3(max.X, max.Y, max.Z),
            new Vector3(max.X, min.Y, max.Z),
            new Vector3(min.X, min.Y, max.Z),
            new Vector3(min.X, max.Y, min.Z),
            new Vector3(max.X, max.Y, min.Z),
            new Vector3(max.X, min.Y, min.Z),
            new Vector3(min.X, min.Y, min.Z)
        };
    }


    public void GetCorners(Vector3[] corners)
    {
        if (corners == null)
            throw new ArgumentNullException("corners");
        if (corners.Length < 8)
            throw new ArgumentOutOfRangeException("corners", "Not Enought Corners");
        corners[0].X = min.X;
        corners[0].Y = max.Y;
        corners[0].Z = max.Z;
        corners[1].X = max.X;
        corners[1].Y = max.Y;
        corners[1].Z = max.Z;
        corners[2].X = max.X;
        corners[2].Y = min.Y;
        corners[2].Z = max.Z;
        corners[3].X = min.X;
        corners[3].Y = min.Y;
        corners[3].Z = max.Z;
        corners[4].X = min.X;
        corners[4].Y = max.Y;
        corners[4].Z = min.Z;
        corners[5].X = max.X;
        corners[5].Y = max.Y;
        corners[5].Z = min.Z;
        corners[6].X = max.X;
        corners[6].Y = min.Y;
        corners[6].Z = min.Z;
        corners[7].X = min.X;
        corners[7].Y = min.Y;
        corners[7].Z = min.Z;
    }


    public override int GetHashCode() => min.GetHashCode() + max.GetHashCode();


    public bool Intersects(Bounds box)
    {
        bool result;
        Intersects(ref box, out result);
        return result;
    }


    public void Intersects(ref Bounds box, out bool result)
    {
        if (max.X >= box.min.X && min.X <= box.max.X)
        {
            if (max.Y < box.min.Y || min.Y > box.max.Y)
            {
                result = false;
                return;
            }

            result = max.Z >= box.min.Z && min.Z <= box.max.Z;
            return;
        }

        result = false;
        return;
    }


    public bool Intersects(BoundingFrustum frustum) => frustum.Intersects(this);


    public PlaneIntersectionType Intersects(Plane plane)
    {
        PlaneIntersectionType result;
        Intersects(ref plane, out result);
        return result;
    }


    public void Intersects(ref Plane plane, out PlaneIntersectionType result)
    {
        // See http://zach.in.tu-clausthal.de/teaching/cg_literatur/lighthouse3d_view_frustum_culling/index.html

        Vector3 positiveVertex;
        Vector3 negativeVertex;

        if (plane.normal.X >= 0)
        {
            positiveVertex.X = max.X;
            negativeVertex.X = min.X;
        }
        else
        {
            positiveVertex.X = min.X;
            negativeVertex.X = max.X;
        }

        if (plane.normal.Y >= 0)
        {
            positiveVertex.Y = max.Y;
            negativeVertex.Y = min.Y;
        }
        else
        {
            positiveVertex.Y = min.Y;
            negativeVertex.Y = max.Y;
        }

        if (plane.normal.Z >= 0)
        {
            positiveVertex.Z = max.Z;
            negativeVertex.Z = min.Z;
        }
        else
        {
            positiveVertex.Z = min.Z;
            negativeVertex.Z = max.Z;
        }

        // Inline Vector3.Dot(plane.Normal, negativeVertex) + plane.D;
        var distance = plane.normal.X * negativeVertex.X + plane.normal.Y * negativeVertex.Y + plane.normal.Z * negativeVertex.Z + plane.distance;
        if (distance > 0)
        {
            result = PlaneIntersectionType.Front;
            return;
        }

        // Inline Vector3.Dot(plane.Normal, positiveVertex) + plane.D;
        distance = plane.normal.X * positiveVertex.X + plane.normal.Y * positiveVertex.Y + plane.normal.Z * positiveVertex.Z + plane.distance;
        if (distance < 0)
        {
            result = PlaneIntersectionType.Back;
            return;
        }

        result = PlaneIntersectionType.Intersecting;
    }


    public Nullable<double> Intersects(Ray ray) => ray.Intersects(this);


    public void Intersects(ref Ray ray, out Nullable<double> result)
    {
        result = Intersects(ray);
    }


    public static bool operator ==(Bounds a, Bounds b) => a.Equals(b);

    public static bool operator !=(Bounds a, Bounds b) => !a.Equals(b);

    internal string DebugDisplayString =>
        string.Concat(
            "Min( ", min.ToString(), " )  \r\n",
            "Max( ", max.ToString(), " )"
        );

    public override string ToString() => "{{Min:" + min.ToString() + " Max:" + max.ToString() + "}}";

    #endregion Public Methods
}