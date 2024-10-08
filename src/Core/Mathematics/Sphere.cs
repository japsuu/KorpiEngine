﻿// MIT License
// Copyright (C) 2024 KorpiEngine Team.
// Copyright (C) 2019 VIMaec LLC.
// Copyright (C) 2019 Ara 3D. Inc
// https://ara3d.com
// Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace KorpiEngine.Mathematics;

/// <summary>
/// Describes a sphere in 3D-space for bounding operations.
/// </summary>
public partial struct Sphere : ITransformable3D<Sphere>
{
    /// <summary>
    /// Test if a bounding box is fully inside, outside, or just intersecting the sphere.
    /// </summary>
    public ContainmentType Contains(AABox box)
    {
        //check if all corners are in the sphere
        bool inside = true;
        foreach (Vector3 corner in box.Corners)
        {
            if (Contains(corner) == ContainmentType.Disjoint)
            {
                inside = false;
                break;
            }
        }

        if (inside)
            return ContainmentType.Contains;

        //check if the distance from sphere center to cube face < radius
        double dmin = 0;

        if (Center.X < box.Min.X)
            dmin += (Center.X - box.Min.X) * (Center.X - box.Min.X);

        else if (Center.X > box.Max.X)
            dmin += (Center.X - box.Max.X) * (Center.X - box.Max.X);

        if (Center.Y < box.Min.Y)
            dmin += (Center.Y - box.Min.Y) * (Center.Y - box.Min.Y);

        else if (Center.Y > box.Max.Y)
            dmin += (Center.Y - box.Max.Y) * (Center.Y - box.Max.Y);

        if (Center.Z < box.Min.Z)
            dmin += (Center.Z - box.Min.Z) * (Center.Z - box.Min.Z);

        else if (Center.Z > box.Max.Z)
            dmin += (Center.Z - box.Max.Z) * (Center.Z - box.Max.Z);

        if (dmin <= Radius * Radius)
            return ContainmentType.Intersects;

        //else disjoint
        return ContainmentType.Disjoint;
    }


    /// <summary>
    /// Test if a sphere is fully inside, outside, or just intersecting the sphere.
    /// </summary>
    public ContainmentType Contains(Sphere sphere)
    {
        float sqDistance = sphere.Center.DistanceSquared(Center);

        if (sqDistance > (sphere.Radius + Radius) * (sphere.Radius + Radius))
            return ContainmentType.Disjoint;

        if (sqDistance <= (Radius - sphere.Radius) * (Radius - sphere.Radius))
            return ContainmentType.Contains;

        return ContainmentType.Intersects;
    }


    /// <summary>
    /// Test if a point is fully inside, outside, or just intersecting the sphere.
    /// </summary>
    public ContainmentType Contains(Vector3 point)
    {
        Contains(point, out ContainmentType result);
        return result;
    }


    /// <summary>
    /// Test if a point is fully inside, outside, or just intersecting the sphere.
    /// </summary>
    public void Contains(Vector3 point, out ContainmentType result)
    {
        float sqRadius = Radius * Radius;
        float sqDistance = point.DistanceSquared(Center);

        if (sqDistance > sqRadius)
            result = ContainmentType.Disjoint;

        else if (sqDistance < sqRadius)
            result = ContainmentType.Contains;

        else
            result = ContainmentType.Intersects;
    }


    /// <summary>
    /// Creates the smallest sphere that contains the box. 
    /// </summary>
    public static Sphere Create(AABox box)
    {
        Vector3 center = box.Center;
        float radius = center.Distance(box.Max);
        return new Sphere(center, radius);
    }


    /// <summary>
    /// Creates the smallest Sphere that contains the given points 
    /// </summary>
    public static Sphere Create(IEnumerable<Vector3> points)
    {
        if (points == null)
            throw new ArgumentNullException(nameof(points));

        // From "Real-Time Collision Detection" (Page 89)

        Vector3 minx = new(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 maxx = -minx;
        Vector3 miny = minx;
        Vector3 maxy = -minx;
        Vector3 minz = minx;
        Vector3 maxz = -minx;

        // Find the most extreme points along the principle axis.
        int numPoints = 0;
        foreach (Vector3 pt in points)
        {
            ++numPoints;

            if (pt.X < minx.X)
                minx = pt;
            if (pt.X > maxx.X)
                maxx = pt;
            if (pt.Y < miny.Y)
                miny = pt;
            if (pt.Y > maxy.Y)
                maxy = pt;
            if (pt.Z < minz.Z)
                minz = pt;
            if (pt.Z > maxz.Z)
                maxz = pt;
        }

        if (numPoints == 0)
            throw new ArgumentException("You should have at least one point in points.");

        float sqDistX = maxx.DistanceSquared(minx);
        float sqDistY = maxy.DistanceSquared(miny);
        float sqDistZ = maxz.DistanceSquared(minz);

        // Pick the pair of most distant points.
        Vector3 min = minx;
        Vector3 max = maxx;
        if (sqDistY > sqDistX && sqDistY > sqDistZ)
        {
            max = maxy;
            min = miny;
        }

        if (sqDistZ > sqDistX && sqDistZ > sqDistY)
        {
            max = maxz;
            min = minz;
        }

        Vector3 center = (min + max) * 0.5f;
        float radius = max.Distance(center);

        // Test every point and expand the sphere.
        // The current bounding sphere is just a good approximation and may not enclose all points.            
        // From: Mathematics for 3D Game Programming and Computer Graphics, Eric Lengyel, Third Edition.
        // Page 218
        float sqRadius = radius * radius;
        foreach (Vector3 pt in points)
        {
            Vector3 diff = pt - center;
            float sqDist = diff.LengthSquared();
            if (sqDist > sqRadius)
            {
                float distance = (float)Math.Sqrt(sqDist); // equal to diff.Length()
                Vector3 direction = diff / distance;
                Vector3 G = center - radius * direction;
                center = (G + pt) / 2;
                radius = pt.Distance(center);
                sqRadius = radius * radius;
            }
        }

        return new Sphere(center, radius);
    }


    /// <summary>
    /// Creates a sphere from the given points
    /// </summary>
    public static Sphere Create(params Vector3[] points) => Create(points.AsEnumerable());


    /// <summary>
    /// Creates a sphere merging it with another 
    /// </summary>
    public Sphere Merge(Sphere additional)
    {
        Vector3 ocenterToaCenter = additional.Center - Center;
        float distance = ocenterToaCenter.Length();
        if (distance <= Radius + additional.Radius) //intersect
        {
            if (distance <= Radius - additional.Radius) //original contain additional
                return this;
            if (distance <= additional.Radius - Radius) //additional contain original
                return additional;
        }

        //else find center of a new sphere and radius
        float leftRadius = Math.Max(Radius - distance, additional.Radius);
        float Rightradius = Math.Max(Radius + distance, additional.Radius);
        ocenterToaCenter = ocenterToaCenter + (leftRadius - Rightradius) / (2 * ocenterToaCenter.Length()) * ocenterToaCenter;
        return new Sphere(Center + ocenterToaCenter, (leftRadius + Rightradius) / 2);
    }


    /// <summary>
    /// Gets whether a specified <see cref="AABox"/> intersects with this sphere.
    /// </summary>
    public bool Intersects(AABox box) => box.Intersects(this);


    /// <summary>
    /// Gets whether the other <see cref="Sphere"/> intersects with this sphere.
    /// </summary>
    public bool Intersects(Sphere sphere)
    {
        float sqDistance = sphere.Center.DistanceSquared(Center);
        return !(sqDistance > (sphere.Radius + Radius) * (sphere.Radius + Radius));
    }


    /// <summary>
    /// Gets whether a specified <see cref="Plane"/> intersects with this sphere.
    /// </summary>
    public PlaneIntersectionType Intersects(Plane plane)
    {
        float distance = Vector3.Dot(plane.Normal, Center);
        distance += plane.D;
        if (distance > Radius)
            return PlaneIntersectionType.Front;
        if (distance < -Radius)
            return PlaneIntersectionType.Back;
        return PlaneIntersectionType.Intersecting;
    }


    /// <summary>
    /// Gets whether a specified <see cref="Ray"/> intersects with this sphere.
    /// </summary>
    public float? Intersects(Ray ray) => ray.Intersects(this);


    public Sphere Transform(Matrix4x4 m) =>
        new(
            Center.Transform(m),
            Radius *
            (float)Math.Sqrt(
                Math.Max(
                    m.M11 * m.M11 + m.M12 * m.M12 + m.M13 * m.M13,
                    Math.Max(
                        m.M21 * m.M21 + m.M22 * m.M22 + m.M23 * m.M23,
                        m.M31 * m.M31 + m.M32 * m.M32 + m.M33 * m.M33))));


    public Sphere Translate(Vector3 offset) => new(Center + offset, Radius);

    public float Distance(Vector3 point) => (Center.Distance(point) - Radius).ClampLower(0);

    public float Distance(Sphere other) => (Center.Distance(other.Center) - Radius - other.Radius).ClampLower(0);
}