using System.Runtime.CompilerServices;
using bottlenoselabs.C2CS.Runtime;
using KorpiEngine.Mathematics;
using Tracy;

namespace KorpiEngine.Tools;

public class TracyProfiler : IProfiler
{
    public IProfilerZone BeginZone(
        string? zoneName = null,
        bool active = true,
        ColorRGB color = default,
        string? text = null,
        [CallerLineNumber] int lineNumber = 0,
        [CallerFilePath] string? filePath = null,
        [CallerMemberName] string? memberName = null)
    {
        using CString filestr = GetCString(filePath, out ulong fileln);
        using CString memberstr = GetCString(memberName, out ulong memberln);
        using CString namestr = GetCString(zoneName, out ulong nameln);
        ulong srcLocId = PInvoke.TracyAllocSrclocName((uint)lineNumber, filestr, fileln, memberstr, memberln, namestr, nameln);
        PInvoke.TracyCZoneCtx context = PInvoke.TracyEmitZoneBeginAlloc(srcLocId, active ? 1 : 0);
        
        // Convert the color to an RRGGBB uint
        uint colorCode = GetColorCode(color);

        if (colorCode != 0)
            PInvoke.TracyEmitZoneColor(context, colorCode);

        if (text == null)
            return new TracyProfilerZone(context);
        
        using CString textstr = GetCString(text, out ulong textln);
        PInvoke.TracyEmitZoneText(context, textstr, textln);

        return new TracyProfilerZone(context);
    }


    /// <summary>
    /// Configure how Tracy will display plotted values.
    /// </summary>
    /// <param name="name">
    /// Name of the plot to configure. Each <paramref name="name"/> represents a unique plot.
    /// </param>
    /// <param name="type">
    /// Changes how the values in the plot are presented by the profiler.
    /// </param>
    /// <param name="step">
    /// Determines whether the plot will be displayed as a staircase or will smoothly change between plot points
    /// </param>
    /// <param name="fill">
    /// If <see langword="false"/> the the area below the plot will not be filled with a solid color.
    /// </param>
    /// <param name="color">
    /// A color code that Tracy will use to color the plot in the profiler.
    /// </param>
    public void PlotConfig(string name, ProfilePlotType type = ProfilePlotType.Number, bool step = false, bool fill = true, ColorRGB color = default)
    {
        uint colorCode = GetColorCode(color);
        using CString namestr = GetCString(name, out ulong _);
        PInvoke.TracyEmitPlotConfig(namestr, (int)type, step ? 1 : 0, fill ? 1 : 0, colorCode);
    }


    /// <summary>
    /// Add a <see langword="double"/> value to a plot.
    /// </summary>
    public void Plot(string name, double val)
    {
        using CString namestr = GetCString(name, out ulong _);
        PInvoke.TracyEmitPlot(namestr, val);
    }


    /// <summary>
    /// Add a <see langword="int"/> value to a plot.
    /// </summary>
    public void Plot(string name, int val)
    {
        using CString namestr = GetCString(name, out ulong _);
        PInvoke.TracyEmitPlotInt(namestr, val);
    }


    /// <summary>
    /// Add a <see langword="float"/> value to a plot.
    /// </summary>
    public void Plot(string name, float val)
    {
        using CString namestr = GetCString(name, out ulong _);
        PInvoke.TracyEmitPlotFloat(namestr, val);
    }


    /// <summary>
    /// Emit the top-level frame marker.
    /// </summary>
    /// <remarks>
    /// Tracy Cpp API and docs refer to this as the <c>FrameMark</c> macro.
    /// </remarks>
    public void EndFrame()
    {
        PInvoke.TracyEmitFrameMark(null);
    }


    /// <summary>
    /// Creates a <seealso cref="CString"/> for use by Tracy. Also returns the
    /// length of the string for interop convenience.
    /// </summary>
    internal static CString GetCString(string? fromString, out ulong clength)
    {
        if (fromString == null)
        {
            clength = 0;
            return new CString(0);
        }

        clength = (ulong)fromString.Length;
        return CString.FromString(fromString);
    }
    
    
    internal static uint GetColorCode(ColorRGB color)
    {
        uint colorCode = 0;
        byte r = color.R;
        byte g = color.G;
        byte b = color.B;
        colorCode |= (uint)r << 16;
        colorCode |= (uint)g << 8;
        colorCode |= b;
        return colorCode;
    }
}