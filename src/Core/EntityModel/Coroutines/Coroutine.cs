using System.Collections;

namespace KorpiEngine.Core.EntityModel.Coroutines;

/// <summary>
/// Represents an instruction that can be executed over multiple frames.
/// </summary>
public sealed class Coroutine
{
    /// <summary>
    /// True if the coroutine has finished executing, false otherwise.
    /// </summary>
    internal bool IsDone { get; private set; }
    
    /// <summary>
    /// The instruction to execute.
    /// </summary>
    private readonly Stack<IEnumerator> _instructions = [];


    internal Coroutine(IEnumerator routine)
    {
        _instructions.Push(routine);
    }


    internal void Run(CoroutineUpdateStage stage)
    {
        // If there are no instructions left, the coroutine is finished.
        if (_instructions.Count == 0)
        {
            IsDone = true;
            return;
        }

        IEnumerator instruction = _instructions.Peek();
        
        // If the current instruction is a yield instruction, handle it.
        switch (instruction.Current)
        {
            case WaitForEndOfFrame when stage != CoroutineUpdateStage.EndOfFrame:
            case WaitForFixedUpdate when stage != CoroutineUpdateStage.FixedUpdate:
                return;
        }

        // If the current instruction has no more steps, pop it off the stack.
        if (!instruction.MoveNext())
        {
            _instructions.Pop();
            return;
        }

        // If the current instruction is another coroutine, push it onto the stack.
        if (instruction.Current is IEnumerator next && instruction != next)
            _instructions.Push(next);
    }
}