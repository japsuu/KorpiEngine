// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace KorpiEngine.Core.Tests.Math;

public static class TestUtil
{
    private static Random s_random = new Random();
    public static void SetRandomSeed(int seed)
        => s_random = new Random(seed);

    /// <summary>
    /// Generates random floats between 0 and 100.
    /// </summary>
    /// <param name="numValues">The number of values to generate</param>
    /// <returns>An array containing the random floats</returns>
    public static float[] GenerateRandomFloats(int numValues)
    {
        var values = new float[numValues];
        for (var g = 0; g < numValues; g++)
        {
            values[g] = (float)(s_random.NextDouble() * 99 + 1);
        }
        return values;
    }

    /// <summary>
    /// Generates random ints between 0 and 99, inclusive.
    /// </summary>
    /// <param name="numValues">The number of values to generate</param>
    /// <returns>An array containing the random ints</returns>
    public static int[] GenerateRandomInts(int numValues)
    {
        var values = new int[numValues];
        for (var g = 0; g < numValues; g++)
        {
            values[g] = s_random.Next(1, 100);
        }
        return values;
    }

    /// <summary>
    /// Generates random doubles between 0 and 100.
    /// </summary>
    /// <param name="numValues">The number of values to generate</param>
    /// <returns>An array containing the random doubles</returns>
    public static double[] GenerateRandomDoubles(int numValues)
    {
        var values = new double[numValues];
        for (var g = 0; g < numValues; g++)
        {
            values[g] = s_random.NextDouble() * 99 + 1;
        }
        return values;
    }

    /// <summary>
    /// Generates random doubles between 1 and 100.
    /// </summary>
    /// <param name="numValues">The number of values to generate</param>
    /// <returns>An array containing the random doubles</returns>
    public static long[] GenerateRandomLongs(int numValues)
    {
        var values = new long[numValues];
        for (var g = 0; g < numValues; g++)
        {
            values[g] = s_random.Next(1, 100) * (long.MaxValue / int.MaxValue);
        }
        return values;
    }

    public static T[] GenerateRandomValues<T>(int numValues, int min = 1, int max = 100) where T : struct
    {
        var values = new T[numValues];
        for (var g = 0; g < numValues; g++)
        {
            values[g] = GenerateSingleValue<T>(min, max);
        }

        return values;
    }

    public static T GenerateSingleValue<T>(int min = 1, int max = 100) where T : struct
    {
        var randomRange = s_random.Next(min, max);
        var value = unchecked((T)(dynamic)randomRange);
        return value;
    }

    public static T Abs<T>(T value) where T : struct
    {
        var unsignedTypes = new[] { typeof(byte), typeof(ushort), typeof(uint), typeof(ulong) };
        if (unsignedTypes.Contains(typeof(T)))
        {
            return value;
        }

        dynamic dyn = (dynamic)value;
        var abs = System.Math.Abs(dyn);
        var ret = (T)abs;
        return ret;
    }

    public static T Sqrt<T>(T value) where T : struct
        => unchecked((T)(dynamic)(System.Math.Sqrt((dynamic)value)));

    public static T Multiply<T>(T left, T right) where T : struct
        => unchecked((T)((dynamic)left * right));

    public static T Divide<T>(T left, T right) where T : struct
        => (T)((dynamic)left / right);

    public static T Add<T>(T left, T right) where T : struct
        => unchecked((T)((dynamic)left + right));

    public static T Subtract<T>(T left, T right) where T : struct
        => unchecked((T)((dynamic)left - right));

    public static T Xor<T>(T left, T right) where T : struct
        => (T)((dynamic)left ^ right);

    public static T AndNot<T>(T left, T right) where T : struct
        => (T)((dynamic)left & ~(dynamic)right);

    public static T OnesComplement<T>(T left) where T : struct
        => unchecked((T)(~(dynamic)left));

    public static float Clamp(float value, float min, float max)
        => value > max ? max : value < min ? min : value;

    public static T Zero<T>() where T : struct
        => (T)(dynamic)0;

    public static T One<T>() where T : struct
        => (T)(dynamic)1;

    public static bool GreaterThan<T>(T left, T right) where T : struct
    {
        var result = (dynamic)left > right;
        return (bool)result;
    }

    public static bool GreaterThanOrEqual<T>(T left, T right) where T : struct
    {
        var result = (dynamic)left >= right;
        return (bool)result;
    }

    public static bool LessThan<T>(T left, T right) where T : struct
    {
        var result = (dynamic)left < right;
        return (bool)result;
    }

    public static bool LessThanOrEqual<T>(T left, T right) where T : struct
    {
        var result = (dynamic)left <= right;
        return (bool)result;
    }

    public static bool AnyEqual<T>(T[] left, T[] right) where T : struct
    {
        for (var g = 0; g < left.Length; g++)
        {
            if (((IEquatable<T>)left[g]).Equals(right[g]))
            {
                return true;
            }
        }
        return false;
    }

    public static bool AllEqual<T>(T[] left, T[] right) where T : struct
    {
        for (var g = 0; g < left.Length; g++)
        {
            if (!((IEquatable<T>)left[g]).Equals(right[g]))
            {
                return false;
            }
        }
        return true;
    }
}