namespace KorpiEngine;

public static class LinqUtil
{
    public static AABox ToAABox(this IEnumerable<Vector3> self) => AABox.Create(self);
}