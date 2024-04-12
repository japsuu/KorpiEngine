namespace KorpiEngine.Core;

public readonly struct Color32
{
    public readonly byte R;
    public readonly byte G;
    public readonly byte B;
    public readonly byte A;


    public Color32(byte r, byte g, byte b, byte a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }
    
    
    public static Color32 White => new Color32(255, 255, 255, 255);
    public static Color32 Black => new Color32(0, 0, 0, 255);
    public static Color32 Red => new Color32(255, 0, 0, 255);
    public static Color32 Green => new Color32(0, 255, 0, 255);
    public static Color32 Blue => new Color32(0, 0, 255, 255);
    public static Color32 Yellow => new Color32(255, 255, 0, 255);
    public static Color32 Cyan => new Color32(0, 255, 255, 255);
    public static Color32 Magenta => new Color32(255, 0, 255, 255);
    public static Color32 Transparent => new Color32(0, 0, 0, 0);
}