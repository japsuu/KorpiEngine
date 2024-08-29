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


namespace KorpiEngine;

public struct Ray : IEquatable<Ray>
{
    #region Public Fields

    public Vector3 Direction;

    public Vector3 Origin;

    #endregion


    #region Public Constructors

    public Ray(Vector3 position, Vector3 direction)
    {
        Origin = position;
        Direction = direction;
    }

    #endregion


    #region Public Methods

    public override bool Equals(object? obj) => obj is Ray ray && Equals(ray);

    public bool Equals(Ray other) => Origin.Equals(other.Origin) && Direction.Equals(other.Direction);

    public override int GetHashCode() => Origin.GetHashCode() ^ Direction.GetHashCode();


    // adapted from http://www.scratchapixel.com/lessons/3d-basic-lessons/lesson-7-intersecting-simple-shapes/ray-box-intersection/
    public double? Intersects(Bounds box)
    {
        const double epsilon = 1e-6;

        double? tMin = null,
            tMax = null;

        if (Math.Abs(Direction.X) < epsilon)
        {
            if (Origin.X < box.Min.X || Origin.X > box.Max.X)
                return null;
        }
        else
        {
            tMin = (box.Min.X - Origin.X) / Direction.X;
            tMax = (box.Max.X - Origin.X) / Direction.X;

            if (tMin > tMax)
            {
                (tMin, tMax) = (tMax, tMin);
            }
        }

        if (Math.Abs(Direction.Y) < epsilon)
        {
            if (Origin.Y < box.Min.Y || Origin.Y > box.Max.Y)
                return null;
        }
        else
        {
            double tMinY = (box.Min.Y - Origin.Y) / Direction.Y;
            double tMaxY = (box.Max.Y - Origin.Y) / Direction.Y;

            if (tMinY > tMaxY)
            {
                (tMinY, tMaxY) = (tMaxY, tMinY);
            }

            if ((tMin.HasValue && tMin > tMaxY) || (tMax.HasValue && tMinY > tMax))
                return null;

            if (!tMin.HasValue || tMinY > tMin)
                tMin = tMinY;
            if (!tMax.HasValue || tMaxY < tMax)
                tMax = tMaxY;
        }

        if (Math.Abs(Direction.Z) < epsilon)
        {
            if (Origin.Z < box.Min.Z || Origin.Z > box.Max.Z)
                return null;
        }
        else
        {
            double tMinZ = (box.Min.Z - Origin.Z) / Direction.Z;
            double tMaxZ = (box.Max.Z - Origin.Z) / Direction.Z;

            if (tMinZ > tMaxZ)
            {
                (tMinZ, tMaxZ) = (tMaxZ, tMinZ);
            }

            if ((tMin.HasValue && tMin > tMaxZ) || (tMax.HasValue && tMinZ > tMax))
                return null;

            if (!tMin.HasValue || tMinZ > tMin)
                tMin = tMinZ;
            if (!tMax.HasValue || tMaxZ < tMax)
                tMax = tMaxZ;
        }

        // having a positive tMin and a negative tMax means the ray is inside the box
        // we expect the intesection distance to be 0 in that case
        if (tMin.HasValue && tMin < 0 && tMax > 0)
            return 0;

        // a negative tMin means that the intersection point is behind the ray's origin
        // we discard these as not hitting the AABB
        if (tMin < 0)
            return null;

        return tMin;
    }


    public void Intersects(ref Bounds box, out double? result)
    {
        result = Intersects(box);
    }


    public double? Intersects(Plane plane)
    {
        double? result;
        Intersects(ref plane, out result);
        return result;
    }


    public void Intersects(ref Plane plane, out double? result)
    {
        double den = Vector3.Dot(Direction, plane.Normal);
        if (Math.Abs(den) < 0.00001)
        {
            result = null;
            return;
        }

        result = (-plane.Distance - Vector3.Dot(plane.Normal, Origin)) / den;

        if (result < 0.0)
        {
            if (result < -0.00001)
            {
                result = null;
                return;
            }

            result = 0.0;
        }
    }


    public static bool operator !=(Ray a, Ray b) => !a.Equals(b);

    public static bool operator ==(Ray a, Ray b) => a.Equals(b);

    public override string ToString() => string.Format("{{Position:{0} Direction:{1}}}", Origin.ToString(), Direction.ToString());

    #endregion
}