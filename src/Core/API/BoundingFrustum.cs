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

namespace KorpiEngine.Core.API;

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

    public BoundingFrustum(Matrix4x4 value)
    {
        _matrix = value;
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


    public ContainmentType Contains(Bounds box)
    {
        Contains(ref box, out ContainmentType result);
        return result;
    }


    public void Contains(ref Bounds box, out ContainmentType result)
    {
        bool intersects = false;
        for (int i = 0; i < PLANE_COUNT; ++i)
        {
            box.Intersects(ref _planes[i], out PlaneIntersectionType planeIntersectionType);
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
            if (!_planes[i].GetSide(point))
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


    public bool Intersects(Bounds box)
    {
        Intersects(ref box, out bool result);
        return result;
    }


    public void Intersects(ref Bounds box, out bool result)
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
        result = plane.Intersects(ref _corners[0]);
        for (int i = 1; i < _corners.Length; i++)
            if (plane.Intersects(ref _corners[i]) != result)
                result = PlaneIntersectionType.Intersecting;
    }
    

    internal string DebugDisplayString =>
        string.Concat(
            "Near( ", _planes[0].DebugDisplayString, " )  \r\n",
            "Far( ", _planes[1].DebugDisplayString, " )  \r\n",
            "Left( ", _planes[2].DebugDisplayString, " )  \r\n",
            "Right( ", _planes[3].DebugDisplayString, " )  \r\n",
            "Top( ", _planes[4].DebugDisplayString, " )  \r\n",
            "Bottom( ", _planes[5].DebugDisplayString, " )  "
        );


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
        sb.Append("}");
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
        _planes[0] = new Plane(-_matrix.M13, -_matrix.M23, -_matrix.M33, -_matrix.M43);
        _planes[1] = new Plane(_matrix.M13 - _matrix.M14, _matrix.M23 - _matrix.M24, _matrix.M33 - _matrix.M34, _matrix.M43 - _matrix.M44);
        _planes[2] = new Plane(-_matrix.M14 - _matrix.M11, -_matrix.M24 - _matrix.M21, -_matrix.M34 - _matrix.M31, -_matrix.M44 - _matrix.M41);
        _planes[3] = new Plane(_matrix.M11 - _matrix.M14, _matrix.M21 - _matrix.M24, _matrix.M31 - _matrix.M34, _matrix.M41 - _matrix.M44);
        _planes[4] = new Plane(_matrix.M12 - _matrix.M14, _matrix.M22 - _matrix.M24, _matrix.M32 - _matrix.M34, _matrix.M42 - _matrix.M44);
        _planes[5] = new Plane(-_matrix.M14 - _matrix.M12, -_matrix.M24 - _matrix.M22, -_matrix.M34 - _matrix.M32, -_matrix.M44 - _matrix.M42);

        NormalizePlane(ref _planes[0]);
        NormalizePlane(ref _planes[1]);
        NormalizePlane(ref _planes[2]);
        NormalizePlane(ref _planes[3]);
        NormalizePlane(ref _planes[4]);
        NormalizePlane(ref _planes[5]);
    }


    private static void IntersectionPoint(ref Plane a, ref Plane b, ref Plane c, out Vector3 result)
    {
        // Formula used
        //                d1 ( N2 * N3 ) + d2 ( N3 * N1 ) + d3 ( N1 * N2 )
        //P =   -------------------------------------------------------------------------
        //                             N1 . ( N2 * N3 )
        //
        // Note: N refers to the normal, d refers to the displacement. '.' means dot product. '*' means cross-product

        Vector3 cross = Vector3.Cross(b.Normal, c.Normal);

        double f = Vector3.Dot(a.Normal, cross);
        f *= -1.0f;

        cross = Vector3.Cross(b.Normal, c.Normal);
        Vector3 v1 = cross * a.Distance;
        
        cross = Vector3.Cross(c.Normal, a.Normal);
        Vector3 v2 = cross * b.Distance;
        
        cross = Vector3.Cross(a.Normal, b.Normal);
        Vector3 v3 = cross * c.Distance;

        result.X = (v1.X + v2.X + v3.X) / f;
        result.Y = (v1.Y + v2.Y + v3.Y) / f;
        result.Z = (v1.Z + v2.Z + v3.Z) / f;
    }


    private static void NormalizePlane(ref Plane p)
    {
        double factor = 1 / p.Normal.Magnitude;
        p.Normal.X *= factor;
        p.Normal.Y *= factor;
        p.Normal.Z *= factor;
        p.Distance *= factor;
    }

    #endregion
}