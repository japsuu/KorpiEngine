namespace KorpiEngine.Core.API;

public static class AssemblyManager
{
    public static void Initialize()
    {
        OnAssemblyUnloadAttribute.FindAll();
        OnAssemblyLoadAttribute.FindAll();
    }
}