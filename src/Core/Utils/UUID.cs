namespace KorpiEngine;

/// <summary>
/// A universally unique 128-bit identifier.
/// </summary>
public readonly struct UUID
{
    public static readonly UUID Empty = Guid.Empty;
    
    private readonly Guid _value;

    
    public UUID() => _value = Guid.NewGuid();
    private UUID(Guid guid) => _value = guid;
    
    
    public static bool TryParse(string input, out UUID result)
    {
        if (Guid.TryParse(input, out Guid guid))
        {
            result = guid;
            return true;
        }

        result = Empty;
        return false;
    }
    
    
    public static UUID Parse(string input) => Guid.Parse(input);
    

    public static implicit operator Guid(UUID uuid) => uuid._value;
    public static implicit operator UUID(Guid value) => new(value);
    public static bool operator ==(UUID a, UUID b) => a._value == b._value;
    public static bool operator !=(UUID a, UUID b) => a._value != b._value;
    public override bool Equals(object? obj) => obj is UUID other && other._value == _value;
    public override int GetHashCode() => _value.GetHashCode();
    public override string ToString() => _value.ToString();
}