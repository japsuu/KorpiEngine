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
    
    
    public static Color White => new Color(1, 1, 1, 1);
    public static Color Black => new Color(0, 0, 0, 1);
    public static Color Red => new Color(1, 0, 0, 1);
    public static Color Green => new Color(0, 1, 0, 1);
    public static Color Blue => new Color(0, 0, 1, 1);
    public static Color Yellow => new Color(1, 1, 0, 1);
    public static Color Cyan => new Color(0, 1, 1, 1);
    public static Color Magenta => new Color(1, 0, 1, 1);
    public static Color Transparent => new Color(0, 0, 0, 0);
}