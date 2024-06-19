using System.Diagnostics;

namespace KorpiEngine.Core.EntityModel.IDs;

public readonly struct EntitySystemID : IEquatable<EntitySystemID>
{
    private static ulong nextID;

    private readonly ulong _id;
    
    
    private EntitySystemID(ulong id)
    {
        _id = id;
    }
    
    
    public static EntitySystemID Generate<T>() where T : IEntitySystem
    {
        Debug.Assert(Interlocked.Read(ref nextID) != ulong.MaxValue, "EntitySystemID overflow!");
        EntitySystemID id = new(TypedIDs<T>.Bit);
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
    
    
    public static implicit operator ulong(EntitySystemID id) => id._id;
    
    public override int GetHashCode() => _id.GetHashCode();
    public bool Equals(EntitySystemID other) => _id == other._id;
    public override bool Equals(object? obj) => obj is EntitySystemID other && Equals(other);
    public static bool operator ==(EntitySystemID left, EntitySystemID right) => left.Equals(right);
    public static bool operator !=(EntitySystemID left, EntitySystemID right) => !left.Equals(right);
    public override string ToString() => _id.ToString();
}