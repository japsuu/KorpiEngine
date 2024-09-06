namespace KorpiEngine.Utils;

public static class AssemblyManager
{
    public static void Initialize()
    {
        OnApplicationUnloadAttribute.FindAll();
        OnApplicationLoadAttribute.FindAll();
    }
}