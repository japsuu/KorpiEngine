﻿// MIT License
// Copyright (C) 2024 KorpiEngine Team.
// Copyright (C) 2019 VIMaec LLC.
// Copyright (C) 2019 Ara 3D. Inc
// https://ara3d.com
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace KorpiEngine.Mathematics;

public partial struct Triangle2D
{
    public int Count => 3;

    public Vector2 this[int n] => n == 0 ? A : n == 1 ? B : C;

    // Compute the signed area of a triangle.
    public float Area => 0.5f * (A.X * (C.Y - B.Y) + B.X * (A.Y - C.Y) + C.X * (B.Y - A.Y));


    // Test if a given point p2 is on the left side of the line formed by p0-p1.
    public static bool OnLeftSideOfLine(Vector2 p0, Vector2 p1, Vector2 p2) => new Triangle2D(p0, p2, p1).Area > 0;


    // Test if a given point is inside a given triangle in R2.
    public bool Contains(Vector2 pp)
    {
        // Point in triangle test using barycentric coordinates
        Vector2 v0 = B - A;
        Vector2 v1 = C - A;
        Vector2 v2 = pp - A;

        float dot00 = v0.Dot(v0);
        float dot01 = v0.Dot(v1);
        float dot02 = v0.Dot(v2);
        float dot11 = v1.Dot(v1);
        float dot12 = v1.Dot(v2);

        float invDenom = 1f / (dot00 * dot11 - dot01 * dot01);
        dot11 = (dot11 * dot02 - dot01 * dot12) * invDenom;
        dot00 = (dot00 * dot12 - dot01 * dot02) * invDenom;

        return dot11 > 0 && dot00 > 0 && dot11 + dot00 < 1;
    }
}