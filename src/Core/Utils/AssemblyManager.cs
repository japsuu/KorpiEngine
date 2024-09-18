namespace KorpiEngine.Utils;

internal static class AssemblyManager
{
    public static void Initialize()
    {
        OnApplicationUnloadAttribute.FindAll();
        OnApplicationLoadAttribute.FindAll();
    }
}