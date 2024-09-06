using System.Reflection;

namespace KorpiEngine;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class OnApplicationUnloadAttribute : Attribute
{
    private static readonly List<MethodInfo> MethodInfos = [];


    public static void Invoke()
    {
        foreach (MethodInfo methodInfo in MethodInfos)
            methodInfo.Invoke(null, null);
    }


    public static void FindAll()
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly assembly in assemblies)
        {
            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (MethodInfo method in methods)
                {
                    IEnumerable<OnApplicationUnloadAttribute> attributes = method.GetCustomAttributes<OnApplicationUnloadAttribute>();
                    if (attributes.Any())
                        MethodInfos.Add(method);
                }
            }
        }
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class OnApplicationLoadAttribute(int order = 0) : Attribute
{
    private readonly int _order = order;
    private static readonly List<MethodInfo> MethodInfos = [];


    public static void Invoke()
    {
        foreach (MethodInfo methodInfo in MethodInfos)
            methodInfo.Invoke(null, null);
    }


    public static void FindAll()
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        List<(MethodInfo, int)> attribMethods = new();
        foreach (Assembly assembly in assemblies)
        {
            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (MethodInfo method in methods)
                {
                    OnApplicationLoadAttribute? attribute = method.GetCustomAttribute<OnApplicationLoadAttribute>();
                    if (attribute != null)
                        attribMethods.Add((method, attribute._order));
                }
            }
        }

        IOrderedEnumerable<(MethodInfo, int)> ordered = attribMethods.OrderBy(x => x.Item2);
        foreach ((MethodInfo, int) attribMethod in ordered)
            MethodInfos.Add(attribMethod.Item1);
    }
}