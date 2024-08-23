namespace KorpiEngine.Core;

/// <summary>
/// A universally unique 128-bit identifier.
/// </summary>
public readonly struct UUID
{
    private readonly Guid _value;

    public UUID()
    {
        _value = Guid.NewGuid();
    }


    private UUID(Guid guid)
    {
        _value = guid;
    }

    public static implicit operator Guid(UUID uuid) => uuid._value;
    public static implicit operator UUID(Guid value) => new(value);
    public static bool operator ==(UUID a, UUID b) => a._value == b._value;
    public static bool operator !=(UUID a, UUID b) => a._value != b._value;
    public override bool Equals(object? obj) => obj is UUID other && other._value == _value;
    public override int GetHashCode() => _value.GetHashCode();
    public override string ToString() => _value.ToString();
}