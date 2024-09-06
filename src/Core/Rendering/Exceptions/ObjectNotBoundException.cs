﻿namespace KorpiEngine.Rendering;

/// <summary>
/// The exception that is thrown when an object is used which must be bound before usage.
/// </summary>
internal class ObjectNotBoundException : OpenGLException
{
    internal ObjectNotBoundException(string message) : base(message)
    {
    }
}