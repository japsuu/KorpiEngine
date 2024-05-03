using KorpiEngine.Core.Rendering;

namespace KorpiEngine.Core.Internal.Rendering;

internal class MeshVertexLayout
{
    public readonly VertexAttributeDescriptor[] Attributes;
    public readonly int VertexSize;
    public readonly int VertexAttributeCount;
    
    private readonly bool[] _enabledVertexAttributes = new bool[6];


    public MeshVertexLayout(VertexAttributeDescriptor[] attributes)
    {
        ArgumentNullException.ThrowIfNull(attributes);

        if (attributes.Length == 0)
            throw new Exception($"The argument '{nameof(attributes)}' is null!");

        Attributes = attributes;

        foreach (VertexAttributeDescriptor element in Attributes)
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
        
        VertexAttributeCount = Attributes.Length;
    }
    
    
    public bool IsVertexAttributeEnabled(VertexAttribute attribute) => _enabledVertexAttributes[(int)attribute];


    public class VertexAttributeDescriptor
    {
        public readonly int Semantic;
        public readonly VertexAttributeType AttributeType;
        public readonly int Count;
        public short Offset { get; private set; }
        public short Stride { get; private set; }
        public readonly bool Normalized;

        
        public VertexAttributeDescriptor(VertexAttribute attribute, VertexAttributeType attributeType, byte count, bool normalized = false)
        {
            Semantic = (int)attribute;
            AttributeType = attributeType;
            Count = count;
            Normalized = normalized;
        }


        public VertexAttributeDescriptor(int semantic, VertexAttributeType attributeType, byte count, bool normalized = false)
        {
            Semantic = semantic;
            AttributeType = attributeType;
            Count = count;
            Normalized = normalized;
        }


        public VertexAttributeDescriptor(VertexAttributeDescriptor copy)
        {
            Semantic = copy.Semantic;
            AttributeType = copy.AttributeType;
            Count = copy.Count;
            Normalized = copy.Normalized;
        }
        
        
        public void SetOffset(short offset) => Offset = offset;
        public void SetStride(short stride) => Stride = stride;
    }
}