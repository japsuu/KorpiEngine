using static KorpiEngine.Serialization.Serializer;

namespace KorpiEngine.Serialization
{
    public interface ISerializable
    {
        public SerializedProperty Serialize(SerializationContext ctx);
        public void Deserialize(SerializedProperty value, SerializationContext ctx);

    }
}
