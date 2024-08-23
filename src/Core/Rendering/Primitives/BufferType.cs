namespace KorpiEngine.Core.Rendering.Primitives;

public enum BufferType
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