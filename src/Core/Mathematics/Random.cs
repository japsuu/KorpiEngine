// MIT License
// Copyright (C) 2019 VIMaec LLC.
// Copyright (C) 2019 Ara 3D. Inc
// https://ara3d.com

namespace KorpiEngine.Mathematics;

public static class StatelessRandom
{
    public static uint RandomUInt(int index, int seed = 0)
        => (uint)Hash.Combine(seed, index);

    public static int RandomInt(int index, int seed = 0)
        => Hash.Combine(seed, index);

    public static float RandomFloat(float min, float max, int index, int seed)
        => (float)RandomUInt(index, seed) / uint.MaxValue * (max - min) + min;

    public static float RandomFloat(int index, int seed = 0)
        => RandomFloat(0, 1, index, seed);

    public static Vector2 RandomVector2(int index, int seed = 0)
        => new Vector2(
            RandomFloat(index * 2, seed),
            RandomFloat(index * 2 + 1, seed));

    public static Vector3 RandomVector3(int index, int seed = 0)
        => new Vector3(
            RandomFloat(index * 3, seed),
            RandomFloat(index * 3 + 1, seed),
            RandomFloat(index * 3 + 2, seed));

    public static Vector4 RandomVector4(int index, int seed = 0)
        => new Vector4(
            RandomFloat(index * 4, seed),
            RandomFloat(index * 4 + 1, seed),
            RandomFloat(index * 4 + 2, seed),
            RandomFloat(index * 4 + 3, seed));
}

/// <summary> A Utility class with a bunch of Random types/values - Based on System.Random.Shared </summary>
public static class SharedRandom
{
    /// <summary> Returns a random value between 0 and 1 </summary>
    public static double Value => Random.Shared.NextDouble();
    
    /// <summary> Returns a random value between 0 and 1 </summary>
    public static float ValueFloat => Random.Shared.NextSingle();

    /// <summary> Randomly returns either -1 or 1 </summary>
    public static double Sign => Value > 0.5f ? 1f : -1f;


    /// <summary> Randomly returns a value between min and max </summary>
    /// <param name="min"> The minimum value [inclusive] </param>
    /// <param name="max"> The maximum value [inclusive] </param>
    public static double Range(double min, double max) => min + Value * (max - min);


    /// <summary> Randomly returns a value between min and max</summary>
    /// <param name="min"> The minimum value [inclusive] </param>
    /// <param name="max"> The maximum value [exclusive] </param>
    public static int Range(int min, int max) => Random.Shared.Next(min, max);


    /// <summary> Returns a random point on the unit circle </summary>
    public static Vector2 OnUnitCircle
    {
        get
        {
            double angle = Value * 2 * Math.PI;
            return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)).Normalize();
        }
    }

    /// <summary> Returns a random point inside the unit circle </summary>
    public static Vector2 InUnitCircle => OnUnitCircle * ValueFloat;

    /// <summary> Returns a random point inside the unit square [0-1] </summary>
    public static Vector2 InUnitSquare => new(ValueFloat, ValueFloat);

    /// <summary> Returns a random point on the unit sphere </summary>
    public static Vector3 OnUnitSphere
    {
        get
        {
            double u = Value * 2 - 1;
            double theta = Value * 2 * Math.PI;
            double sqrtOneMinusUSquared = Math.Sqrt(1 - u * u);

            return new Vector3(
                (float)(sqrtOneMinusUSquared * Math.Cos(theta)),
                (float)(sqrtOneMinusUSquared * Math.Sin(theta)),
                (float)u
            ).Normalize();
        }
    }

    /// <summary> Returns a random point inside the unit sphere </summary>
    public static Vector3 InUnitSphere => OnUnitSphere * MathF.Sqrt(ValueFloat);

    /// <summary> Returns a random point inside the unit cube [0-1] </summary>
    public static Vector3 InUnitCube => new(ValueFloat, ValueFloat, MathF.Cbrt(ValueFloat));

    /// <summary> Returns a random angle in radians from 0 to TAU </summary>
    public static double Angle => Value * Math.Tau;

    /// <summary> Returns a random uniformly distributed rotation </summary>
    public static Quaternion Rotation => new(OnUnitSphere, (float)(Value * Math.Tau));

    /// <summary> Returns a random Boolean value </summary>
    public static bool Boolean => Value > 0.5;

    /// <summary> Returns a random uniformly distributed color </summary>
    public static ColorRGBA Color
    {
        get
        {
            unchecked
            {
                uint val = (uint)Random.Shared.Next();
                return new ColorRGBA((byte)(val & 255), (byte)((val >> 8) & 255), (byte)((val >> 16) & 255), (byte)(Random.Shared.Next() & 255));
            }
        }
    }

    /// <summary> Returns a random uniformly distributed HDR color </summary>
    public static ColorHDR ColorHDR
    {
        get
        {
            ColorRGBA color = Color;
            return new ColorHDR(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        }
    }

    /// <summary> Returns a random uniformly distributed color with an alpha of 1.0 </summary>
    public static ColorRGBA ColorFullAlpha
    {
        get
        {
            unchecked
            {
                uint val = (uint)Random.Shared.Next();
                return new ColorRGBA((byte)(val & 255), (byte)((val >> 8) & 255), (byte)((val >> 16) & 255), 255);
            }
        }
    }
    
    /// <summary> Returns a random uniformly distributed HDR color with an alpha of 1.0 </summary>
    public static ColorHDR ColorHDRFullAlpha
    {
        get
        {
            ColorRGBA color = ColorFullAlpha;
            return new ColorHDR(color.R / 255f, color.G / 255f, color.B / 255f, 1f);
        }
    }
}