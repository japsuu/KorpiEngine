#region LICENCE

/*
 Adapted from the Prowl Engine by Michael Sakharov:
 
MIT License

Copyright (c) 2023 Michael Sakharov

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

#endregion

namespace KorpiEngine;

/// <summary> A Utility class with a bunch of Random types/values - Based on System.Random.Shared </summary>
public static class Random
{
    /// <summary> Returns a random value between 0 and 1 </summary>
    public static double Value => System.Random.Shared.NextDouble();

    /// <summary> Randomly returns either -1 or 1 </summary>
    public static double Sign => Value > 0.5f ? 1f : -1f;


    /// <summary> Randomly returns a value between min and max </summary>
    /// <param name="min"> The minimum value [inclusive] </param>
    /// <param name="max"> The maximum value [inclusive] </param>
    public static double Range(double min, double max) => min + Value * (max - min);


    /// <summary> Randomly returns a value between min and max</summary>
    /// <param name="min"> The minimum value [inclusive] </param>
    /// <param name="max"> The maximum value [exclusive] </param>
    public static int Range(int min, int max) => System.Random.Shared.Next(min, max);


    /// <summary> Returns a random point on the unit circle </summary>
    public static Vector2 OnUnitCircle
    {
        get
        {
            double angle = Value * 6.283185307179586476925286766559;
            return Vector2.Normalize(new Vector2(Math.Cos(angle), Math.Sin(angle)));
        }
    }

    /// <summary> Returns a random point inside the unit circle </summary>
    public static Vector2 InUnitCircle => OnUnitCircle * Value;

    /// <summary> Returns a random point inside the unit square [0-1] </summary>
    public static Vector2 InUnitSquare => new(Value, Value);

    /// <summary> Returns a random point on the unit sphere </summary>
    public static Vector3 OnUnitSphere
    {
        get
        {
            double a = Value * 6.283185307179586476925286766559;
            double b = Value * Math.PI;
            double sinB = Math.Sin(b);
            return Vector3.Normalize(new Vector3(sinB * Math.Cos(a), sinB * Math.Sin(a), Math.Cos(b)));
        }
    }

    /// <summary> Returns a random point inside the unit sphere </summary>
    public static Vector3 InUnitSphere => OnUnitSphere * Value;

    /// <summary> Returns a random point inside the unit cube [0-1] </summary>
    public static Vector3 InUnitCube => new(Value, Value, Value);

    /// <summary> Returns a random angle in radians from 0 to TAU </summary>
    public static double Angle => Value * Mathd.TAU;

    /// <summary> Returns a random uniformly distributed rotation </summary>
    public static Quaternion Rotation => new(OnUnitSphere, Value * Mathd.TAU);

    /// <summary> Returns a random Boolean value </summary>
    public static bool Boolean => Value > 0.5f;

    /// <summary> Returns a random uniformly distributed color </summary>
    public static Color Color
    {
        get
        {
            unchecked
            {
                uint val = (uint)System.Random.Shared.Next();
                return new Color32((byte)(val & 255), (byte)((val >> 8) & 255), (byte)((val >> 16) & 255), (byte)(System.Random.Shared.Next() & 255));
            }
        }
    }

    /// <summary> Returns a random uniformly distributed color with an alpha of 1.0 </summary>
    public static Color ColorFullAlpha
    {
        get
        {
            unchecked
            {
                uint val = (uint)System.Random.Shared.Next();
                return new Color32((byte)(val & 255), (byte)((val >> 8) & 255), (byte)((val >> 16) & 255), 255);
            }
        }
    }
}