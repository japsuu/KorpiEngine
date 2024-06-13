using KorpiEngine.Core.API;

namespace KorpiEngine.Core;

public readonly struct Color
{
    public readonly float R;
    public readonly float G;
    public readonly float B;
    public readonly float A;


    public Color(float r, float g, float b, float a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }
    
    
    public static Color White => new(1, 1, 1, 1);
    public static Color Black => new(0, 0, 0, 1);
    public static Color Gray => new(0.5f, 0.5f, 0.5f, 1);
    public static Color Red => new(1, 0, 0, 1);
    public static Color Green => new(0, 1, 0, 1);
    public static Color Blue => new(0, 0, 1, 1);
    public static Color Yellow => new(1, 1, 0, 1);
    public static Color Cyan => new(0, 1, 1, 1);
    public static Color Magenta => new(1, 0, 1, 1);
    public static Color Transparent => new(0, 0, 0, 0);


    public void Deconstruct(out float r, out float g, out float b, out float a)
    {
        r = R;
        g = G;
        b = B;
        a = A;
    }
    
    public static implicit operator Vector4(Color c) => new(c.R, c.G, c.B, c.A);
    public static implicit operator System.Numerics.Vector4(Color c) => new(c.R, c.G, c.B, c.A);

    public static implicit operator Color(Vector4 v) => new((float)v.X, (float)v.Y, (float)v.Z, (float)v.W);
    public static implicit operator Color(System.Numerics.Vector4 v) => new(v.X, v.Y, v.Z, v.W);
}