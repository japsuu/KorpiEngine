namespace KorpiEngine.Core;

public readonly struct Color
{
    public readonly float R;
    public readonly float G;
    public readonly float B;
    public readonly float A;
    
    public static Color White => new Color(1, 1, 1, 1);
    public static Color Black => new Color(0, 0, 0, 1);
    public static Color Gray => new Color(0.5f, 0.5f, 0.5f, 1);
    public static Color Red => new Color(1, 0, 0, 1);
    public static Color Green => new Color(0, 1, 0, 1);
    public static Color Blue => new Color(0, 0, 1, 1);
    public static Color Yellow => new Color(1, 1, 0, 1);
    public static Color Cyan => new Color(0, 1, 1, 1);
    public static Color Magenta => new Color(1, 0, 1, 1);
    public static Color Transparent => new Color(0, 0, 0, 0);


    public Color(float r, float g, float b, float a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }


    public Color(System.Numerics.Vector4 color)
    {
        R = color.X;
        G = color.Y;
        B = color.Z;
        A = color.W;
    }


    public void Deconstruct(out float r, out float g, out float b, out float a)
    {
        r = R;
        g = G;
        b = B;
        a = A;
    }


    public override string ToString() => $"({R}, {G}, {B}, {A})";
}