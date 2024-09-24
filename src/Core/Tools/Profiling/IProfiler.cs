﻿using System.Runtime.CompilerServices;
using KorpiEngine.Mathematics;

namespace KorpiEngine.Tools;

public interface IProfiler
{
    /// <summary>
    /// Begins a new <see cref="TracyProfilerZone"/> and returns the handle to that zone. Time
    /// spent inside a zone is calculated and shown in the profiler. A zone is
    /// ended when <see cref="TracyProfilerZone.Dispose"/> is called either automatically via 
    /// disposal scope rules or by calling it manually.
    /// </summary>
    /// <param name="zoneName">A custom name for this zone.</param>
    /// <param name="active">Is the zone active? An inactive zone won't be shown in the profiler.</param>
    /// <param name="color">A color code that will be used to color the zone in the profiler.</param>
    /// <param name="text">Arbitrary text associated with this zone.</param>
    /// <param name="lineNumber">
    /// The source code line number that this zone begins at.
    /// If this param is not explicitly assigned the value will be provided by <see cref="CallerLineNumberAttribute"/>.
    /// </param>
    /// <param name="filePath">
    /// The source code file path that this zone begins at.
    /// If this param is not explicitly assigned the value will be provided by <see cref="CallerFilePathAttribute"/>.
    /// </param>
    /// <param name="memberName">
    /// The source code member name that this zone begins at.
    /// If this param is not explicitly assigned the value will be provided by <see cref="CallerMemberNameAttribute"/>.
    /// </param>
    /// <returns>A handle to the newly created zone.</returns>
    public IProfilerZone BeginZone(
        string? zoneName = null,
        bool active = true,
        ColorRGB color = default,
        string? text = null,
        [CallerLineNumber] int lineNumber = 0,
        [CallerFilePath] string? filePath = null,
        [CallerMemberName] string? memberName = null);
    
    
    /// <summary>
    /// Configure how plotted values are displayed in the profiler.
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
    /// A color code that will be used to color the plot in the profiler.
    /// </param>
    public void PlotConfig(string name, ProfilePlotType type = ProfilePlotType.Number, bool step = false, bool fill = true, ColorRGB color = default);
    
    
    /// <summary>
    /// Add a <see langword="double"/> value to a plot.
    /// </summary>
    public void Plot(string name, double val);
    
    
    /// <summary>
    /// Add a <see langword="int"/> value to a plot.
    /// </summary>
    public void Plot(string name, int val);


    /// <summary>
    /// Add a <see langword="float"/> value to a plot.
    /// </summary>
    public void Plot(string name, float val);
    
    
    /// <summary>
    /// Emit the top-level frame marker.
    /// Should be called at the end of each frame.
    /// </summary>
    public void EndFrame();
}