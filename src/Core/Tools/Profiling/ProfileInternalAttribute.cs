using System.Diagnostics;
using System.Runtime.CompilerServices;
using AspectInjector.Broker;

namespace KorpiEngine.Tools;

[Conditional("KORPI_PROFILE")]
[Injection(typeof(ProfileInternalAspect))]
[AttributeUsage(AttributeTargets.Method)]
internal sealed class ProfileInternalAttribute(
    string? zoneName = null,
    uint color = 0x808080FF,
    [CallerLineNumber] int lineNumber = 0,
    [CallerFilePath] string? filePath = null) : Attribute
{
    public readonly string? ZoneName = zoneName;
    public readonly uint Color = color;
    public readonly int LineNumber = lineNumber;
    public readonly string? FilePath = filePath;
}

[Aspect(Scope.Global)]
public class ProfileInternalAspect
{
    [Advice(Kind.Around, Targets = Target.Method)]
    public object OnInvoke(
        [Argument(Source.Name)] string methodName,
        [Argument(Source.Type)] Type type,
        [Argument(Source.Arguments)] object[] arguments,
        [Argument(Source.Target)] Func<object[], object> method,
        [Argument(Source.Triggers)] Attribute[] triggers)
    {
        // Retrieve the ProfileInternalAttribute from the triggers
        ProfileInternalAttribute? profileTrigger = triggers.OfType<ProfileInternalAttribute>().FirstOrDefault();

        // If no profiling is required, just proceed with the method call
        if (profileTrigger == null)
            return method(arguments);

        string zoneName = profileTrigger.ZoneName ?? $"{type.Name}.{methodName}";
        IProfilerZone zone = Application.Profiler.BeginZone(
            zoneName,
            true,
            profileTrigger.Color,
            null,
            profileTrigger.LineNumber + 1,  // Add 1 to the line number to point to the actual method call instead of the attribute
            profileTrigger.FilePath,
            methodName);

        try
        {
            // Execute the original method
            return method(arguments);
        }
        finally
        {
            // Dispose of the profiling zone
            zone.Dispose();
        }
    }
}