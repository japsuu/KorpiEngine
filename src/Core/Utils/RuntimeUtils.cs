using System.Reflection;
using System.Text;
using KorpiEngine.Serialization;

namespace KorpiEngine.Utils;

internal static class RuntimeUtils
{
    public static Type? FindType(string qualifiedTypeName)
    {
        Type? t = Type.GetType(qualifiedTypeName);

        if (t != null)
        {
            return t;
        }

        foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            t = asm.GetType(qualifiedTypeName);
            if (t != null)
                return t;
        }

        return null;
    }


    public static FieldInfo[] GetSerializableFields(object target)
    {
        FieldInfo[] fields = GetAllFields(target.GetType()).ToArray();

        // Only allow public or ones with SerializeField
        fields = fields.Where(
            field => (field.IsPublic || field.GetCustomAttribute<SerializeFieldAttribute>() != null) &&
                     field.GetCustomAttribute<SerializeIgnoreAttribute>() == null).ToArray();

        // Remove Public NonSerialized fields
        fields = fields.Where(field => !field.IsPublic || field.GetCustomAttribute<NonSerializedAttribute>() == null).ToArray();
        return fields;
    }


    public static IEnumerable<FieldInfo> GetAllFields(Type? t)
    {
        if (t == null)
            return [];

        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                                   BindingFlags.Instance | BindingFlags.DeclaredOnly;

        return t.GetFields(flags).Concat(GetAllFields(t.BaseType));
    }


    public static object? GetValue(this MemberInfo member, object? target)
    {
        if (member is PropertyInfo prop)
            return prop.GetValue(target);
        if (member is FieldInfo field)
            return field.GetValue(target);
        return null;
    }


    public static void SetValue(this MemberInfo member, object? target, object? value)
    {
        if (member is PropertyInfo prop)
            prop.SetValue(target, value);
        else if (member is FieldInfo field)
            field.SetValue(target, value);
    }


    public static string Prettify(string label)
    {
        if (label.StartsWith('_'))
            label = label[1..];

        // Use a StringBuilder to avoid modifying the original string in the loop
        StringBuilder result = new(label.Length * 2);
        result.Append(char.ToUpper(label[0]));

        // Add space before each Capital letter (except the first)
        for (int i = 1; i < label.Length; i++)
        {
            if (char.IsUpper(label[i]))
            {
                result.Append(' '); // Add space
            }

            // Append the current character
            result.Append(label[i]); // Append the current uppercase character
        }

        return Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(result.ToString());
    }


    public static IEnumerable<Type> GetTypesWithAttribute<T>()
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly assembly in assemblies)
        {
            foreach (Type type in assembly.GetTypes())
                if (type.GetCustomAttributes(typeof(T), true).Length > 0)
                    yield return type;
        }
    }
}