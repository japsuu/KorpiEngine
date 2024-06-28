namespace KorpiEngine.Core.API;

public static class AssemblyManager
{
    public static void Initialize()
    {
        OnApplicationUnloadAttribute.FindAll();
        OnApplicationLoadAttribute.FindAll();
    }
}