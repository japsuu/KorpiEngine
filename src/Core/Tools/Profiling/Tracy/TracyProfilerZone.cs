using bottlenoselabs.C2CS.Runtime;
using Tracy;

namespace KorpiEngine.Tools;

public readonly struct TracyProfilerZone : IProfilerZone
{
    public readonly PInvoke.TracyCZoneCtx Context;
    
    public uint Id => Context.Data.Id;
    public int Active => Context.Data.Active;


    internal TracyProfilerZone(PInvoke.TracyCZoneCtx context)
    {
        Context = context;
    }


    public void EmitName(string name)
    {
        using CString namestr = TracyProfiler.GetCString(name, out ulong nameln);
        PInvoke.TracyEmitZoneName(Context, namestr, nameln);
    }


    public void EmitColor(uint color)
    {
        PInvoke.TracyEmitZoneColor(Context, color);
    }


    public void EmitText(string text)
    {
        using CString textstr = TracyProfiler.GetCString(text, out ulong textln);
        PInvoke.TracyEmitZoneText(Context, textstr, textln);
    }


    public void Dispose()
    {
        PInvoke.TracyEmitZoneEnd(Context);
    }
}