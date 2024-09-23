namespace KorpiEngine.Rendering;

public class MeshVertexLayout
{
    private readonly VertexAttributeDescriptor[] _attributes;
    
    public readonly int VertexSize;
    public IReadOnlyList<VertexAttributeDescriptor> Attributes => _attributes;

    private readonly bool[] _enabledVertexAttributes = new bool[6];


    public MeshVertexLayout(VertexAttributeDescriptor[] attributes)
    {
        ArgumentNullException.ThrowIfNull(attributes);

        if (attributes.Length == 0)
            throw new ArgumentNullException(nameof(attributes), $"The argument '{nameof(attributes)}' is null!");

        _attributes = attributes;

        foreach (VertexAttributeDescriptor element in _attributes)
        {
            int attributeSize = element.Count * element.AttributeType switch
            {
                VertexAttributeType.Byte => 1,
                VertexAttributeType.UnsignedByte => 1,
                VertexAttributeType.Short => 2,
                VertexAttributeType.UnsignedShort => 2,
                VertexAttributeType.Int => 4,
                VertexAttributeType.UnsignedInt => 4,
                VertexAttributeType.Float => 4,
                _ => throw new ArgumentOutOfRangeException(nameof(attributes))
            };
            
            element.SetOffset((short)VertexSize);
            element.SetStride((short)attributeSize);
            VertexSize += attributeSize;
            
            _enabledVertexAttributes[element.Semantic] = true;
        }
    }
    
    
    public bool IsVertexAttributeEnabled(VertexAttribute attribute) => _enabledVertexAttributes[(int)attribute];


    public class VertexAttributeDescriptor(int semantic, VertexAttributeType attributeType, byte count, bool normalized = false)
    {
        public readonly int Semantic = semantic;
        public readonly VertexAttributeType AttributeType = attributeType;
        public readonly int Count = count;
        public short Offset { get; private set; }
        public short Stride { get; private set; }
        public readonly bool Normalized = normalized;

        
        public VertexAttributeDescriptor(VertexAttribute attribute, VertexAttributeType attributeType, byte count, bool normalized = false) : this((int)attribute, attributeType, count, normalized) { }

        public void SetOffset(short offset) => Offset = offset;
        public void SetStride(short stride) => Stride = stride;
    }
}