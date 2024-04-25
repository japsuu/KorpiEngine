using static KorpiEngine.Core.Internal.Serializer.Serializer;

namespace KorpiEngine.Core.Internal.Serializer
{
    public interface ISerializable
    {
        public SerializedProperty Serialize(SerializationContext ctx);
        public void Deserialize(SerializedProperty value, SerializationContext ctx);

    }
}
