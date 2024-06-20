using System.Diagnostics;

namespace KorpiEngine.Core.EntityModel.IDs;

public readonly struct ComponentID : IEquatable<ComponentID>
{
    private static ulong nextID;

    private readonly ulong _id;
    
    
    private ComponentID(ulong id)
    {
        _id = id;
    }
    
    
    public static ComponentID Generate()
    {
        Debug.Assert(Interlocked.Read(ref nextID) != ulong.MaxValue, "ComponentID overflow!");
        ComponentID id = new(Interlocked.Increment(ref nextID));
        return id;
    }
    
    
    public static implicit operator ulong(ComponentID id) => id._id;
    
    public override int GetHashCode() => _id.GetHashCode();
    public bool Equals(ComponentID other) => _id == other._id;
    public override bool Equals(object? obj) => obj is ComponentID other && Equals(other);
    public static bool operator ==(ComponentID left, ComponentID right) => left.Equals(right);
    public static bool operator !=(ComponentID left, ComponentID right) => !left.Equals(right);
    public override string ToString() => _id.ToString();
}