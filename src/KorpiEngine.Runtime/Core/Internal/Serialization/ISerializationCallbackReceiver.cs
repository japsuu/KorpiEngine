﻿namespace KorpiEngine.Core.Internal.Serialization
{
    /// <summary>
    /// Sometimes you dont want a Constructor to be called when deserializing, but still need todo some work after the object has been created
    /// This interface allows you to do that
    /// </summary>
    public interface ISerializationCallbackReceiver
    {
        /// <summary>
        /// Called right before the Serializer serializes this object
        /// </summary>
        public void OnBeforeSerialize();

        /// <summary>
        /// Called right after the Serializer deserializes this object
        /// </summary>
        public void OnAfterDeserialize();
    }
}
