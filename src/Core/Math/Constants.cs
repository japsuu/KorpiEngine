// MIT License
// Copyright (C) 2024 KorpiEngine Team.
// Copyright (C) 2019 VIMaec LLC.
// Copyright (C) 2019 Ara 3D. Inc
// https://ara3d.com
// Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace KorpiEngine;

public static class Constants
{
    public static readonly Plane XYPlane = new(Vector3.UnitZ, 0);
    public static readonly Plane XZPlane = new(Vector3.UnitY, 0);
    public static readonly Plane YZPlane = new(Vector3.UnitX, 0);

    public const float PI = (float)Math.PI;
    public const float HALF_PI = PI / 2f;
    public const float TWO_PI = PI * 2f;
    public const float TOLERANCE = 0.0000001f;
    public const float LOG10_E = 0.4342945f;
    public const float LOG2_E = 1.442695f;
    public const float E = (float)Math.E;

    public const double RADIANS_TO_DEGREES = 57.295779513082320876798154814105;
    public const double DEGREES_TO_RADIANS = 0.017453292519943295769236907684886;

    public const double ONE_TENTH_OF_A_DEGREE = DEGREES_TO_RADIANS / 10;

    public const double MM_TO_FEET = 0.00328084;
    public const double FEET_TO_MM = 1 / MM_TO_FEET;
}