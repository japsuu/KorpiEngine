using System.Diagnostics;

namespace KorpiEngine.Core.EntityModel.IDs;

public readonly struct EntityID : IEquatable<EntityID>
{
    private static ulong nextID;

    private readonly ulong _id;
    
    
    private EntityID(ulong id)
    {
        _id = id;
    }
    
    
    public static EntityID Generate()
    {
        Debug.Assert(Interlocked.Read(ref nextID) != ulong.MaxValue, "EntityID overflow!");
        EntityID id = new(Interlocked.Increment(ref nextID));
        return id;
    }
    
    
    public static implicit operator ulong(EntityID id) => id._id;
    
    public override int GetHashCode() => _id.GetHashCode();
    public bool Equals(EntityID other) => _id == other._id;
    public override bool Equals(object? obj) => obj is EntityID other && Equals(other);
    public static bool operator ==(EntityID left, EntityID right) => left.Equals(right);
    public static bool operator !=(EntityID left, EntityID right) => !left.Equals(right);
    public override string ToString() => _id.ToString();
}