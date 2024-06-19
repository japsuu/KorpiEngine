/*using System.Diagnostics;

namespace KorpiEngine.Core.EntityModel.IDs;

public readonly struct ComponentID : IEquatable<ComponentID>
{
    private static ulong nextID;
    
    public readonly ulong ID;
    
    
    private ComponentID(ulong id)
    {
        ID = id;
    }
    
    
    public static ComponentID Generate()
    {
        Debug.Assert(Interlocked.Read(ref nextID) != ulong.MaxValue, "ComponentID overflow!");
        ComponentID id = new(Interlocked.Increment(ref nextID));
        return id;
    }
    
    
    public static implicit operator ulong(ComponentID id) => id.ID;
    
    public override int GetHashCode() => ID.GetHashCode();
    public bool Equals(ComponentID other) => ID == other.ID;
    public override bool Equals(object? obj) => obj is ComponentID other && Equals(other);
    public static bool operator ==(ComponentID left, ComponentID right) => left.Equals(right);
    public static bool operator !=(ComponentID left, ComponentID right) => !left.Equals(right);
    public override string ToString() => ID.ToString();
}*/