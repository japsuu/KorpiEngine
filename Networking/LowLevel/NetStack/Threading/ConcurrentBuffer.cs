/*
 *  Copyright (c) 2018 Alexander Nikitin, Stanislav Denisov
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in all
 *  copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 *  SOFTWARE.
 */

using System.Runtime.InteropServices;

namespace KorpiEngine.Networking.LowLevel.NetStack.Threading;

[StructLayout(LayoutKind.Explicit, Size = 192)]
public sealed class ConcurrentBuffer
{
    [FieldOffset(0)]
    private readonly Cell[] _buffer;

    [FieldOffset(8)]
    private readonly int _bufferMask;

    [FieldOffset(64)]
    private int _enqueuePosition;

    [FieldOffset(128)]
    private int _dequeuePosition;

    public int Count => _enqueuePosition - _dequeuePosition;


    public ConcurrentBuffer(int bufferSize)
    {
        if (bufferSize < 2)
            throw new ArgumentException("Buffer size should be greater than or equal to two");

        if ((bufferSize & (bufferSize - 1)) != 0)
            throw new ArgumentException("Buffer size should be a power of two");

        _bufferMask = bufferSize - 1;
        _buffer = new Cell[bufferSize];

        for (int i = 0; i < bufferSize; i++)
            _buffer[i] = new Cell(i, null);

        _enqueuePosition = 0;
        _dequeuePosition = 0;
    }


    public void Enqueue(object item)
    {
        while (true)
        {
            if (TryEnqueue(item))
                break;

            Thread.SpinWait(1);
        }
    }


    public bool TryEnqueue(object item)
    {
        do
        {
            Cell[] buffer = _buffer;
            int position = _enqueuePosition;
            int index = position & _bufferMask;
            Cell cell = buffer[index];

            if (cell.Sequence == position && Interlocked.CompareExchange(ref _enqueuePosition, position + 1, position) == position)
            {
                buffer[index].Element = item;

#if NET_4_6 || NET_STANDARD_2_0
                Volatile.Write(ref buffer[index].Sequence, position + 1);
#else
						Thread.MemoryBarrier();
						buffer[index].Sequence = position + 1;
#endif

                return true;
            }

            if (cell.Sequence < position)
                return false;
        }

        while (true);
    }


    public object Dequeue()
    {
        while (true)
        {
            object element;

            if (TryDequeue(out element))
                return element;
        }
    }


    public bool TryDequeue(out object result)
    {
        do
        {
            Cell[] buffer = _buffer;
            int bufferMask = _bufferMask;
            int position = _dequeuePosition;
            int index = position & bufferMask;
            Cell cell = buffer[index];

            if (cell.Sequence == position + 1 && Interlocked.CompareExchange(ref _dequeuePosition, position + 1, position) == position)
            {
                result = cell.Element;
                buffer[index].Element = null;

#if NET_4_6 || NET_STANDARD_2_0
                Volatile.Write(ref buffer[index].Sequence, position + bufferMask + 1);
#else
						Thread.MemoryBarrier();
						buffer[index].Sequence = position + bufferMask + 1;
#endif

                return true;
            }

            if (cell.Sequence < position + 1)
            {
                result = default;

                return false;
            }
        }

        while (true);
    }


    [StructLayout(LayoutKind.Explicit, Size = 16)]
    private struct Cell
    {
        [FieldOffset(0)]
        public int Sequence;

        [FieldOffset(8)]
        public object Element;


        public Cell(int sequence, object element)
        {
            Sequence = sequence;
            Element = element;
        }
    }
}