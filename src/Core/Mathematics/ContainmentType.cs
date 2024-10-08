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
/// Defines how the bounding volumes intersect or contain one another.
/// </summary>
public enum ContainmentType
{
    /// <summary>
    /// Indicates that there is no overlap between two bounding volumes.
    /// </summary>
    Disjoint,

    /// <summary>
    /// Indicates that one bounding volume completely contains another volume.
    /// </summary>
    Contains,

    /// <summary>
    /// Indicates that bounding volumes partially overlap one another.
    /// </summary>
    Intersects
}