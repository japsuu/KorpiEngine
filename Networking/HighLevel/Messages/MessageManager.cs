using System.Reflection;
using System.Runtime.Serialization;

namespace Korpi.Networking.HighLevel.Messages;

internal static class MessageManager
{
    /// <summary>
    /// Uses reflection to find all NetMessage types, and registers them.
    /// </summary>
    public static void RegisterAllMessages()
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        foreach (Type type in assembly.GetTypes())
            if (type.IsSubclassOf(typeof(NetMessage)) && !type.IsAbstract)
                RegisterMessage(type);
    }


    private static void RegisterMessage(Type messageType)
    {
        // Use reflection to call the generic GetId<T> method to generate the message ID.
        MethodInfo? getIdMethod = typeof(MessageIdCache).GetMethod(nameof(MessageIdCache.GetId), Type.EmptyTypes);
        if (getIdMethod == null)
            throw new InvalidOperationException("Could not find GetId method in MessageIdCache. Cannot register message types.");
        MethodInfo getIdMethodGeneric = getIdMethod.MakeGenericMethod(messageType);
        getIdMethodGeneric.Invoke(null, null);

        // Use reflection to call the generic Register<T> method.
        MethodInfo? registerMethod = typeof(MessageTypeCache).GetMethod(nameof(MessageTypeCache.Register), Type.EmptyTypes);
        if (registerMethod == null)
            throw new InvalidOperationException("Could not find Register method in MessageTypeCache. Cannot register message types.");
        MethodInfo registerMethodGeneric = registerMethod.MakeGenericMethod(messageType);
        registerMethodGeneric.Invoke(null, null);
    }


    /// <summary>
    /// Provides methods for retrieving message IDs for message types.
    /// </summary>
    internal static class MessageIdCache
    {
        private static ushort nextId;
        private static readonly Dictionary<Type, ushort> TypeToIdMap = new();

        public static ushort GetId<T>() => GetId(typeof(T));


        public static ushort GetId(Type type)
        {
            if (TypeToIdMap.TryGetValue(type, out ushort id))
                return id;

            if (nextId == ushort.MaxValue)
                throw new InvalidOperationException("Ran out of message IDs. Cannot register more message types.");

            id = nextId++;
            TypeToIdMap[type] = id;

            return id;
        }
    }

    /// <summary>
    /// Provides methods for creating instances of message types from their IDs.
    /// </summary>
    internal static class MessageTypeCache
    {
        private static readonly Dictionary<ushort, Func<NetMessage>> IdToCreatorMap = new();


        public static void Register<T>() where T : NetMessage
        {
            ushort id = MessageIdCache.GetId<T>();
            IdToCreatorMap[id] = () => (T)FormatterServices.GetUninitializedObject(typeof(T));
        }


        public static NetMessage CreateInstance(ushort id)
        {
            if (IdToCreatorMap.TryGetValue(id, out Func<NetMessage>? creator))
                return creator();

            throw new InvalidOperationException($"No message type registered for ID {id}, cannot create instance.");
        }
    }
}