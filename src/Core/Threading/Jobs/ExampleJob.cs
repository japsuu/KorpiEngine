namespace KorpiEngine.Threading;

/// <summary>
/// Test job with an integer result equal to the product of 7 and 6.
/// </summary>
public class ExampleJob : KorpiJob<int>
{
    public override float GetPriority() => WorkItemPriority.NORMAL;


    public override void Execute()
    {
        // Do some work.
        int result = 7 * 6;

        // Set the result of the job.
        SetResult(result);
    }
}