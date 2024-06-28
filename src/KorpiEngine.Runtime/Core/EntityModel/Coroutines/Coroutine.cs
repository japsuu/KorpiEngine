using System.Collections;

namespace KorpiEngine.Core.EntityModel.Coroutines;

public sealed class Coroutine : YieldInstruction
{
    internal bool IsDone { get; private set; }
    internal readonly IEnumerator Enumerator;


    internal Coroutine(IEnumerator routine)
    {
        Enumerator = routine;
    }


    private bool CanRun
    {
        get
        {
            object? current = Enumerator.Current;

            return current switch
            {
                Coroutine dep => dep.IsDone,
                WaitForSeconds wait => wait.Duration <= Time.TotalTime,
                _ => true
            };
        }
    }


    internal void Run()
    {
        if (CanRun)
            IsDone = !Enumerator.MoveNext();
    }
}