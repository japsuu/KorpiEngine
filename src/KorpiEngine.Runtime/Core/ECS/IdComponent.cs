namespace KorpiEngine.Core.ECS;

public struct IdComponent : INativeComponent
{
    public UUID Id;


    public IdComponent(UUID id)
    {
        Id = id;
    }
}