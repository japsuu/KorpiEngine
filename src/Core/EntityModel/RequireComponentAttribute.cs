namespace KorpiEngine.EntityModel;

[AttributeUsage(AttributeTargets.Class)]
public class RequireComponentAttribute(params Type[] types) : Attribute
{
    public Type[] Types { get; } = types;
}