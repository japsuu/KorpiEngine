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

    public Vector3 Min;

    public Vector3 Max;

    public const int CORNER_COUNT = 8;

    #endregion Public Fields


    #region Public Properties

    public Vector3 Center
    {
        get => (Min + Max) * 0.5f;
        set
        {
            Vector3 s = Size * 0.5f;
            Min = value - s;
            Max = value + s;
        }
    }

    public Vector3 Extents
    {
        get => (Max - Min) * 0.5f;
        set
        {
            Vector3 c = Center;
            Min = c - value;
            Max = c + value;
        }
    }

    public Vector3 Size
    {
        get => Max - Min;
        set
        {
            Vector3 c = Center;
            Vector3 s = value * 0.5f;
            Min = c - s;
            Max = c + s;
        }
    }

    #endregion


    #region Public Constructors

    public Bounds(Vector3 center, Vector3 size)
    {
        Vector3 hs = size * 0.5f;
        Min = center - hs;
        Max = center + hs;
    }

    #endregion Public Constructors


    #region Public Methods

    public ContainmentType Contains(Bounds box)
    {
        //test if all corner is in the same side of a face by just checking min and max
        if (box.Max.X < Min.X
            || box.Min.X > Max.X
            || box.Max.Y < Min.Y
            || box.Min.Y > Max.Y
            || box.Max.Z < Min.Z
            || box.Min.Z > Max.Z)
            return ContainmentType.Disjoint;


        if (box.Min.X >= Min.X
            && box.Max.X <= Max.X
            && box.Min.Y >= Min.Y
            && box.Max.Y <= Max.Y
            && box.Min.Z >= Min.Z
            && box.Max.Z <= Max.Z)
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
        if (point.X < Min.X
            || point.X > Max.X
            || point.Y < Min.Y
            || point.Y > Max.Y
            || point.Z < Min.Z
            || point.Z > Max.Z)
            result = ContainmentType.Disjoint;

        //or if point is on box because coordinate of point is lesser or equal
        else if (point.X == Min.X
                 || point.X == Max.X
                 || point.Y == Min.Y
                 || point.Y == Max.Y
                 || point.Z == Min.Z
                 || point.Z == Max.Z)
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
        Vector3 minVec = MaxVector3;
        Vector3 maxVec = MinVector3;
        foreach (Vector3 ptVector in points)
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
        Min = Vector3.Min(Min, point);
        Max = Vector3.Max(Max, point);
    }


    public void Encapsulate(Bounds bounds)
    {
        Encapsulate(bounds.Center - bounds.Extents);
        Encapsulate(bounds.Center + bounds.Extents);
    }


    public void Expand(double amount)
    {
        Extents += new Vector3(amount, amount, amount) * .5;
    }


    public void Expand(Vector3 amount)
    {
        Extents += amount * .5;
    }


    public static Bounds CreateMerged(Bounds original, Bounds additional)
    {
        Bounds result;
        CreateMerged(ref original, ref additional, out result);
        return result;
    }


    public static void CreateMerged(ref Bounds original, ref Bounds additional, out Bounds result)
    {
        result.Min.X = Math.Min(original.Min.X, additional.Min.X);
        result.Min.Y = Math.Min(original.Min.Y, additional.Min.Y);
        result.Min.Z = Math.Min(original.Min.Z, additional.Min.Z);
        result.Max.X = Math.Max(original.Max.X, additional.Max.X);
        result.Max.Y = Math.Max(original.Max.Y, additional.Max.Y);
        result.Max.Z = Math.Max(original.Max.Z, additional.Max.Z);
    }


    public bool Equals(Bounds other) => Min == other.Min && Max == other.Max;

    public override bool Equals(object? obj) => obj is Bounds bounds ? Equals(bounds) : false;


    public Vector3[] GetCorners()
    {
        return new Vector3[]
        {
            new Vector3(Min.X, Max.Y, Max.Z),
            new Vector3(Max.X, Max.Y, Max.Z),
            new Vector3(Max.X, Min.Y, Max.Z),
            new Vector3(Min.X, Min.Y, Max.Z),
            new Vector3(Min.X, Max.Y, Min.Z),
            new Vector3(Max.X, Max.Y, Min.Z),
            new Vector3(Max.X, Min.Y, Min.Z),
            new Vector3(Min.X, Min.Y, Min.Z)
        };
    }


    public void GetCorners(Vector3[] corners)
    {
        if (corners == null)
            throw new ArgumentNullException("corners");
        if (corners.Length < 8)
            throw new ArgumentOutOfRangeException("corners", "Not Enought Corners");
        corners[0].X = Min.X;
        corners[0].Y = Max.Y;
        corners[0].Z = Max.Z;
        corners[1].X = Max.X;
        corners[1].Y = Max.Y;
        corners[1].Z = Max.Z;
        corners[2].X = Max.X;
        corners[2].Y = Min.Y;
        corners[2].Z = Max.Z;
        corners[3].X = Min.X;
        corners[3].Y = Min.Y;
        corners[3].Z = Max.Z;
        corners[4].X = Min.X;
        corners[4].Y = Max.Y;
        corners[4].Z = Min.Z;
        corners[5].X = Max.X;
        corners[5].Y = Max.Y;
        corners[5].Z = Min.Z;
        corners[6].X = Max.X;
        corners[6].Y = Min.Y;
        corners[6].Z = Min.Z;
        corners[7].X = Min.X;
        corners[7].Y = Min.Y;
        corners[7].Z = Min.Z;
    }


    public override int GetHashCode() => Min.GetHashCode() + Max.GetHashCode();


    public bool Intersects(Bounds box)
    {
        bool result;
        Intersects(ref box, out result);
        return result;
    }


    public void Intersects(ref Bounds box, out bool result)
    {
        if (Max.X >= box.Min.X && Min.X <= box.Max.X)
        {
            if (Max.Y < box.Min.Y || Min.Y > box.Max.Y)
            {
                result = false;
                return;
            }

            result = Max.Z >= box.Min.Z && Min.Z <= box.Max.Z;
            return;
        }

        result = false;
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

        if (plane.Normal.X >= 0)
        {
            positiveVertex.X = Max.X;
            negativeVertex.X = Min.X;
        }
        else
        {
            positiveVertex.X = Min.X;
            negativeVertex.X = Max.X;
        }

        if (plane.Normal.Y >= 0)
        {
            positiveVertex.Y = Max.Y;
            negativeVertex.Y = Min.Y;
        }
        else
        {
            positiveVertex.Y = Min.Y;
            negativeVertex.Y = Max.Y;
        }

        if (plane.Normal.Z >= 0)
        {
            positiveVertex.Z = Max.Z;
            negativeVertex.Z = Min.Z;
        }
        else
        {
            positiveVertex.Z = Min.Z;
            negativeVertex.Z = Max.Z;
        }

        // Inline Vector3.Dot(plane.Normal, negativeVertex) + plane.D;
        double distance = plane.Normal.X * negativeVertex.X + plane.Normal.Y * negativeVertex.Y + plane.Normal.Z * negativeVertex.Z + plane.Distance;
        if (distance > 0)
        {
            result = PlaneIntersectionType.Front;
            return;
        }

        // Inline Vector3.Dot(plane.Normal, positiveVertex) + plane.D;
        distance = plane.Normal.X * positiveVertex.X + plane.Normal.Y * positiveVertex.Y + plane.Normal.Z * positiveVertex.Z + plane.Distance;
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
            "Min( ", Min.ToString(), " )  \r\n",
            "Max( ", Max.ToString(), " )"
        );

    public override string ToString() => "{{Min:" + Min.ToString() + " Max:" + Max.ToString() + "}}";

    #endregion Public Methods
}