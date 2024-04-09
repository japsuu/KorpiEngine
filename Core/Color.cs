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
}