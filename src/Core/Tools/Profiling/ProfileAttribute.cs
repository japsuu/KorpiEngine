using System.Diagnostics;
using System.Runtime.CompilerServices;
using AspectInjector.Broker;

namespace KorpiEngine.Tools;

/// <summary>
/// An attribute that can be applied to methods to profile them using the configured profiler.
/// </summary>
/// <param name="zoneName">The name of the profiling zone. If not provided, the method name will be used.</param>
/// <param name="color">The RRGGBB color of the profiling zone.</param>
/// <param name="lineNumber">The line number of the method call. Automatically provided by the compiler.</param>
/// <param name="filePath">The file path of the method call. Automatically provided by the compiler.</param>
[Conditional("KORPI_PROFILE")]
[Injection(typeof(ProfileAspect))]
[AttributeUsage(AttributeTargets.Method)]
public sealed class ProfileAttribute(
    string? zoneName = null,
    uint color = 0,
    [CallerLineNumber] int lineNumber = 0,
    [CallerFilePath] string? filePath = null) : Attribute
{
    public readonly string? ZoneName = zoneName;
    public readonly uint Color = color;
    public readonly int LineNumber = lineNumber;
    public readonly string? FilePath = filePath;
}

[Aspect(Scope.Global)]
public class ProfileAspect
{
    [Advice(Kind.Around, Targets = Target.Method)]
    public object OnInvoke(
        [Argument(Source.Name)] string methodName,
        [Argument(Source.Type)] Type type,
        [Argument(Source.Arguments)] object[] arguments,
        [Argument(Source.Target)] Func<object[], object> method,
        [Argument(Source.Triggers)] Attribute[] triggers)
    {
        // Retrieve the ProfileAttribute from the triggers
        ProfileAttribute? profileTrigger = triggers.OfType<ProfileAttribute>().FirstOrDefault();

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