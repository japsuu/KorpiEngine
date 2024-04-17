namespace KorpiEngine.Core.ECS;

public struct NameComponent : INativeComponent
{
    public string Name;


    public NameComponent(string name)
    {
        Name = name;
    }
}