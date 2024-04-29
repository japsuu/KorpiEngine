using System.Reflection;

namespace KorpiEngine.Core.API;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class OnAssemblyUnloadAttribute : Attribute
{
    public OnAssemblyUnloadAttribute()
    {
    }


    private static readonly List<MethodInfo> MethodInfos = new();


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
                    IEnumerable<OnAssemblyUnloadAttribute> attributes = method.GetCustomAttributes<OnAssemblyUnloadAttribute>();
                    if (attributes.Count() > 0)
                        MethodInfos.Add(method);
                }
            }
        }
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class OnAssemblyLoadAttribute : Attribute
{
    private readonly int _order;
    private static List<MethodInfo> methodInfos = new();


    public OnAssemblyLoadAttribute(int order = 0)
    {
        _order = order;
    }


    public static void Invoke()
    {
        foreach (MethodInfo methodInfo in methodInfos)
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
                    OnAssemblyLoadAttribute? attribute = method.GetCustomAttribute<OnAssemblyLoadAttribute>();
                    if (attribute != null)
                        attribMethods.Add((method, attribute._order));
                }
            }
        }

        IOrderedEnumerable<(MethodInfo, int)> ordered = attribMethods.OrderBy(x => x.Item2);
        foreach ((MethodInfo, int) attribMethod in ordered)
            methodInfos.Add(attribMethod.Item1);
    }
}