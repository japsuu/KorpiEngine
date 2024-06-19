using System.Diagnostics;

namespace KorpiEngine.Core.EntityModel.IDs;

public readonly struct WorldSystemID : IEquatable<WorldSystemID>
{
    private static ulong nextID;

    private readonly ulong _id;
    
    
    private WorldSystemID(ulong id)
    {
        _id = id;
    }
    
    
    public static WorldSystemID Generate<T>() where T : WorldSystem
    {
        Debug.Assert(Interlocked.Read(ref nextID) != ulong.MaxValue, "GlobalSystemID overflow!");
        WorldSystemID id = new(TypedIDs<T>.Bit);
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
    
    
    public static implicit operator ulong(WorldSystemID id) => id._id;
    
    public override int GetHashCode() => _id.GetHashCode();
    public bool Equals(WorldSystemID other) => _id == other._id;
    public override bool Equals(object? obj) => obj is WorldSystemID other && Equals(other);
    public static bool operator ==(WorldSystemID left, WorldSystemID right) => left.Equals(right);
    public static bool operator !=(WorldSystemID left, WorldSystemID right) => !left.Equals(right);
    public override string ToString() => _id.ToString();
}