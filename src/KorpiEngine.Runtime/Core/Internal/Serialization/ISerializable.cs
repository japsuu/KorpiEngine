using static KorpiEngine.Core.Internal.Serialization.Serializer;

namespace KorpiEngine.Core.Internal.Serialization
{
    public interface ISerializable
    {
        public SerializedProperty Serialize(SerializationContext ctx);
        public void Deserialize(SerializedProperty value, SerializationContext ctx);

    }
}
