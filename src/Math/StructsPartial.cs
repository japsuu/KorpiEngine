// MIT License
// Copyright (C) 2024 KorpiEngine Team.
// Copyright (C) 2019 VIMaec LLC.
// Copyright (C) 2019 Ara 3D. Inc
// https://ara3d.com

using System.Runtime.CompilerServices;

namespace KorpiEngine;

public partial struct ColorHDR
{
    public static readonly ColorHDR White = new(1f, 1f, 1f, 1f);
    public static readonly ColorHDR Black = new(0f, 0f, 0f, 1f);
    public static readonly ColorHDR Gray = new(0.5f, 0.5f, 0.5f, 1f);
    public static readonly ColorHDR Transparent = new(0f, 0f, 0f, 0f);

    public static readonly ColorHDR Red = new(1f, 0f, 0f, 1f);
    public static readonly ColorHDR Green = new(0f, 1f, 0f, 1f);
    public static readonly ColorHDR Blue = new(0f, 0f, 1f, 1f);

    public static readonly ColorHDR Yellow = new(1f, 1f, 0f, 1f);
    public static readonly ColorHDR Cyan = new(0f, 1f, 1f, 1f);
    public static readonly ColorHDR Magenta = new(1f, 0f, 1f, 1f);

    public static readonly ColorHDR LightRed = new(1f, 0.5f, 0.5f, 1f);
    public static readonly ColorHDR LightGreen = new(0.5f, 1f, 0.5f, 1f);
    public static readonly ColorHDR LightBlue = new(0.5f, 0.5f, 1f, 1f);
}

public partial struct ColorRGB
{
    public static readonly ColorRGB White = new(255, 255, 255);
    public static readonly ColorRGB Black = new(0, 0, 0);
    public static readonly ColorRGB Gray = new(128, 128, 128);

    public static readonly ColorRGB Red = new(255, 0, 0);
    public static readonly ColorRGB Green = new(0, 255, 0);
    public static readonly ColorRGB Blue = new(0, 0, 255);

    public static readonly ColorRGB Yellow = new(255, 255, 0);
    public static readonly ColorRGB Cyan = new(0, 255, 255);
    public static readonly ColorRGB Magenta = new(255, 0, 255);

    public static readonly ColorRGB LightRed = new(255, 128, 128);
    public static readonly ColorRGB LightGreen = new(128, 255, 128);
    public static readonly ColorRGB LightBlue = new(128, 128, 255);


    public void DeconstructFloat(out float r, out float g, out float b)
    {
        r = R / 255f;
        g = G / 255f;
        b = B / 255f;
    }
}

public partial struct ColorRGBA
{
    public static readonly ColorRGBA White = new(255, 255, 255, 255);
    public static readonly ColorRGBA Black = new(0, 0, 0, 255);
    public static readonly ColorRGBA Gray = new(128, 128, 128, 255);
    public static readonly ColorRGBA Transparent = new(0, 0, 0, 0);

    public static readonly ColorRGBA Red = new(255, 0, 0, 255);
    public static readonly ColorRGBA Green = new(0, 255, 0, 255);
    public static readonly ColorRGBA Blue = new(0, 0, 255, 255);

    public static readonly ColorRGBA Yellow = new(255, 255, 0, 255);
    public static readonly ColorRGBA Cyan = new(0, 255, 255, 255);
    public static readonly ColorRGBA Magenta = new(255, 0, 255, 255);

    public static readonly ColorRGBA LightRed = new(255, 128, 128, 255);
    public static readonly ColorRGBA LightGreen = new(128, 255, 128, 255);
    public static readonly ColorRGBA LightBlue = new(128, 128, 255, 255);


    public void DeconstructFloat(out float r, out float g, out float b, out float a)
    {
        r = R / 255f;
        g = G / 255f;
        b = B / 255f;
        a = A / 255f;
    }
}

public partial struct Vector4 : ITransformable3D<Vector4>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector4(Vector3 v, float w)
        : this(v.X, v.Y, v.Z, w)
    {
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector4(Vector2 v, float z, float w)
        : this(v.X, v.Y, z, w)
    {
    }


    /// <summary>
    /// Transforms a vector by the given matrix.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector4 Transform(Matrix4x4 matrix) =>
        new(
            X * matrix.M11 + Y * matrix.M21 + Z * matrix.M31 + W * matrix.M41,
            X * matrix.M12 + Y * matrix.M22 + Z * matrix.M32 + W * matrix.M42,
            X * matrix.M13 + Y * matrix.M23 + Z * matrix.M33 + W * matrix.M43,
            X * matrix.M14 + Y * matrix.M24 + Z * matrix.M34 + W * matrix.M44);


    public Vector3 XYZ => new(X, Y, Z);
    public Vector2 XY => new(X, Y);
}

public partial struct Vector3 : ITransformable3D<Vector3>
{
    public static readonly Vector3 Right = new(1.0f, 0.0f, 0.0f);
    public static readonly Vector3 Left = new(-1.0f, 0.0f, 0.0f);
    public static readonly Vector3 Up = new(0.0f, 1.0f, 0.0f);
    public static readonly Vector3 Down = new(0.0f, -1.0f, 0.0f);
    public static readonly Vector3 Forward = new(0.0f, 0.0f, -1.0f);
    public static readonly Vector3 Backward = new(0.0f, 0.0f, 1.0f);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3(float x, float y)
        : this(x, y, 0)
    {
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3(Vector2 xy, float z)
        : this(xy.X, xy.Y, z)
    {
    }


    /// <summary>
    /// Transforms a vector by the given matrix.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 Transform(Matrix4x4 mat) =>
        new(
            X * mat.M11 + Y * mat.M21 + Z * mat.M31 + mat.M41,
            X * mat.M12 + Y * mat.M22 + Z * mat.M32 + mat.M42,
            X * mat.M13 + Y * mat.M23 + Z * mat.M33 + mat.M43);


    /// <summary>
    /// Computes the cross product of two vectors.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 Cross(Vector3 vector2) =>
        new(
            Y * vector2.Z - Z * vector2.Y,
            Z * vector2.X - X * vector2.Z,
            X * vector2.Y - Y * vector2.X);


    /// <summary>
    /// Returns the mixed product
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double MixedProduct(Vector3 v1, Vector3 v2) => Cross(v1).Dot(v2);


    /// <summary>
    /// Returns the reflection of a vector off a surface that has the specified normal.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 Reflect(Vector3 normal) => this - normal * Dot(normal) * 2f;


    /// <summary>
    /// Transforms a vector normal by the given matrix.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 TransformNormal(Matrix4x4 matrix) =>
        new(
            X * matrix.M11 + Y * matrix.M21 + Z * matrix.M31,
            X * matrix.M12 + Y * matrix.M22 + Z * matrix.M32,
            X * matrix.M13 + Y * matrix.M23 + Z * matrix.M33);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 Clamp(AABox box) => this.Clamp(box.Min, box.Max);


    public Vector2 XY => new(X, Y);
    public Vector2 XZ => new(X, Z);
    public Vector2 YZ => new(Y, Z);
    public Vector3 XZY => new(X, Z, Y);
    public Vector3 ZXY => new(Z, X, Y);
    public Vector3 ZYX => new(Z, Y, Z);
    public Vector3 YXZ => new(Y, X, Z);
    public Vector3 YZX => new(Y, Z, X);
}

public partial struct Line : ITransformable3D<Line>, IPoints, IMappable<Line, Vector3>
{
    public Vector3 Vector => B - A;
    public Ray Ray => new(A, Vector);
    public float Length => A.Distance(B);
    public float LengthSquared => A.DistanceSquared(B);
    public Vector3 MidPoint => A.Average(B);
    public Line Normal => new(A, A + Vector.Normalize());
    public Line Inverse => new(B, A);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 Lerp(float amount) => A.Lerp(B, amount);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Line SetLength(float length) => new(A, A + Vector.Along(length));


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Line Transform(Matrix4x4 mat) => new(A.Transform(mat), B.Transform(mat));


    public int NumPoints => 2;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 GetPoint(int n) => n == 0 ? A : B;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Line Map(Func<Vector3, Vector3> f) => new(f(A), f(B));
}

public partial struct Int2
{
    public Vector2 ToVector2() => new(X, Y);

    public static implicit operator Vector2(Int2 self) => self.ToVector2();
}

public partial struct Int3
{
    public Vector3 ToVector3() => new(X, Y, Z);

    public static implicit operator Vector3(Int3 self) => self.ToVector3();
}

public partial struct Vector2
{
    public Vector3 ToVector3() => new(X, Y, 0);

    public static implicit operator Vector3(Vector2 self) => self.ToVector3();


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double PointCrossProduct(Vector2 other) => X * other.Y - other.X * Y;


    /// <summary>
    /// Computes the cross product of two vectors.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Cross(Vector2 v2) => X * v2.Y - Y * v2.X;
}

public partial struct Line2D
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AABox2D BoundingBox() => AABox2D.Create(A.Min(B), A.Max(B));


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double LinePointCrossProduct(Vector2 point)
    {
        Line2D tmpLine = new(Vector2.Zero, B - A);
        Vector2 tmpPoint = point - A;
        return tmpLine.B.PointCrossProduct(tmpPoint);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsPointOnLine(Vector2 point) => Math.Abs(LinePointCrossProduct(point)) < Constants.TOLERANCE;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsPointRightOfLine(Vector2 point) => LinePointCrossProduct(point) < 0;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TouchesOrCrosses(Line2D other) => IsPointOnLine(other.A) || IsPointOnLine(other.B) || IsPointRightOfLine(other.A) ^ IsPointRightOfLine(other.B);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Intersects(AABox2D thisBox, Line2D otherLine, AABox2D otherBox) =>
        thisBox.Intersects(otherBox) && TouchesOrCrosses(otherLine) && otherLine.TouchesOrCrosses(this);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Intersects(Line2D other) =>

        // Inspired by: https://martin-thoma.com/how-to-check-if-two-line-segments-intersect/
        Intersects(BoundingBox(), other, other.BoundingBox());
}

public partial struct DVector2
{
    public Vector2 Vector2 => new((float)X, (float)Y);
}

public partial struct DVector3
{
    public Vector3 Vector3 => new((float)X, (float)Y, (float)Z);


    /// <summary>
    /// Computes the cross product of two vectors.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DVector3 Cross(DVector3 vector2) =>
        new(
            Y * vector2.Z - Z * vector2.Y,
            Z * vector2.X - X * vector2.Z,
            X * vector2.Y - Y * vector2.X);
}

public partial struct DVector4
{
    public Vector4 Vector4 => new((float)X, (float)Y, (float)Z, (float)W);
}

public partial struct DAABox
{
    public AABox AABox => new(Min.Vector3, Max.Vector3);
}

public partial struct DQuaternion
{
    public Quaternion Quaternion => new((float)X, (float)Y, (float)Z, (float)W);

    public DVector4 DVector4 => new(X, Y, Z, W);
}

public partial struct TransformData
{
    public static TransformData Identity => new(Vector3.Zero, Quaternion.Identity, Vector3.One);
}

public partial struct HorizontalCoordinate
{
    public static implicit operator DVector2(HorizontalCoordinate angle) => new(angle.Azimuth, angle.Inclination);

    public static explicit operator Vector2(HorizontalCoordinate angle) => new((float)angle.Azimuth, (float)angle.Inclination);

    public static implicit operator HorizontalCoordinate(DVector2 vector) => new(vector.X, vector.Y);

    public static implicit operator HorizontalCoordinate(Vector2 vector) => new(vector.X, vector.Y);
}

/*
public static class MovementExtensions
{
    public static Vector3 ComputeFrictionVector(this LinearMotion motion)
    {
        var f = motion.Velocity.Normalize() * motion.Friction;
            if (f.LengthSquared() > f.)
        
        public static LinearMotion Update(this LinearMotion self, float amount)
            => self.SetVelocity(self.Velocity + self.Acceleration * amount - self.Velocity * self.Friction * amount);
        
        public static AngularMotion Update(this AngularMotion self, float amount)
            => self.SetVelocity(self.Velocity + self.Acceleration * amount - self.Velocity.Normalize() * self.Friction * amount);
        
        public static Motion Update(this Motion self, float amount)
            => self.SetLinear(self.Linear.Update(amount)).SetAngular(self.Angular.Update(amount));
    }
}
*/