using static KorpiEngine.Tools.Serialization.Serializer;

namespace KorpiEngine.Tools.Serialization
{
    public interface ISerializable
    {
        public SerializedProperty Serialize(SerializationContext ctx);
        public void Deserialize(SerializedProperty value, SerializationContext ctx);

    }
}
