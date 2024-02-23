namespace Korpi.Networking.Utility;

public static class ArraySegmentUtils
{
    public static string AsStringHex(this ArraySegment<byte> segment)
    {
        return string.Join(" ", segment.AsSpan().ToArray().Select(b => b.ToString("X2").PadLeft(2, '0')));
    }
    
    
    public static string AsStringDecimal(this ArraySegment<byte> segment)
    {
        return string.Join(" ", segment.AsSpan().ToArray().Select(b => b.ToString()));
    }
    
    
    public static string AsStringBits(this ArraySegment<byte> segment)
    {
        return string.Join(" ", segment.AsSpan().ToArray().Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
    }
}