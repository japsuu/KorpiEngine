using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace KorpiEngine.Core.API;

public struct Vector2i : IEquatable<Vector2i>, IFormattable
{
    public int X;
    public int Y;


    #region Constructors

    /// <summary> Constructs a vector whose elements are all the single specified value. </summary>
    public Vector2i(int value) : this(value, value)
    {
    }


    /// <summary> Constructs a vector with the given individual elements. </summary>
    public Vector2i(int x, int y)
    {
        X = x;
        Y = y;
    }

    #endregion Constructors

    
    #region Public Instance Methods
    
    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode()
    {
        int hash = X.GetHashCode();
        hash = HashCode.Combine(hash, Y.GetHashCode());
        return hash;
    }


    /// <summary>
    /// Returns a boolean indicating whether the given Object is equal to this Vector2 instance.
    /// </summary>
    /// <param name="obj">The Object to compare against.</param>
    /// <returns>True if the Object is equal to this Vector2; False otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj)
    {
        if (obj is not Vector2i)
            return false;
        return Equals((Vector2i)obj);
    }


    /// <summary>
    /// Returns a boolean indicating whether the given Vector2i is equal to this Vector2i instance.
    /// </summary>
    /// <param name="other">The Vector2i to compare this instance to.</param>
    /// <returns>True if the other Vector2 is equal to this instance; False otherwise.</returns>
    public bool Equals(Vector2i other) => X == other.X && Y == other.Y;


    /// <summary>
    /// Returns a String representing this Vector2 instance.
    /// </summary>
    /// <returns>The string representation.</returns>
    public override string ToString() => ToString("G", CultureInfo.CurrentCulture);


    /// <summary>
    /// Returns a String representing this Vector2 instance, using the specified format to format individual elements.
    /// </summary>
    /// <param name="format">The format of individual elements.</param>
    /// <returns>The string representation.</returns>
    public string ToString(string format) => ToString(format, CultureInfo.CurrentCulture);


    /// <summary>
    /// Returns a String representing this Vector2 instance, using the specified format to format individual elements 
    /// and the given IFormatProvider.
    /// </summary>
    /// <param name="format">The format of individual elements.</param>
    /// <param name="formatProvider">The format provider to use when formatting elements.</param>
    /// <returns>The string representation.</returns>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        StringBuilder sb = new();
        string separator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;
        sb.Append('<');
        sb.Append(X.ToString(format, formatProvider));
        sb.Append(separator);
        sb.Append(' ');
        sb.Append(Y.ToString(format, formatProvider));
        sb.Append('>');
        return sb.ToString();
    }

    #endregion Public Instance Methods


    #region Public Static Properties

    public static Vector2i Zero => new();
    public static Vector2i One => new(1, 1);
    public static Vector2i Right => new(1, 0);
    public static Vector2i Left => new(1, 0);
    public static Vector2i Up => new(0, 1);
    public static Vector2i Down => new(0, 1);

    #endregion Public Static Properties
}