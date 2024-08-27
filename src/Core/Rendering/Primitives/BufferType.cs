namespace KorpiEngine.Core.Rendering.Primitives;

internal enum BufferType
{
    VertexBuffer,
    ElementsBuffer,
    UniformBuffer,
    StructuredBuffer,
    /// <summary>
    /// DO NOT USE!
    /// This is used to count the number of possible buffer types.
    /// </summary>
    Count
}