using System.Collections;
using System.Reflection;
using KorpiEngine.Core.Internal.Utils;
using KorpiEngine.Core.Logging;

namespace KorpiEngine.Core.Internal.Serialization;

// Strongly based on the MIT-licenced Prowl engine serializer by michaelsakharov:
// https://github.com/michaelsakharov/Prowl/blob/main/Prowl.Runtime/Serializer/Serializer.cs

/// <summary>
/// A class that can serialize and deserialize objects into a SerializedProperty format.
/// </summary>
public static class Serializer
{
    private static readonly IKorpiLogger Logger = LogFactory.GetLogger(typeof(Serializer));

    public class SerializationContext
    {
        public readonly Dictionary<object, int> ObjectToId = new(ReferenceEqualityComparer.Instance);
        public readonly Dictionary<int, object> IDToObject = new();
        public readonly HashSet<Guid> Dependencies = new();
        public int NextId;
        private int _dependencyCounter = 0;


        public SerializationContext()
        {
            ObjectToId.Clear();
            ObjectToId.Add(new NullKey(), 0);
            IDToObject.Clear();
            IDToObject.Add(0, new NullKey());
            NextId = 1;
            Dependencies.Clear();
        }


        public void AddDependency(Guid guid)
        {
            if (_dependencyCounter > 0)
                Dependencies.Add(guid);
            else
                throw new InvalidOperationException("Cannot add a dependency outside of a BeginDependencies/EndDependencies block.");
        }


        public void BeginDependencies()
        {
            _dependencyCounter++;
        }


        public HashSet<Guid> EndDependencies()
        {
            _dependencyCounter--;
            if (_dependencyCounter == 0)
                return Dependencies;
            return new HashSet<Guid>();
        }
    }

    private class NullKey
    {
    }


    private static bool IsPrimitive(Type t) => t.IsPrimitive || t.IsAssignableTo(typeof(string)) || t.IsAssignableTo(typeof(decimal)) ||
                                               t.IsAssignableTo(typeof(Guid)) || t.IsAssignableTo(typeof(DateTime)) || t.IsEnum ||
                                               t.IsAssignableTo(typeof(byte[]));


    #region Serialize

    public static SerializedProperty Serialize(object? value) => Serialize(value, new SerializationContext());


    public static SerializedProperty Serialize(object? value, SerializationContext ctx)
    {
        if (value == null)
            return new SerializedProperty(PropertyType.Null, null);

        if (value is SerializedProperty t)
        {
            SerializedProperty clone = t.Clone();
            HashSet<Guid> deps = new();
            clone.GetAllAssetRefs(ref deps);
            foreach (Guid dep in deps)
                ctx.AddDependency(dep);
            return clone;
        }

        Type type = value.GetType();
        if (IsPrimitive(type))
            return PrimitiveToTag(value);

        if (type.IsArray && value is Array array)
            return ArrayToListTag(array, ctx);

        SerializedProperty? tag = DictionaryToTag(value, ctx);
        if (tag != null)
            return tag;

        if (value is IList iList)
            return ListInterfaceToTag(iList, ctx);

        return SerializeObject(value, ctx);
    }


    private static SerializedProperty PrimitiveToTag(object p)
    {
        if (p is byte b)
            return new SerializedProperty(PropertyType.Byte, b);
        else if (p is sbyte sb)
            return new SerializedProperty(PropertyType.SByte, sb);
        else if (p is short s)
            return new SerializedProperty(PropertyType.Short, s);
        else if (p is int i)
            return new SerializedProperty(PropertyType.Int, i);
        else if (p is long l)
            return new SerializedProperty(PropertyType.Long, l);
        else if (p is uint ui)
            return new SerializedProperty(PropertyType.UInt, ui);
        else if (p is ulong ul)
            return new SerializedProperty(PropertyType.ULong, ul);
        else if (p is ushort us)
            return new SerializedProperty(PropertyType.UShort, us);
        else if (p is float f)
            return new SerializedProperty(PropertyType.Float, f);
        else if (p is double d)
            return new SerializedProperty(PropertyType.Double, d);
        else if (p is decimal dec)
            return new SerializedProperty(PropertyType.Decimal, dec);
        else if (p is string str)
            return new SerializedProperty(PropertyType.String, str);
        else if (p is byte[] bArr)
            return new SerializedProperty(PropertyType.ByteArray, bArr);
        else if (p is bool bo)
            return new SerializedProperty(PropertyType.Bool, bo);
        else if (p is DateTime date)
            return new SerializedProperty(PropertyType.Long, date.ToBinary());
        else if (p is Guid g)
            return new SerializedProperty(PropertyType.String, g.ToString());
        else if (p.GetType().IsEnum)
            return new SerializedProperty(PropertyType.Int, (int)p); // Serialize as integers
        else
            throw new NotSupportedException("The type '" + p.GetType() + "' is not a supported primitive.");
    }


    private static SerializedProperty ArrayToListTag(Array array, SerializationContext ctx)
    {
        List<SerializedProperty> tags = new();
        for (int i = 0; i < array.Length; i++)
            tags.Add(Serialize(array.GetValue(i), ctx));
        return new SerializedProperty(tags);
    }


    private static SerializedProperty? DictionaryToTag(object obj, SerializationContext ctx)
    {
        Type t = obj.GetType();
        if (obj is IDictionary dict &&
            t.IsGenericType &&
            t.GetGenericArguments()[0] == typeof(string))
        {
            SerializedProperty tag = new(PropertyType.Compound, null);
            foreach (DictionaryEntry kvp in dict)
                tag.Add((string)kvp.Key, Serialize(kvp.Value, ctx));
            return tag;
        }

        return null;
    }


    private static SerializedProperty ListInterfaceToTag(IList iList, SerializationContext ctx)
    {
        List<SerializedProperty> tags = new();
        foreach (object? item in iList)
            tags.Add(Serialize(item, ctx));
        return new SerializedProperty(tags);
    }


    private static SerializedProperty SerializeObject(object? value, SerializationContext ctx)
    {
        if (value == null)
            return new SerializedProperty(PropertyType.Null, null); // ID defaults to 0 which is null or an Empty Compound

        Type type = value.GetType();

        SerializedProperty compound = SerializedProperty.NewCompound();

        if (ctx.ObjectToId.TryGetValue(value, out int id))
        {
            compound["$id"] = new SerializedProperty(PropertyType.Int, id);

            // Don't need to write compound data, its already been serialized at some point earlier
            return compound;
        }

        id = ctx.NextId++;
        ctx.ObjectToId[value] = id;
        ctx.IDToObject[id] = value;
        ctx.BeginDependencies();

        if (value is ISerializationCallbackReceiver callback)
            callback.OnBeforeSerialize();

        if (value is ISerializable serializable)

            // Manual Serialization
            compound = serializable.Serialize(ctx);
        else

            // Auto Serialization
            foreach (FieldInfo field in RuntimeUtils.GetSerializableFields(value))
            {
                string name = field.Name;

                object? propValue = field.GetValue(value);
                if (propValue == null)
                {
                    if (Attribute.GetCustomAttribute(field, typeof(IgnoreOnNullAttribute)) != null)
                        continue;
                    compound.Add(name, new SerializedProperty(PropertyType.Null, null));
                }
                else
                {
                    SerializedProperty tag = Serialize(propValue, ctx);
                    compound.Add(name, tag);
                }
            }

        compound["$id"] = new SerializedProperty(PropertyType.Int, id);
        compound["$type"] = new SerializedProperty(PropertyType.String, type.FullName);
        ctx.EndDependencies();

        return compound;
    }

    #endregion


    #region Deserialize

    public static T? Deserialize<T>(SerializedProperty value) => (T?)Deserialize(value, typeof(T));

    public static object? Deserialize(SerializedProperty value, Type type) => Deserialize(value, type, new SerializationContext());

    public static T? Deserialize<T>(SerializedProperty value, SerializationContext ctx) => (T?)Deserialize(value, typeof(T), ctx);


    public static object? Deserialize(SerializedProperty value, Type targetType, SerializationContext ctx)
    {
        if (value.TagType == PropertyType.Null)
            return null;

        if (value.GetType().IsAssignableTo(targetType))
            return value;

        if (IsPrimitive(targetType))
        {
            // Special Cases
            if (targetType.IsEnum)
                if (value.TagType == PropertyType.Int)
                    return Enum.ToObject(targetType, value.IntValue);

            if (targetType == typeof(DateTime))
                if (value.TagType == PropertyType.Long)
                    return DateTime.FromBinary(value.LongValue);

            if (targetType == typeof(Guid))
                if (value.TagType == PropertyType.String)
                    return Guid.Parse(value.StringValue);

            return Convert.ChangeType(value.Value, targetType);
        }

        if (value.TagType == PropertyType.List)
        {
            if (targetType.IsArray)
            {
                // Deserialize List into Array
                Type type = targetType.GetElementType() ?? throw new InvalidOperationException("Array type is null");
                Array array = Array.CreateInstance(type, value.Count);
                for (int idx = 0; idx < array.Length; idx++)
                    array.SetValue(Deserialize(value[idx], type, ctx), idx);
                return array;
            }
            else if (targetType.IsAssignableTo(typeof(IList)))
            {
                // IEnumerable covers many types, we need to find the type of element in the IEnumerable
                // For now just assume its the first generic argument
                Type type = targetType.GetGenericArguments()[0];
                IList list2 = CreateInstance(targetType) as IList ?? throw new InvalidOperationException("Failed to create instance of type: " + targetType);
                foreach (SerializedProperty tag in value.List)
                    list2.Add(Deserialize(tag, type, ctx));
                return list2;
            }

            throw new InvalidCastException("ListTag cannot deserialize into type of: '" + targetType + "'");
        }
        else if (value.TagType == PropertyType.Compound)
        {
            if (targetType.IsAssignableTo(typeof(IDictionary)) &&
                targetType.IsGenericType &&
                targetType.GetGenericArguments()[0] == typeof(string))
            {
                IDictionary dict = CreateInstance(targetType) as IDictionary ??
                                   throw new InvalidOperationException("Failed to create instance of type: " + targetType);
                Type valueType = targetType.GetGenericArguments()[1];
                foreach (KeyValuePair<string, SerializedProperty> tag in value.Tags)
                    dict.Add(tag.Key, Deserialize(tag.Value, valueType, ctx));
                return dict;
            }

            return DeserializeObject(value, ctx);
        }

        throw new NotSupportedException("The node type '" + value.GetType() + "' is not supported.");
    }


    private static object? DeserializeObject(SerializedProperty compound, SerializationContext ctx)
    {
        SerializedProperty? id = compound.Get("$id");
        if (id != null)
            if (ctx.IDToObject.TryGetValue(id.IntValue, out object? existingObj))
                return existingObj;

        SerializedProperty? typeProperty = compound.Get("$type");
        if (typeProperty == null)
            return null;

        string type = typeProperty.StringValue;
        if (string.IsNullOrWhiteSpace(type))
            return null;

        Type? oType = RuntimeUtils.FindType(type);
        if (oType == null)
        {
            Logger.Error("Couldn't find type: " + type);
            return null;
        }

        object resultObject = CreateInstance(oType);

        if (id != null)
            ctx.IDToObject[id.IntValue] = resultObject;
        resultObject = DeserializeInto(compound, resultObject, ctx);

        return resultObject;
    }


    public static object DeserializeInto(SerializedProperty tag, object into) => DeserializeInto(tag, into, new SerializationContext());


    private static object DeserializeInto(SerializedProperty tag, object into, SerializationContext ctx)
    {
        if (into is ISerializable serializable)
        {
            serializable.Deserialize(tag, ctx);
            into = serializable;
        }
        else
        {
            foreach (FieldInfo field in RuntimeUtils.GetSerializableFields(into))
            {
                string name = field.Name;

                if (!tag.TryGet(name, out SerializedProperty? node))
                {
                    // Before we completely give up, a field can have FormerlySerializedAs Attributes
                    // This allows backwards compatibility
                    Attribute[] formerNames = Attribute.GetCustomAttributes(field, typeof(FormerlySerializedAsAttribute));
                    foreach (FormerlySerializedAsAttribute formerName in formerNames.Cast<FormerlySerializedAsAttribute>())
                        if (tag.TryGet(formerName.oldName, out node))
                        {
                            break;
                        }
                }

                if (node == null) // Continue onto the next field
                    continue;

                object? data = Deserialize(node, field.FieldType, ctx);

                // Some manual casting for edge cases
                if (data is byte @byte)
                {
                    if (field.FieldType == typeof(bool))
                        data = @byte != 0;
                    if (field.FieldType == typeof(sbyte))
                        data = (sbyte)@byte;
                }

                field.SetValue(into, data);
            }
        }

        if (into is ISerializationCallbackReceiver callback2)
            callback2.OnAfterDeserialize();
        return into;
    }


    private static object CreateInstance(Type type) =>
        Activator.CreateInstance(type) ?? throw new InvalidOperationException("Failed to create instance of type: " + type);

    #endregion
}