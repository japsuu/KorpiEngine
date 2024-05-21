namespace KorpiEngine.Core.Internal.Utils;

public static class Hashing
{
    public static int GetXorHashCode<T>(HashSet<T> set)
    {
        int hashCode = 0;
        foreach (T item in set)
        {
            hashCode ^= HashCode.Combine(set.Comparer.GetHashCode(item!));
        }
        return hashCode;
    }
    
    public static int GetAdditiveHashCode<T>(SortedSet<T> set)
    {
        unchecked // Overflow is fine, just wrap
        {
            int hash = 17;
            foreach (T item in set)
            {
                hash = hash * 23 + item!.GetHashCode();
            }
            
            return hash;
        }
    }
}