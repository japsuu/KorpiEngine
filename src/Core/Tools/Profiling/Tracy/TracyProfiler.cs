using System.Runtime.CompilerServices;
using bottlenoselabs.C2CS.Runtime;
using Tracy;

namespace KorpiEngine.Tools;

public class TracyProfiler : IProfiler
{
    public IProfilerZone BeginZone(
        string? zoneName = null,
        bool active = true,
        uint color = 0,
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

        if (color != 0)
            PInvoke.TracyEmitZoneColor(context, color);

        if (text == null)
            return new TracyProfilerZone(context);
        
        using CString textstr = GetCString(text, out ulong textln);
        PInvoke.TracyEmitZoneText(context, textstr, textln);

        return new TracyProfilerZone(context);
    }


    public void PlotConfig(string name, ProfilePlotType type = ProfilePlotType.Number, bool step = false, bool fill = true, uint color = 0)
    {
        using CString namestr = GetCString(name, out ulong _);
        PInvoke.TracyEmitPlotConfig(namestr, (int)type, step ? 1 : 0, fill ? 1 : 0, color);
    }


    public void Plot(string name, double val)
    {
        using CString namestr = GetCString(name, out ulong _);
        PInvoke.TracyEmitPlot(namestr, val);
    }


    public void Plot(string name, int val)
    {
        using CString namestr = GetCString(name, out ulong _);
        PInvoke.TracyEmitPlotInt(namestr, val);
    }


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
}