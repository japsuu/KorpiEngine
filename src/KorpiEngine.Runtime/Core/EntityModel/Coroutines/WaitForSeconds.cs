namespace KorpiEngine.Core.EntityModel.Coroutines;

public sealed class WaitForSeconds(float seconds) : YieldInstruction
{
    public readonly double Duration = Time.TotalTime + seconds;
}