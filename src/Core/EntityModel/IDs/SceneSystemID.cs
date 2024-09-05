using System.Diagnostics;

namespace KorpiEngine.EntityModel.IDs;

public readonly struct SceneSystemID : IEquatable<SceneSystemID>
{
    private static ulong nextID;

    private readonly ulong _id;
    
    
    private SceneSystemID(ulong id)
    {
        _id = id;
    }
    
    
    public static SceneSystemID Generate<T>() where T : SceneSystem
    {
        Debug.Assert(Interlocked.Read(ref nextID) != ulong.MaxValue, "GlobalSystemID overflow!");
        SceneSystemID id = new(TypedIDs<T>.Bit);
        return id;
    }

    // ReSharper disable once UnusedTypeParameter
    private static class TypedIDs<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        internal static readonly ulong Bit;

        static TypedIDs()
        {
            Bit = Interlocked.Increment(ref nextID);
        }
    }
    
    
    public static implicit operator ulong(SceneSystemID id) => id._id;
    
    public override int GetHashCode() => _id.GetHashCode();
    public bool Equals(SceneSystemID other) => _id == other._id;
    public override bool Equals(object? obj) => obj is SceneSystemID other && Equals(other);
    public static bool operator ==(SceneSystemID left, SceneSystemID right) => left.Equals(right);
    public static bool operator !=(SceneSystemID left, SceneSystemID right) => !left.Equals(right);
    public override string ToString() => _id.ToString();
}