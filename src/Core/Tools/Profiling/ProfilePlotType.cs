namespace KorpiEngine.Tools;

public enum ProfilePlotType
{
    /// <summary>
    /// Values will be displayed as plain numbers.
    /// </summary>
    Number = 0,

    /// <summary>
    /// Treats the values as memory sizes. Will display kilobytes, megabytes, etc.
    /// </summary>
    Memory = 1,

    /// <summary>
    /// Values will be displayed as percentage (with value 100 being equal to 100%).
    /// </summary>
    Percentage = 2
}