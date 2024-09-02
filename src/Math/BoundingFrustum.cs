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


using System.Text;

namespace KorpiEngine;

public sealed class BoundingFrustum : IEquatable<BoundingFrustum>
{
    #region Private Fields

    private Matrix4x4 _matrix;
    private readonly Vector3[] _corners = new Vector3[CORNER_COUNT];
    private readonly Plane[] _planes = new Plane[PLANE_COUNT];

    private const int PLANE_COUNT = 6;

    #endregion Private Fields


    #region Public Fields

    public const int CORNER_COUNT = 8;

    #endregion


    #region Public Constructors

    public BoundingFrustum(Matrix4x4 viewProjectionMatrix)
    {
        _matrix = viewProjectionMatrix;
        CreatePlanes();
        CreateCorners();
    }

    #endregion Public Constructors


    #region Public Properties

    public Matrix4x4 Matrix
    {
        get => _matrix;
        set
        {
            _matrix = value;
            CreatePlanes();
            CreateCorners();
        }
    }

    public Plane Near => _planes[0];

    public Plane Far => _planes[1];

    public Plane Left => _planes[2];

    public Plane Right => _planes[3];

    public Plane Top => _planes[4];

    public Plane Bottom => _planes[5];

    #endregion Public Properties


    #region Public Methods

    public static bool operator ==(BoundingFrustum? a, BoundingFrustum? b)
    {
        if (Equals(a, null))
            return Equals(b, null);

        if (Equals(b, null))
            return Equals(a, null);

        return a._matrix == b._matrix;
    }


    public static bool operator !=(BoundingFrustum? a, BoundingFrustum? b) => !(a == b);


    public ContainmentType Contains(AABox box)
    {
        Contains(ref box, out ContainmentType result);
        return result;
    }


    public void Contains(ref AABox box, out ContainmentType result)
    {
        bool intersects = false;
        for (int i = 0; i < PLANE_COUNT; ++i)
        {
            PlaneIntersectionType planeIntersectionType = box.Intersects(_planes[i]);
            switch (planeIntersectionType)
            {
                case PlaneIntersectionType.Front:
                    result = ContainmentType.Disjoint;
                    return;
                case PlaneIntersectionType.Intersecting:
                    intersects = true;
                    break;
            }
        }

        result = intersects ? ContainmentType.Intersects : ContainmentType.Contains;
    }


    public ContainmentType Contains(BoundingFrustum frustum)
    {
        if (this == frustum) // We check to see if the two frustums are equal
            return ContainmentType.Contains; // If they are, there's no need to go any further.

        bool intersects = false;
        for (int i = 0; i < PLANE_COUNT; ++i)
        {
            frustum.Intersects(ref _planes[i], out PlaneIntersectionType planeIntersectionType);
            switch (planeIntersectionType)
            {
                case PlaneIntersectionType.Front:
                    return ContainmentType.Disjoint;
                case PlaneIntersectionType.Intersecting:
                    intersects = true;
                    break;
            }
        }

        return intersects ? ContainmentType.Intersects : ContainmentType.Contains;
    }


    public ContainmentType Contains(Vector3 point)
    {
        Contains(ref point, out ContainmentType result);
        return result;
    }


    public void Contains(ref Vector3 point, out ContainmentType result)
    {
        for (int i = 0; i < PLANE_COUNT; ++i)
        {
            if (_planes[i].ClassifyPoint(point) < 0)
                continue;
            
            result = ContainmentType.Disjoint;
            return;
        }

        result = ContainmentType.Contains;
    }


    public bool Equals(BoundingFrustum? other) => this == other;


    public override bool Equals(object? obj)
    {
        return obj is BoundingFrustum f && this == f;
    }


    public Vector3[] GetCorners() => (Vector3[])_corners.Clone();


    public void GetCorners(Vector3[] corners)
    {
        ArgumentNullException.ThrowIfNull(corners);
        if (corners.Length < CORNER_COUNT)
            throw new ArgumentOutOfRangeException(nameof(corners));

        _corners.CopyTo(corners, 0);
    }


    public override int GetHashCode() => _matrix.GetHashCode();


    public bool Intersects(AABox box)
    {
        Intersects(ref box, out bool result);
        return result;
    }


    public void Intersects(ref AABox box, out bool result)
    {
        Contains(ref box, out ContainmentType containment);
        result = containment != ContainmentType.Disjoint;
    }


    public bool Intersects(BoundingFrustum frustum) => Contains(frustum) != ContainmentType.Disjoint;


    public PlaneIntersectionType Intersects(Plane plane)
    {
        Intersects(ref plane, out PlaneIntersectionType result);
        return result;
    }


    public void Intersects(ref Plane plane, out PlaneIntersectionType result)
    {
        bool inFront = false;
        bool behind = false;

        foreach (Vector3 corner in _corners)
        {
            float distance = plane.ClassifyPoint(corner);
            
            if (distance > 0)
                inFront = true;
            else if (distance < 0)
                behind = true;

            if (!inFront || !behind)
                continue;
            
            result = PlaneIntersectionType.Intersecting;
            return;
        }

        result = inFront ?
            PlaneIntersectionType.Front :
            PlaneIntersectionType.Back;
    }


    public override string ToString()
    {
        StringBuilder sb = new StringBuilder(256);
        sb.Append("{Near:");
        sb.Append(_planes[0].ToString());
        sb.Append(" Far:");
        sb.Append(_planes[1].ToString());
        sb.Append(" Left:");
        sb.Append(_planes[2].ToString());
        sb.Append(" Right:");
        sb.Append(_planes[3].ToString());
        sb.Append(" Top:");
        sb.Append(_planes[4].ToString());
        sb.Append(" Bottom:");
        sb.Append(_planes[5].ToString());
        sb.Append('}');
        return sb.ToString();
    }

    #endregion Public Methods


    #region Private Methods

    private void CreateCorners()
    {
        IntersectionPoint(ref _planes[0], ref _planes[2], ref _planes[4], out _corners[0]);
        IntersectionPoint(ref _planes[0], ref _planes[3], ref _planes[4], out _corners[1]);
        IntersectionPoint(ref _planes[0], ref _planes[3], ref _planes[5], out _corners[2]);
        IntersectionPoint(ref _planes[0], ref _planes[2], ref _planes[5], out _corners[3]);
        IntersectionPoint(ref _planes[1], ref _planes[2], ref _planes[4], out _corners[4]);
        IntersectionPoint(ref _planes[1], ref _planes[3], ref _planes[4], out _corners[5]);
        IntersectionPoint(ref _planes[1], ref _planes[3], ref _planes[5], out _corners[6]);
        IntersectionPoint(ref _planes[1], ref _planes[2], ref _planes[5], out _corners[7]);
    }


    private void CreatePlanes()
    {
        Plane plane0 = new Plane(-_matrix.M13, -_matrix.M23, -_matrix.M33, -_matrix.M43);
        Plane plane1 = new Plane(_matrix.M13 - _matrix.M14, _matrix.M23 - _matrix.M24, _matrix.M33 - _matrix.M34, _matrix.M43 - _matrix.M44);
        Plane plane2 = new Plane(-_matrix.M14 - _matrix.M11, -_matrix.M24 - _matrix.M21, -_matrix.M34 - _matrix.M31, -_matrix.M44 - _matrix.M41);
        Plane plane3 = new Plane(_matrix.M11 - _matrix.M14, _matrix.M21 - _matrix.M24, _matrix.M31 - _matrix.M34, _matrix.M41 - _matrix.M44);
        Plane plane4 = new Plane(_matrix.M12 - _matrix.M14, _matrix.M22 - _matrix.M24, _matrix.M32 - _matrix.M34, _matrix.M42 - _matrix.M44);
        Plane plane5 = new Plane(-_matrix.M14 - _matrix.M12, -_matrix.M24 - _matrix.M22, -_matrix.M34 - _matrix.M32, -_matrix.M44 - _matrix.M42);

        _planes[0] = NormalizePlane(ref plane0);
        _planes[1] = NormalizePlane(ref plane1);
        _planes[2] = NormalizePlane(ref plane2);
        _planes[3] = NormalizePlane(ref plane3);
        _planes[4] = NormalizePlane(ref plane4);
        _planes[5] = NormalizePlane(ref plane5);
    }


    private static void IntersectionPoint(ref Plane a, ref Plane b, ref Plane c, out Vector3 result)
    {
        // Formula used
        //                d1 ( N2 * N3 ) + d2 ( N3 * N1 ) + d3 ( N1 * N2 )
        //P =   -------------------------------------------------------------------------
        //                             N1 . ( N2 * N3 )
        //
        // Note: N refers to the normal, d refers to the displacement. '.' means dot product. '*' means cross-product
        
        Vector3 cross = MathOps.Cross(b.Normal, c.Normal);

        double f = Vector3.Dot(a.Normal, cross);
        f *= -1.0f;

        cross = MathOps.Cross(b.Normal, c.Normal);
        Vector3 v1 = cross * a.D;
        
        cross = MathOps.Cross(c.Normal, a.Normal);
        Vector3 v2 = cross * b.D;
        
        cross = MathOps.Cross(a.Normal, b.Normal);
        Vector3 v3 = cross * c.D;

        double x = (v1.X + v2.X + v3.X) / f;
        double y = (v1.Y + v2.Y + v3.Y) / f;
        double z = (v1.Z + v2.Z + v3.Z) / f;
        
        result = new Vector3((float)x, (float)y, (float)z);
    }


    private static Plane NormalizePlane(ref Plane p)
    {
        double factor = 1 / p.Normal.Magnitude();
        double normalX = p.Normal.X * factor;
        double normalY = p.Normal.Y * factor;
        double normalZ = p.Normal.Z * factor;
        double distance = p.D * factor;
        
        return new Plane((float)normalX, (float)normalY, (float)normalZ, (float)distance);
    }

    #endregion
}