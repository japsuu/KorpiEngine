namespace KorpiEngine.Core.Internal.Utils;

public static class Hashing
{
    public static int GetHashCode<T>(HashSet<T> hashSet)
    {
        int hashCode = 0;
        foreach (T item in hashSet)
        {
            hashCode ^= HashCode.Combine(hashSet.Comparer.GetHashCode(item!));
        }
        return hashCode;
    }
}