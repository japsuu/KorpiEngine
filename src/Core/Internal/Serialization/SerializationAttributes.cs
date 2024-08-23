﻿namespace KorpiEngine.Core.Internal.Serialization
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class IgnoreOnNullAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class SerializeIgnoreAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class SerializeFieldAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class FormerlySerializedAsAttribute : Attribute
    {
        public string oldName { get; set; }
        public FormerlySerializedAsAttribute(string name) => oldName = name;
    }
}
